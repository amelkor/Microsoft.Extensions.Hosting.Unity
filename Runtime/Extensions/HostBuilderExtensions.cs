using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Unity.Configuration;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    /// <summary>
    /// <see cref="IHostBuilder"/> extensions for Unity3D.
    /// </summary>
    public static class HostBuilderExtensions
    {
        private const string INJECTABLE_METHOD_NAME = "AwakeServices";
        private const string MONO_BEHAVIOUR_BUILDER_KEY = "MonoBehaviourBuilder";

        /// <summary>
        /// Add <see cref="MonoBehaviour"/> classes to this <see cref="IHost"/> services.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IHostBuilder ConfigureMonoBehaviours(this IHostBuilder builder, Action<IUnityObjectServiceCollectionBuilder> configure)
        {
            if (!builder.Properties.TryGetValue(MONO_BEHAVIOUR_BUILDER_KEY, out var monoCollectionObject))
            {
                throw new ArgumentException("MonoBehaviour ServiceCollection Builder is not registered in HostBuilder. Add " + nameof(UseMonoBehaviourServiceCollection) + " to the IHostBuilder");
            }

            var monoCollection = (IUnityObjectServiceCollectionBuilder) monoCollectionObject;
            configure?.Invoke(monoCollection);

            return builder;
        }

        /// <summary>
        /// Enable <see cref="MonoBehaviour"/> in this <see cref="IHost"/> container.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="monoBehaviourInjectMethodName">Method name to use to inject services into <see cref="MonoBehaviour"/> classes.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">When this method was already called once.</exception>
        internal static IHostBuilder UseMonoBehaviourServiceCollection(this IHostBuilder hostBuilder, string monoBehaviourInjectMethodName)
        {
            if (hostBuilder.Properties.ContainsKey(MONO_BEHAVIOUR_BUILDER_KEY))
            {
                throw new InvalidOperationException(nameof(UseMonoBehaviourServiceCollection) + " can not be called twice");
            }

            var monoCollection = new UnityObjectServiceCollectionBuilder(hostBuilder, monoBehaviourInjectMethodName);
            hostBuilder.Properties.Add(MONO_BEHAVIOUR_BUILDER_KEY, monoCollection);

            return hostBuilder;
        }

        /// <summary>
        /// Allows to register a specific <see cref="IHostedService"/> implementation to be injected.
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public static IServiceCollection AddHostedService<TService, TImplementation>(this IServiceCollection services)
            where TService : class
            where TImplementation : class, IHostedService, TService
        {
            services.AddSingleton<TService, TImplementation>();
            services.AddHostedService<HostedServiceWrapper<TService>>();
            return services;
        }

        /// <summary>
        /// Allows to register a specific <see cref="IHostedService"/> implementation to be injected.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="implementationFactory">The factory that creates the service.</param>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public static IServiceCollection AddHostedService<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, IHostedService, TService
        {
            services.AddSingleton<TService, TImplementation>(implementationFactory);
            services.AddHostedService<HostedServiceWrapper<TService>>();
            return services;
        }

        /// <summary>
        /// Will suppress host lifetime messages logging if <paramref name="suppressStatusMessages"/> is set to true.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="suppressStatusMessages">Will not log host lifetime messages if true.</param>
        /// <returns></returns>
        public static IServiceCollection SuppressStatusMessages(this IServiceCollection services, bool suppressStatusMessages)
        {
            services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = suppressStatusMessages);
            return services;
        }

        /// <summary>
        /// Sets ApplicationName for the HostingEnvironment.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IHostBuilder SetApplicationName(this IHostBuilder builder, string name)
        {
            builder.ConfigureAppConfiguration((context, _) => context.HostingEnvironment.ApplicationName = name);
            return builder;
        }

        /// <summary>
        /// Sets EnvironmentName for the HostingEnvironment.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IHostBuilder SetEnvironmentName(this IHostBuilder builder, string name)
        {
            builder.ConfigureAppConfiguration((context, _) => context.HostingEnvironment.EnvironmentName = name);
            return builder;
        }

        /// <summary>
        /// Use Config/appsettings.json to store settings.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <typeparam name="TSettings"></typeparam>
        // ReSharper disable once CognitiveComplexity
        public static void UseGlobalSettings<TSettings>(this IHostBuilder hostBuilder) where TSettings : GlobalSettings, new()
        {
            const string globalSettingsConfigured = "GLOBALSETTINGS_CONFIGURED";
            const string globalSettingsSource = "GLOBALSETTINGS_SOURCE";

            if (hostBuilder.Properties.TryGetValue(globalSettingsSource, out _))
                throw new InvalidOperationException("GlobalSettings already configured");

            hostBuilder.ConfigureAppConfiguration((context, builder) =>
            {
                var source = new GlobalSettingsConfigurationSource<TSettings>();
                builder.Add(source);

                if (!context.Properties.ContainsKey(globalSettingsSource))
                    hostBuilder.Properties.Add(globalSettingsSource, source);
            });
            hostBuilder.ConfigureServices((context, collection) =>
            {
                if (context.Properties.TryGetValue(globalSettingsSource, out var source))
                {
                    if (source is GlobalSettingsConfigurationSource<TSettings> s)
                        collection.AddSingleton<TSettings>(s.Provider);
                }
            });
        }

        #region private methods

        private static void SetupUnityComponent<T>(T component, IServiceProvider provider) where T : MonoBehaviour
        {
            if (typeof(T).TryGetInjectionMethod(INJECTABLE_METHOD_NAME, out var inject))
            {
                var instances = new object[inject.types.Length];
                for (var i = 0; i < inject.types.Length; i++)
                {
                    instances[i] = provider.GetRequiredService(inject.types[i]);
                }

                inject.method.Invoke(component, instances);
            }

            var lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                if (!component)
                    return;

                component.StopAllCoroutines();
                component.CancelInvoke();
            });
        }

        #endregion
    }
}