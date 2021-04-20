using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    public interface IMonoBehaviourServiceCollectionBuilder
    {
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T>() where T : MonoBehaviour;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourSingleton<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourTransient<T>() where T : MonoBehaviour;
        IMonoBehaviourServiceCollectionBuilder AddMonoBehaviourTransient<T, TImpl>() where T : class where TImpl : MonoBehaviour, T;
    }
}