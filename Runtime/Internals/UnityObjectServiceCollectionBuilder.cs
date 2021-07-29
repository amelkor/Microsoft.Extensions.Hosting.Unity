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
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton(MonoBehaviour component, Type type = default, bool useHostLifetime = false)
        {
            _hostBuilder.ConfigureServices(services =>
            {
                type ??= component.GetType();

                services.AddSingleton(type, provider =>
                {
                    InjectServices(type, component, provider, _serviceInjectionMethodName);

                    if (useHostLifetime)
                        SetupLifetime(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourHostedService<T>() where T : MonoBehaviour, IHostedService
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService<T, T>(provider =>
                {
                    var root = provider.GetRequiredService<IMonoBehaviourHostRoot>();
                    var component = root.AddComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }
        
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourHostedService<T, TImpl>() where T : MonoBehaviour, IHostedService where TImpl : MonoBehaviour, T
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddHostedService<T, TImpl>(provider =>
                {
                    var root = provider.GetRequiredService<IMonoBehaviourHostRoot>();
                    var component = root.AddComponent<TImpl>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>(TImpl component, bool useHostLifetime = false) where TImpl : MonoBehaviour, T where T : class
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    InjectServices(component, provider, _serviceInjectionMethodName);

                    if (useHostLifetime)
                        SetupLifetime(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    var root = provider.GetRequiredService<IMonoBehaviourHostRoot>();
                    var component = root.AddComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, _serviceInjectionMethodName);

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
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, _serviceInjectionMethodName);

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
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, _serviceInjectionMethodName);

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
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, _serviceInjectionMethodName);

                    return component;
                });
            });

            return this;
        }

        public IMonoBehaviourServiceCollectionBuilder AddScriptableObjectSingleton<T>() where T : ScriptableObject
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    var component = ScriptableObject.CreateInstance(typeof(T));
                    InjectServices(component, provider, _serviceInjectionMethodName);

                    return (T) component;
                });
            });
            
            return this;
        }

        public IMonoBehaviourServiceCollectionBuilder AddScriptableObjectSingleton<T, TImpl>() where T : ScriptableObject where TImpl : ScriptableObject, T
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    var component = ScriptableObject.CreateInstance(typeof(TImpl));
                    InjectServices(component, provider, _serviceInjectionMethodName);

                    return (TImpl) component;
                });
            });
            
            return this;
        }

        public IMonoBehaviourServiceCollectionBuilder AddScriptableObjectTransient<T>() where T : ScriptableObject
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<T>(provider =>
                {
                    var component = ScriptableObject.CreateInstance(typeof(T));
                    InjectServices(component, provider, _serviceInjectionMethodName);

                    return (T) component;
                });
            });
            
            return this;
        }

        public IMonoBehaviourServiceCollectionBuilder AddScriptableObjectTransient<T, TImpl>() where T : class where TImpl : ScriptableObject, T
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<T>(provider =>
                {
                    var component = ScriptableObject.CreateInstance(typeof(TImpl));
                    InjectServices(component, provider, _serviceInjectionMethodName);

                    return (TImpl) component;
                });
            });
            
            return this;
        }

        #region private methods

        private static void InjectServices<T>(T component, IServiceProvider provider, string injectionMethodName) where T : UnityEngine.Object
        {
            InjectServices(typeof(T), component, provider, injectionMethodName);
        }
        
        private static void InjectServices(Type type, UnityEngine.Object component, IServiceProvider provider, string injectionMethodName)
        {
            if (!type.TryGetInjectionMethod(injectionMethodName, out var inject))
                return;

            var instances = new object[inject.types.Length];
            for (var i = 0; i < inject.types.Length; i++)
            {
                instances[i] = provider.GetRequiredService(inject.types[i]);
            }

            inject.method.Invoke(component, instances);
        }

        private static void SetupLifetime<T>(T component, IServiceProvider provider, string injectionMethodName) where T : MonoBehaviour
        {
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