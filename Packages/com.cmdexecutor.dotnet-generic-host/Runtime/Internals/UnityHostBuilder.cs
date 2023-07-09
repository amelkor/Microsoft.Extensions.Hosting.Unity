using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Hosting.Unity
{
    internal class UnityHostBuilder : HostBuilder, IUnityHostBuilder
    {
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        private readonly List<Action<HostBuilderContext, IUnityObjectsConfigurator>> _configureDelegates = new List<Action<HostBuilderContext, IUnityObjectsConfigurator>>();
        private readonly UnityObjectsConfigurator _unityObjectsConfigurator;

        public UnityHostBuilder(string serviceInjectionMethodName)
        {
            _unityObjectsConfigurator = new UnityObjectsConfigurator(serviceInjectionMethodName);
        }

        /// <summary>
        /// Register <see cref="UnityEngine.MonoBehaviour"/> classes to this <see cref="IHost"/> services.
        /// </summary>
        /// <param name="configureDelegate">Configure action.</param>
        /// <returns></returns>
        public IUnityHostBuilder ConfigureUnityObjects(Action<HostBuilderContext, IUnityObjectsConfigurator> configureDelegate)
        {
            if (configureDelegate == null)
                throw new ArgumentNullException(nameof(configureDelegate));

            _configureDelegates.Add(configureDelegate);

            return this;
        }

        public new IHost Build()
        {
            ConfigureServices((context, services) => services.AddScoped<ScopeRoot>());
            ConfigureServices((context, services) =>
            {
                foreach (var configure in _configureDelegates)
                {
                    configure.Invoke(context, _unityObjectsConfigurator);
                }

                _unityObjectsConfigurator.Configure(context, services);
            });

            return BuildInternal();
        }

        private bool _hostBuilt;

        private IHost BuildInternal()
        {
            if (_hostBuilt)
            {
                throw new InvalidOperationException("Build can only be called once.");
            }

            _hostBuilt = true;

            // REVIEW: If we want to raise more events outside of these calls then we will need to
            // stash this in a field.
            using DiagnosticListener diagnosticListener = LogHostBuilding(this);

            InitializeHostConfiguration_Method.Invoke(this, default);
            InitializeHostingEnvironment_Method.Invoke(this, default);
            InitializeHostBuilderContext_Method.Invoke(this, default);
            InitializeAppConfiguration_Method.Invoke(this, default);
            InitializeServiceProvider();

            var appServices = _appServices_Field.GetValue(this);

            return (IHost)ResolveHost_Method.Invoke(this, new[] { appServices, diagnosticListener });
        }

        private void InitializeServiceProvider()
        {
            var services = new ServiceCollection();
            var hostBuilderContext = (HostBuilderContext)_hostBuilderContext_Field.GetValue(this);

            var configureServicesActions = (List<Action<HostBuilderContext, IServiceCollection>>)_configureServicesActions_Field.GetValue(this);
            var serviceProviderFactory = new IServiceFactoryAdapter_Wrapper(_serviceProviderFactory_Field.GetValue(this));
            var configureContainerActions = (IEnumerable)_configureContainerActions_Field.GetValue(this);
            
            PopulateServiceCollection(
                services,
                hostBuilderContext!,
                (HostingEnvironment)_hostingEnvironment_Field.GetValue(this)!,
                (PhysicalFileProvider)_defaultProvider_Field.GetValue(this)!,
                (IConfiguration)_appConfiguration_Field.GetValue(this)!,
                () => (IServiceProvider)_appServices_Field.GetValue(this)!);

            foreach (Action<HostBuilderContext, IServiceCollection> configureServicesAction in configureServicesActions)
            {
                configureServicesAction(hostBuilderContext!, services);
            }

            object containerBuilder = serviceProviderFactory.CreateBuilder(services);

            foreach (var containerAction in configureContainerActions)
            {
                new IConfigureContainerAdapter_Wrapper(containerAction).ConfigureContainer(hostBuilderContext!, containerBuilder);
            }

            _appServices_Field.SetValue(this, serviceProviderFactory.CreateServiceProvider(containerBuilder));
        }

        private readonly struct IServiceFactoryAdapter_Wrapper
        {
            private readonly object _target;

            public IServiceFactoryAdapter_Wrapper(object target) => _target = target;

            public object CreateBuilder(IServiceCollection services)
            {
                return CreateBuilder_Method.Invoke(_target, new[] { services });
            }

            public IServiceProvider CreateServiceProvider(object containerBuilder)
            {
                return (IServiceProvider)CreateServiceProvider_Method.Invoke(_target, new[] { containerBuilder });
            }

            private static readonly MethodInfo CreateBuilder_Method = typeof(HostingEnvironment).Assembly.GetType("Microsoft.Extensions.Hosting.Internal.IServiceFactoryAdapter").GetMethod("CreateBuilder");
            private static readonly MethodInfo CreateServiceProvider_Method = typeof(HostingEnvironment).Assembly.GetType("Microsoft.Extensions.Hosting.Internal.IServiceFactoryAdapter").GetMethod("CreateServiceProvider");
        }

        private readonly struct IConfigureContainerAdapter_Wrapper
        {
            private readonly object _target;

            public IConfigureContainerAdapter_Wrapper(object target) => this._target = target;

            public void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder)
            {
                ConfigureContainer_Method.Invoke(_target, new[] { hostContext, containerBuilder });
            }

            private static readonly MethodInfo ConfigureContainer_Method = typeof(HostingEnvironment).Assembly.GetType("Microsoft.Extensions.Hosting.Internal.IConfigureContainerAdapter").GetMethod("ConfigureContainer");
        }

        private static void PopulateServiceCollection(
            IServiceCollection services,
            HostBuilderContext hostBuilderContext,
            HostingEnvironment hostingEnvironment,
            PhysicalFileProvider defaultFileProvider,
            IConfiguration appConfiguration,
            Func<IServiceProvider> serviceProviderGetter)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddSingleton<IHostingEnvironment>(hostingEnvironment);
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddSingleton<IHostEnvironment>(hostingEnvironment);
            services.AddSingleton(hostBuilderContext);
            // register configuration as factory to make it dispose with the service provider
            services.AddSingleton(_ => appConfiguration);
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddSingleton(s => (IApplicationLifetime)s.GetRequiredService<IHostApplicationLifetime>());
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();

            services.AddSingleton<IHostLifetime, UnityLifetime>();

            services.AddSingleton<IHost>(_ =>
            {
                // We use serviceProviderGetter() instead of the _ parameter because these can be different given a custom IServiceProviderFactory.
                // We want the host to always dispose the IServiceProvider returned by the IServiceProviderFactory.
                // https://github.com/dotnet/runtime/issues/36060
                IServiceProvider appServices = serviceProviderGetter();
                return new MonoHost(appServices,
                    hostingEnvironment,
                    defaultFileProvider,
                    appServices.GetRequiredService<IHostApplicationLifetime>(),
                    appServices.GetRequiredService<ILogger<MonoHost>>(),
                    appServices.GetRequiredService<IHostLifetime>(),
                    appServices.GetRequiredService<IOptions<HostOptions>>());
            });
            services.AddOptions().Configure<HostOptions>(options =>
            {
                var timeoutSeconds = hostBuilderContext.Configuration["shutdownTimeoutSeconds"];
                if (!string.IsNullOrEmpty(timeoutSeconds) && int.TryParse(timeoutSeconds, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
                {
                    options.ShutdownTimeout = TimeSpan.FromSeconds(seconds);
                }
            });
            services.AddLogging();
        }

        #region inaccessible base members reflection

        private static readonly FieldInfo _appServices_Field = typeof(HostBuilder).GetField("_appServices", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _hostBuilderContext_Field = typeof(HostBuilder).GetField("_hostBuilderContext", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _hostingEnvironment_Field = typeof(HostBuilder).GetField("_hostingEnvironment", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _defaultProvider_Field = typeof(HostBuilder).GetField("_defaultProvider", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _appConfiguration_Field = typeof(HostBuilder).GetField("_appConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _serviceProviderFactory_Field = typeof(HostBuilder).GetField("_serviceProviderFactory", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _configureServicesActions_Field = typeof(HostBuilder).GetField("_configureServicesActions", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo _configureContainerActions_Field = typeof(HostBuilder).GetField("_configureContainerActions", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo InitializeHostConfiguration_Method = typeof(HostBuilder).GetMethod("InitializeHostConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo InitializeHostingEnvironment_Method = typeof(HostBuilder).GetMethod("InitializeHostingEnvironment", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo InitializeHostBuilderContext_Method = typeof(HostBuilder).GetMethod("InitializeHostBuilderContext", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo InitializeAppConfiguration_Method = typeof(HostBuilder).GetMethod("InitializeAppConfiguration", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo InitializeServiceProvider_Method = typeof(HostBuilder).GetMethod("InitializeServiceProvider", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo ResolveHost_Method = typeof(HostBuilder).GetMethod("ResolveHost", BindingFlags.Static | BindingFlags.NonPublic);

        #endregion

        #region inaccessible base methods duplication

        private const string HostBuildingDiagnosticListenerName = "Microsoft.Extensions.Hosting.Unity";
        private const string HostBuildingEventName = "HostBuilding";
        private const string HostBuiltEventName = "HostBuilt";

        private static DiagnosticListener LogHostBuilding(IHostBuilder hostBuilder)
        {
            var diagnosticListener = new DiagnosticListener(HostBuildingDiagnosticListenerName);

            if (diagnosticListener.IsEnabled() && diagnosticListener.IsEnabled(HostBuildingEventName))
            {
                diagnosticListener.Write(HostBuildingEventName, hostBuilder);
            }

            return diagnosticListener;
        }

        #endregion
    }
}