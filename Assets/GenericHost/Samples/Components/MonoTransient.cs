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
            
            var rb = sphere.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.AddForce(RandomVector3(Random.Range(10f, 100f)), ForceMode.Impulse);
            
            Destroy(gameObject, 3f);
        }

        private static Vector3 RandomVector3(float force)
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * force;
        }

        private void OnDestroy()
        {
            _logger.LogInformation("Destroying Transient MonoBehaviour");
        }
    }
}