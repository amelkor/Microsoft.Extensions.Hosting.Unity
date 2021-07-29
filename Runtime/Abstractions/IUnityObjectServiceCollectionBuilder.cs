using System;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    public interface IMonoBehaviourServiceCollectionBuilder
    {
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton(MonoBehaviour component, Type type = default, bool useHostLifetime = false);
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourHostedService<T>() where T : MonoBehaviour, IHostedService;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourHostedService<T, TImpl>() where T : MonoBehaviour, IHostedService where TImpl : MonoBehaviour, T;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>(TImpl component, bool useHostLifetime = false) where TImpl : MonoBehaviour, T where T : class;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        
        IMonoBehaviourServiceCollectionBuilder AddScriptableObjectSingleton<T>() where T : ScriptableObject;
        IMonoBehaviourServiceCollectionBuilder AddScriptableObjectSingleton<T, TImpl>() where T : ScriptableObject where TImpl : ScriptableObject, T;
        IMonoBehaviourServiceCollectionBuilder AddScriptableObjectTransient<T>() where T : ScriptableObject;
        IMonoBehaviourServiceCollectionBuilder AddScriptableObjectTransient<T, TImpl>() where T : class where TImpl : ScriptableObject, T;
    }
}