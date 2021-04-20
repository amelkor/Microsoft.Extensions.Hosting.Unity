using System;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    /// <inheritdoc/>
    internal class MonoBehaviourServiceCollectionBuilder : IMonoBehaviourServiceCollectionBuilder
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly string _serviceInjectionMethodName;

        public MonoBehaviourServiceCollectionBuilder(IHostBuilder hostBuilder, string serviceInjectionMethodName)
        {
            _hostBuilder = hostBuilder;
            _serviceInjectionMethodName = serviceInjectionMethodName;
        }

        /// <inheritdoc/>
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(provider =>
                {
                    var root = provider.GetRequiredService<IMonoBehaviourHostRoot>();
                    var component = root.AddComponent<T>();
                    SetupUnityComponent(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T, TImpl>(provider =>
                {
                    var root = provider.GetRequiredService<IMonoBehaviourHostRoot>();
                    var component = root.AddComponent<TImpl>();
                    SetupUnityComponent(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<T>(provider =>
                {
                    var gameObject = new GameObject($"{typeof(T).Name} (hosted transient)");
                    var component = gameObject.AddComponent<T>();
                    SetupUnityComponent(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<T, TImpl>(provider =>
                {
                    var gameObject = new GameObject($"{typeof(TImpl).Name} (hosted transient)");
                    var component = gameObject.AddComponent<TImpl>();
                    SetupUnityComponent(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        #region private methods

        private static void SetupUnityComponent<T>(T component, IServiceProvider provider, string injectionMethodName) where T : MonoBehaviour
        {
            if (Reflection.TryGetInjectionMethod<T>(injectionMethodName, out var inject))
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