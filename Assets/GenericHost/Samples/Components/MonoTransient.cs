using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Scripting;

namespace GenericHost.Samples
{
    public class MonoTransient : MonoBehaviour
    {
        private ILogger<MonoTransient> _logger;

        [Preserve]
        private void AwakeServices(ILogger<MonoTransient> logger)
        {
            _logger = logger;
        }

        private void Start()
        {
            _logger.LogInformation("Transient MonoBehaviour Start()");

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(transform);
            sphere.AddComponent<Rigidbody>();
            
            Destroy(gameObject, 3f);
        }

        private void OnDestroy()
        {
            _logger.LogInformation("Destroying Transient MonoBehaviour");
        }
    }
}