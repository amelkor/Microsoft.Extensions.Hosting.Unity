using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Unity.Logging;
using Microsoft.Extensions.Logging;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Microsoft.Extensions.Hosting.Unity
{
    /// <summary>
    /// Base class for using <see cref="Microsoft.Extensions.Hosting"/> in Unity.
    /// </summary>
    public abstract class HostManager : MonoBehaviour
    {
        private const string DEFAULT_HOST_APPLICATION_NAME = "Unity Application";
        private const string DEFAULT_INJECTION_METHOD_NAME = "AwakeServices";
        private const int DEFAULT_GRACEFUL_SHUTDOWN_TIMEOUT_MS = 5000;
        private const int MIN_GRACEFUL_SHUTDOWN_TIMEOUT_MS = 100;

        [Tooltip("Application name for the HostingEnvironment")]
        [SerializeField] private string hostApplicationName;

        [Tooltip("The method name which will be used to inject Host services. Works like constructor injection in ordinary classes, but since MonoBehaviours can't use constructors, this method will be used instead")]
        [SerializeField] private string servicesInjectionMethodName = DEFAULT_INJECTION_METHOD_NAME;

        [Tooltip("If false, " + nameof(BuildManually) + "() method needs to be called to build the host")]
        [SerializeField] private bool buildOnAwake = true;

        [SerializeField] private bool controlUnityLifetime;

        [Tooltip("Timeout for the host to shutdown gracefully")]
        [SerializeField] private int gracefulShutduwnTimeoutMs = DEFAULT_GRACEFUL_SHUTDOWN_TIMEOUT_MS;

        [Header("Logging")]
        [Tooltip("Use Unity Debug.Log to print log messages")]
        [SerializeField] private bool logToUnity = true;
        
        [Tooltip("Minimum log level")]
        [SerializeField] private LogLevel logLevel = LogLevel.Information;

        [Tooltip("Will suppress host lifetime messages logging if set to true")]
        [SerializeField] private bool suppressStatusMessages;

        [Header("Options")]
        [Tooltip("Optional command line arguments to pass into the Host")]
        [SerializeField] private string[] cmdArguments;

        [Header("Events")]
        [SerializeField] private UnityEvent hostBuilt;
        [SerializeField] private UnityEvent hostStarted;
        [SerializeField] private UnityEvent hostStopping;
        [SerializeField] private UnityEvent hostStopped;

        private IHostBuilder _hostBuilder;

        // ReSharper disable once MemberCanBePrivate.Global
        protected IHost host;

        private CancellationTokenSource _cts;

        private bool _isBuilt;
        private bool _isStarted;

        /// <summary>
        /// Override to use Unity's Awake() method.
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        /// <summary>
        /// Override to use Unity's Start() method.
        /// </summary>
        protected virtual void OnStart()
        {
        }
        
        /// <summary>
        /// Allows to add an extra step to the host builder before it's built.
        /// </summary>
        /// <param name="hostBuilder">Host builder.</param>
        protected virtual void ConfigureExtra(IHostBuilder hostBuilder)
        {
        }

        protected abstract void ConfigureAppConfiguration(IConfigurationBuilder builder);
        protected abstract void ConfigureLogging(ILoggingBuilder builder);
        protected abstract void ConfigureServices(IServiceCollection services);
        protected abstract void ConfigureUnityObjects(IUnityObjectServiceCollectionBuilder services);
        

        // ReSharper disable once MemberCanBeProtected.Global
        /// <summary>
        /// The service provider of the Host.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the Host is not built yet.</exception>
        public IServiceProvider Services
        {
            get
            {
                if (!_isBuilt)
                    throw new InvalidOperationException("Host must be built before accessing Services");

                return host.Services;
            }
        }

        private void Awake()
        {
            _hostBuilder = UnityHost.CreateDefaultBuilder(servicesInjectionMethodName, cmdArguments);
            _hostBuilder.ConfigureLogging(builder => builder.SetMinimumLevel(logLevel));
            _hostBuilder.ConfigureLogging(ConfigureLogging);

            if (logToUnity)
                _hostBuilder.ConfigureLogging(builder => builder.AddUnityLogger());

            _hostBuilder.ConfigureAppConfiguration(ConfigureAppConfiguration);
            _hostBuilder.ConfigureServices(services => { services.SuppressStatusMessages(suppressStatusMessages); });

            _hostBuilder.ConfigureServices(ConfigureServices);
            _hostBuilder.ConfigureMonoBehaviours(ConfigureUnityObjects);
            ConfigureExtra(_hostBuilder);
            OnAwake();

            if (buildOnAwake)
                BuildHost();
        }

        #region Public API methods

        /// <summary>
        /// Build the <see cref="IHost"/> if <see cref="buildOnAwake"/> set to false.
        /// </summary>
        public void BuildManually()
        {
            if (_isBuilt)
            {
                Debug.LogWarning("Host is already built");
                return;
            }

            if (buildOnAwake)
            {
                Debug.LogWarning(nameof(buildOnAwake) + " should be set to false to use " + nameof(BuildManually) + " method");
                return;
            }

            BuildHost();
        }

        /// <summary>
        /// Start the Host manually. Call this after <see cref="BuildManually"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the Host is not build yet.</exception>
        public void StartManually()
        {
            if (!_isBuilt)
                throw new InvalidOperationException("Host must be build before start.");

            StartHost();
        }

        /// <summary>
        /// Start the Host manually.
        /// </summary>
        /// <exception cref="InvalidOperationException">when the Host was not build and started yet.</exception>
        public void StopManually()
        {
            if (!_isBuilt && !_isStarted)
                throw new InvalidOperationException("Host must be build and started before stop.");

            if (host != null)
            {
                _cts?.Cancel();
            }
        }

        #endregion

        private void BuildHost()
        {
            try
            {
                _isBuilt = true;
                host = _hostBuilder.Build();

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

                // host events occur only once per Host lifetime so remove them after invocation
                
                if (hostStarted != null)
                    lifetime.ApplicationStarted.Register(() =>
                    {
                        hostStarted.Invoke();
                        hostStarted.RemoveAllListeners();
                    });

                if (hostStopping != null)
                    lifetime.ApplicationStopping.Register(() =>
                    {
                        hostStopping.Invoke();
                        hostStopping.RemoveAllListeners();
                    });

                if (hostStopped != null)
                    lifetime.ApplicationStopped.Register(() =>
                    {
                        hostStopped.Invoke();
                        hostStopped.RemoveAllListeners();
                    });

                if (controlUnityLifetime)
                {
                    lifetime.ApplicationStopped.Register(() =>
                    {
                        Debug.Log("Application stopped");
#if UNITY_EDITOR
                        EditorApplication.isPlaying = false;
#endif
                        Application.Quit();
                    });
                }

                Application.quitting += () =>
                {
                    _cts?.Cancel();
                    using var cts = new CancellationTokenSource(gracefulShutduwnTimeoutMs);
                    host.StopAsync(cts.Token)
                        .GetAwaiter()
                        .GetResult();
                    host?.Dispose();
                };

                hostBuilt?.Invoke();
                hostBuilt?.RemoveAllListeners();
            }
            catch (Exception)
            {
                _isBuilt = false;
                throw;
            }
        }

        #region Host control by Unity events

        private async void Start()
        {
            OnStart();

            if (_isBuilt)
            {
                await StartHostAsync();
            }
            else if (buildOnAwake && !_isBuilt)
            {
                Debug.LogWarning("Host can not be started. Build on Awake is true, but the host wasn't built");
            }
        }

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }

        #endregion

        #region private methods

        private void StartHost()
        {
            try
            {
                if (_isStarted)
                {
                    Debug.LogWarning("The Host is already started");
                    return;
                }

                _isStarted = true;
                host.Start();
            }
            catch (Exception)
            {
                _isStarted = false;
                throw;
            }
        }

        private async Task StartHostAsync()
        {
            try
            {
                if (_isStarted)
                {
                    Debug.LogWarning("The Host is already started");
                    return;
                }

                _isStarted = true;
                await host.StartAsync(_cts.Token);
            }
            catch (Exception)
            {
                _isStarted = false;
                throw;
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(servicesInjectionMethodName))
                servicesInjectionMethodName = DEFAULT_INJECTION_METHOD_NAME;

            if (string.IsNullOrEmpty(hostApplicationName))
            {
                hostApplicationName = string.IsNullOrEmpty(Application.productName)
                    ? DEFAULT_HOST_APPLICATION_NAME
                    : Application.productName;
            }

            if (gracefulShutduwnTimeoutMs <= MIN_GRACEFUL_SHUTDOWN_TIMEOUT_MS)
                gracefulShutduwnTimeoutMs = DEFAULT_GRACEFUL_SHUTDOWN_TIMEOUT_MS;
        }
#endif
    }
}