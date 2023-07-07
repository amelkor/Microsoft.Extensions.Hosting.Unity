using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    internal class UnityCollectionBuilder : IMonoObjectsBuilder
    {
        // ReSharper disable once ArrangeObjectCreationWhenTypeEvident
        private readonly List<Action<HostBuilderContext, IServiceCollection>> _configureServicesActions = new List<Action<HostBuilderContext, IServiceCollection>>();
        private readonly string _serviceInjectionMethodName;

        public IEnumerable<Action<HostBuilderContext, IServiceCollection>> ConfigureActions => _configureServicesActions;

        public UnityCollectionBuilder(string serviceInjectionMethodName)
        {
            _serviceInjectionMethodName = serviceInjectionMethodName;
        }

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddMonoBehaviourSingleton(MonoBehaviour component, Type type = default, bool useHostLifetime = false)
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (!component)
                    throw new ArgumentNullException(nameof(component), "Component is missing");

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
        public IMonoObjectsBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour
        {
            _configureServicesActions.Add((context, services) =>
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
        public IMonoObjectsBuilder AddDetachedMonoBehaviourSingleton<T>() where T : MonoBehaviour
        {
            _configureServicesActions.Add((context, services) =>
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

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddMonoBehaviourHostedService<T>() where T : MonoBehaviour, IHostedService
        {
            _configureServicesActions.Add((context, services) =>
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

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddMonoBehaviourHostedService<T, TImpl>() where T : MonoBehaviour, IHostedService where TImpl : MonoBehaviour, T
        {
            _configureServicesActions.Add((context, services) =>
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
        public IMonoObjectsBuilder AddMonoBehaviourSingleton<T, TImpl>(TImpl component, bool useHostLifetime = false) where T : class where TImpl : MonoBehaviour, T
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (!component)
                    throw new ArgumentNullException(nameof(component), "Component is missing");
                
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
        public IMonoObjectsBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T
        {
            _configureServicesActions.Add((context, services) =>
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
        public IMonoObjectsBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour
        {
            _configureServicesActions.Add((context, services) =>
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
        public IMonoObjectsBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T
        {
            _configureServicesActions.Add((context, services) =>
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

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddScriptableObjectSingleton(ScriptableObject scriptableObject, Type type)
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (!scriptableObject)
                    throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is missing");
                
                services.AddSingleton(type, provider =>
                {
                    InjectServices(scriptableObject, provider, _serviceInjectionMethodName);

                    return scriptableObject;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddScriptableObjectSingleton<T>(T scriptableObject) where T : ScriptableObject
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (!scriptableObject)
                    throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is missing");
                
                services.AddSingleton<T>(provider =>
                {
                    InjectServices(scriptableObject, provider, _serviceInjectionMethodName);

                    return scriptableObject;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddScriptableObjectSingleton<T, TImpl>(TImpl scriptableObject) where T : class where TImpl : ScriptableObject, T
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (!scriptableObject)
                    throw new ArgumentNullException(nameof(scriptableObject), "ScriptableObject is missing");
                
                services.AddSingleton<T>(provider =>
                {
                    InjectServices(scriptableObject, provider, _serviceInjectionMethodName);

                    return (TImpl) scriptableObject;
                });
            });

            return this;
        }

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddMonoBehaviourPrefabSingleton<T>(string resourcesPath) where T : MonoBehaviour
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (string.IsNullOrWhiteSpace(resourcesPath))
                    throw new ArgumentNullException(nameof(resourcesPath), "Resources path is missing");
                
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
        public IMonoObjectsBuilder AddMonoBehaviourPrefabTransient<T>(string resourcesPath) where T : MonoBehaviour
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (string.IsNullOrWhiteSpace(resourcesPath))
                    throw new ArgumentNullException(nameof(resourcesPath), "Resources path is missing");
                
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

        /// <inheritdoc/>
        public IMonoObjectsBuilder AddMonoBehaviourPrefabScoped<T>(string resourcesPath) where T : MonoBehaviour
        {
            _configureServicesActions.Add((context, services) =>
            {
                if (string.IsNullOrWhiteSpace(resourcesPath))
                    throw new ArgumentNullException(nameof(resourcesPath), "Resources path is missing");
                
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

        private static IMonoBehaviourHostRoot GetHostRoot(HostBuilderContext context)
        {
            // ReSharper disable once InvertIf
            if (context.Properties.TryGetValue(Constants.MonoBehaviourHostRootInstanceKey, out var root))
            {
                if (root is IMonoBehaviourHostRoot r)
                    return r;
            }

            throw new ArgumentException(nameof(HostBuilderContext) + " does not contain " + nameof(IMonoBehaviourHostRoot));
        }

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

            if (dontDestroyOnLoad)
                UnityEngine.Object.DontDestroyOnLoad(component.gameObject);
        }

        #endregion
    }
}