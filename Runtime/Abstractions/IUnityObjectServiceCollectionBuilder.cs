using System;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    public interface IUnityObjectServiceCollectionBuilder
    {
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton(MonoBehaviour component, Type type = default, bool useHostLifetime = false);
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour;
        
        /// <summary>
        /// Add a MonoBehaviour as a separate GameObject which won't be destroyed on load.
        /// </summary>
        IUnityObjectServiceCollectionBuilder AddDetachedMonoBehaviourSingleton<T>() where T : MonoBehaviour;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourHostedService<T>() where T : MonoBehaviour, IHostedService;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourHostedService<T, TImpl>() where T : MonoBehaviour, IHostedService where TImpl : MonoBehaviour, T;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>(TImpl component, bool useHostLifetime = false) where TImpl : MonoBehaviour, T where T : class;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        
        IUnityObjectServiceCollectionBuilder AddScriptableObjectSingleton(ScriptableObject scriptableObject, Type type);
        IUnityObjectServiceCollectionBuilder AddScriptableObjectSingleton<T>(T scriptableObject) where T : ScriptableObject;
        IUnityObjectServiceCollectionBuilder AddScriptableObjectSingleton<T, TImpl>(TImpl scriptableObject) where T : ScriptableObject where TImpl : ScriptableObject, T;

        IUnityObjectServiceCollectionBuilder AddMonoBehaviourPrefabSingleton<T>(string resourcesPath) where T : MonoBehaviour;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourPrefabTransient<T>(string resourcesPath) where T : MonoBehaviour;
        IUnityObjectServiceCollectionBuilder AddMonoBehaviourPrefabScoped<T>(string resourcesPath) where T : MonoBehaviour;
    }
}