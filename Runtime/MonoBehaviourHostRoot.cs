using System;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    internal interface IMonoBehaviourHostRoot
    {
        T AddComponent<T>() where T : MonoBehaviour;
    }

    internal class MonoBehaviourHostRoot : MonoBehaviour, IMonoBehaviourHostRoot
    {
        private GameObject _root;

        private void Awake()
        {
            _root = gameObject;
        }

        public T AddComponent<T>() where T : MonoBehaviour => _root.AddComponent<T>();
    }
}