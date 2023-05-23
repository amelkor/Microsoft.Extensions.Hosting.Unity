using System;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    /// <inheritdoc/>
    internal class UnityObjectServiceCollectionBuilder : IUnityObjectServiceCollectionBuilder
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly string _serviceInjectionMethodName;

        public UnityObjectServiceCollectionBuilder(IHostBuilder hostBuilder, string serviceInjectionMethodName)
        {
            _hostBuilder = hostBuilder;
            _serviceInjectionMethodName = serviceInjectionMethodName;
        }

        private static IMonoBehaviourHostRoot GetHostRoot(HostBuilderContext context)
        {
            // ReSharper disable once InvertIf
            if (context.Properties.TryGetValue(Constants.MonoBehaviourHostRootInstanceKey, out var root))
            {
                if (root is IMonoBehaviourHostRoot r)
                    return r;
            }

            throw new ArgumentException("HostBuilderContext does not contain IMonoBehaviourHostRoot");
        }
        
        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton(MonoBehaviour component, Type type = default, bool useHostLifetime = false)
        {
            if (!component)
                throw new ArgumentNullException(nameof(component), "Component is missing");
            
            _hostBuilder.ConfigureServices(services =>
            {
                type ??= component.GetType();

                services.AddSingleton(type, provider =>
                {
                    InjectServices(type, component, provider, _serviceInjectionMethodName);

                    if (useHostLifetime)
                        SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }
        
        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddDetachedMonoBehaviourSingleton<T>() where T : MonoBehaviour
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    var gameObject = new GameObject($"{typeof(T).Name} (hosted singleton)");
                    var component = gameObject.AddComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider, true);

                    return component;
                });
            });

            return this;
        }

        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourHostedService<T>() where T : MonoBehaviour, IHostedService
        {
            _hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<T, T>(provider =>
                {
                    var root = GetHostRoot(context);
                    var component = root.AddComponent<T>();
                    component.enabled = false;
                    
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    component.enabled = true;
                    
                    return component;
                });
            });

            return this;
        }
        
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourHostedService<T, TImpl>() where T : MonoBehaviour, IHostedService where TImpl : MonoBehaviour, T
        {
            _hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<T, TImpl>(provider =>
                {
                    var root = GetHostRoot(context);
                    var component = root.AddComponent<TImpl>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>(TImpl component, bool useHostLifetime = false) where TImpl : MonoBehaviour, T where T : class
        {
            if (!component)
                throw new ArgumentNullException(nameof(component), "Component is missing");
            
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    InjectServices(component, provider, _serviceInjectionMethodName);

                    if (useHostLifetime)
                        SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour
        {
            _hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<T>(provider =>
                {
                    var root = GetHostRoot(context);
                    var component = root.AddComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T
        {
            _hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddSingleton<T, TImpl>(provider =>
                {
                    var root = GetHostRoot(context);
                    var component = root.AddComponent<TImpl>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }
        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<T>(provider =>
                {
                    var gameObject = new GameObject($"{typeof(T).Name} (hosted transient)");
                    var component = gameObject.AddComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T
        {
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddTransient<T, TImpl>(provider =>
                {
                    var gameObject = new GameObject($"{typeof(TImpl).Name} (hosted transient)");
                    var component = gameObject.AddComponent<TImpl>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }

        public IUnityObjectServiceCollectionBuilder AddScriptableObjectSingleton(ScriptableObject scriptableObject, Type type)
        {
            if (!scriptableObject)
                throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is missing");
            
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(type, provider =>
                {
                    InjectServices(scriptableObject, provider, _serviceInjectionMethodName);

                    return scriptableObject;
                });
            });
            
            return this;
        }

        public IUnityObjectServiceCollectionBuilder AddScriptableObjectSingleton<T>(T scriptableObject) where T : ScriptableObject
        {
            if (!scriptableObject)
                throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is missing");

            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    InjectServices(scriptableObject, provider, _serviceInjectionMethodName);

                    return scriptableObject;
                });
            });
            
            return this;
        }

        public IUnityObjectServiceCollectionBuilder AddScriptableObjectSingleton<T, TImpl>(TImpl scriptableObject) where T : class where TImpl : ScriptableObject, T
        {
            if (!scriptableObject)
                throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is missing");
            
            _hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<T>(provider =>
                {
                    InjectServices(scriptableObject, provider, _serviceInjectionMethodName);

                    return (TImpl) scriptableObject;
                });
            });
            
            return this;
        }

        
        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourPrefabSingleton<T>(string resourcesPath) where T : MonoBehaviour
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
                throw new ArgumentNullException(nameof(resourcesPath), "Resources path is missing");

            _hostBuilder.ConfigureServices((context, services) =>
            {
                var prefab = Resources.Load<T>(resourcesPath);
                
                services.AddSingleton<T>(provider =>
                {
                    var root = GetHostRoot(context);
                    var go = UnityEngine.Object.Instantiate(prefab.gameObject, root.GameObject.transform);
                    var component = go.GetComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }
        
        /// <inheritdoc/>
        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourPrefabTransient<T>(string resourcesPath) where T : MonoBehaviour
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
                throw new ArgumentNullException(nameof(resourcesPath), "Resources path is missing");

            _hostBuilder.ConfigureServices((context, services) =>
            {
                var prefab = Resources.Load<T>(resourcesPath);
                
                services.AddTransient<T>(provider =>
                {
                    var go = UnityEngine.Object.Instantiate(prefab.gameObject);
                    var component = go.GetComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
                });
            });

            return this;
        }

        public IUnityObjectServiceCollectionBuilder AddMonoBehaviourPrefabScoped<T>(string resourcesPath) where T : MonoBehaviour
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
                throw new ArgumentNullException(nameof(resourcesPath), "Resources path is missing");

            _hostBuilder.ConfigureServices((context, services) =>
            {
                var prefab = Resources.Load<T>(resourcesPath);
                
                services.AddScoped<T>(provider =>
                {
                    var s = provider.GetService<ScopeRoot>();
                    var go = UnityEngine.Object.Instantiate(prefab.gameObject, s.root.transform);
                    var component = go.GetComponent<T>();
                    InjectServices(component, provider, _serviceInjectionMethodName);
                    SetupLifetime(component, provider);

                    return component;
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

        private static void SetupLifetime<T>(T component, IServiceProvider provider, bool dontDestroyOnLoad = false) where T : MonoBehaviour
        {
            var lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
            lifetime.ApplicationStopping.Register(() =>
            {
                if (!component)
                    return;

                component.StopAllCoroutines();
                component.CancelInvoke();
            });
            
            if(dontDestroyOnLoad)
                GameObject.DontDestroyOnLoad(component.gameObject);
        }

        #endregion
    }
}