using System;
using System.Collections.Generic;
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
    [DefaultExecutionOrder(-9000)]
    public abstract class HostManager : MonoBehaviour
    {
        private const string DEFAULT_INJECTION_METHOD_NAME = "AwakeServices";

        [Tooltip("The method name which will be used to inject Host services. Works like constructor injection in ordinary classes, but since MonoBehaviours can't use constructors, this method will be used instead")]
        [SerializeField] private string servicesInjectionMethodName = DEFAULT_INJECTION_METHOD_NAME;

        [Tooltip("If false, " + nameof(BuildManually) + "() method needs to be called to build the host")]
        [SerializeField] private bool buildOnAwake = true;

        [SerializeField] private bool bindWithUnityLifetime = true;

        [Header("Logging")]
        [Tooltip("Use Unity Debug.Log to print log messages")]
        [SerializeField] private bool logToUnity = true;

        [Tooltip("Will suppress host lifetime messages logging if set to true")]
        [SerializeField] private bool suppressStatusMessages;

        [Header("Events")]
        [SerializeField] private UnityEvent hostBuilt;
        [SerializeField] private UnityEvent hostStarted;
        [SerializeField] private UnityEvent hostStopping;
        [SerializeField] private UnityEvent hostStopped;

        private IHostBuilder _hostBuilder;
        protected IHost host;

        private bool _isBuilt;
        private bool _isStarted;

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected abstract void ConfigureAppConfiguration(IConfigurationBuilder builder);
        protected abstract void ConfigureLogging(ILoggingBuilder builder);
        protected abstract void ConfigureServices(IServiceCollection services);
        protected abstract void ConfigureMonoBehaviours(IMonoBehaviourServiceCollectionBuilder services);

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
            _hostBuilder = UnityHost.CreateDefaultBuilder(servicesInjectionMethodName);
            _hostBuilder.ConfigureLogging(ConfigureLogging);

            if (logToUnity)
                _hostBuilder.ConfigureLogging(builder => builder.AddUnityLogger());

            _hostBuilder.ConfigureAppConfiguration(ConfigureAppConfiguration);
            _hostBuilder.ConfigureServices(services => { services.SuppressStatusMessages(suppressStatusMessages); });

            _hostBuilder.ConfigureServices(ConfigureServices);
            _hostBuilder.ConfigureMonoBehaviours(ConfigureMonoBehaviours);

            OnAwake();

            if (buildOnAwake)
                BuildHost();
        }

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

        #region Public API methods

        private void BuildHost()
        {
            try
            {
                _isBuilt = true;
                host = _hostBuilder.Build();

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

                if (hostStarted != null)
                    lifetime.ApplicationStarted.Register(hostStarted.Invoke);

                if (hostStopping != null)
                    lifetime.ApplicationStopping.Register(hostStopping.Invoke);

                if (hostStopped != null)
                    lifetime.ApplicationStopped.Register(hostStopped.Invoke);

                if (bindWithUnityLifetime)
                {
                    Application.wantsToQuit += () =>
                    {
                        lifetime.StopApplication();
                        return true;
                    };
                    lifetime.ApplicationStopped.Register(() =>
                    {
                        Debug.Log("Application stopped");
#if UNITY_EDITOR
                        EditorApplication.isPlaying = false;
#endif
                        Application.Quit();
                    });
                }

                hostBuilt?.Invoke();
            }
            catch (Exception)
            {
                _isBuilt = false;
                throw;
            }
        }

        #endregion

        #region Host control by Unity events

        public async void Start()
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

        private async void OnDisable()
        {
            if (host != null && _isBuilt && _isStarted)
            {
                await host.StopAsync();
            }
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
                await host.StartAsync();
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
        }
#endif
    }
}