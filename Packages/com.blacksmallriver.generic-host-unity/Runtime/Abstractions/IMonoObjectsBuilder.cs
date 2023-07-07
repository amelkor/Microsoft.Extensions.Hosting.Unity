using System;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    public interface IMonoObjectsBuilder
    {
        IMonoObjectsBuilder AddMonoBehaviourSingleton(MonoBehaviour component, Type type = default, bool useHostLifetime = false);
        IMonoObjectsBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour;
        
        /// <summary>
        /// Add a MonoBehaviour as a separate GameObject which won't be destroyed on load.
        /// </summary>
        IMonoObjectsBuilder AddDetachedMonoBehaviourSingleton<T>() where T : MonoBehaviour;
        IMonoObjectsBuilder AddMonoBehaviourHostedService<T>() where T : MonoBehaviour, IHostedService;
        IMonoObjectsBuilder AddMonoBehaviourHostedService<T, TImpl>() where T : MonoBehaviour, IHostedService where TImpl : MonoBehaviour, T;
        IMonoObjectsBuilder AddMonoBehaviourSingleton<T, TImpl>(TImpl component, bool useHostLifetime = false) where TImpl : MonoBehaviour, T where T : class;
        IMonoObjectsBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        IMonoObjectsBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour;
        IMonoObjectsBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        
        IMonoObjectsBuilder AddScriptableObjectSingleton(ScriptableObject scriptableObject, Type type);
        IMonoObjectsBuilder AddScriptableObjectSingleton<T>(T scriptableObject) where T : ScriptableObject;
        IMonoObjectsBuilder AddScriptableObjectSingleton<T, TImpl>(TImpl scriptableObject) where T : class where TImpl : ScriptableObject, T;

        IMonoObjectsBuilder AddMonoBehaviourPrefabSingleton<T>(string resourcesPath) where T : MonoBehaviour;
        IMonoObjectsBuilder AddMonoBehaviourPrefabTransient<T>(string resourcesPath) where T : MonoBehaviour;
        IMonoObjectsBuilder AddMonoBehaviourPrefabScoped<T>(string resourcesPath) where T : MonoBehaviour;
    }
}