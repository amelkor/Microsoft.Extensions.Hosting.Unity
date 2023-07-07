using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting.Unity
{
    internal class MonoHostBuilder : IMonoHostBuilder
    {
        private readonly List<Action<HostBuilderContext, IMonoObjectsBuilder>> _configureMonoActions = new List<Action<HostBuilderContext, IMonoObjectsBuilder>>();
        private readonly UnityCollectionBuilder _unityCollectionBuilder;
        
        private readonly IHostBuilder _hostBuilder;

        public IDictionary<object, object> Properties => _hostBuilder.Properties;
        
        public MonoHostBuilder(IHostBuilder hostBuilder, string serviceInjectionMethodName)
        {
            _unityCollectionBuilder = new UnityCollectionBuilder(serviceInjectionMethodName);
            _hostBuilder = hostBuilder;
        }
        
        /// <summary>
        /// Register <see cref="UnityEngine.MonoBehaviour"/> classes to this <see cref="IHost"/> services.
        /// </summary>
        /// <param name="configureDelegate">Configure action.</param>
        /// <returns></returns>
        public IMonoHostBuilder ConfigureUnityObjects(Action<HostBuilderContext, IMonoObjectsBuilder> configureDelegate)
        {
            _configureMonoActions.Add(configureDelegate);

            return this;
        }

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) => _hostBuilder.ConfigureHostConfiguration(configureDelegate);
        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => _hostBuilder.ConfigureAppConfiguration(configureDelegate);
        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) => _hostBuilder.ConfigureServices(configureDelegate);
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) => _hostBuilder.UseServiceProviderFactory(factory);
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) => _hostBuilder.UseServiceProviderFactory(factory);
        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => _hostBuilder.ConfigureContainer(configureDelegate);

        public IHost Build()
        {
            _hostBuilder.ConfigureServices(collection => collection.AddScoped<ScopeRoot>());
            _hostBuilder.ConfigureServices((context, services) =>
            {
                foreach (var action in _unityCollectionBuilder.ConfigureActions)
                {
                    action?.Invoke(context, services);
                }
            });

            return _hostBuilder.Build();
        }
    }
}