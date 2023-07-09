using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Unity;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace GenericHost.Samples
{
    public class SampleHost : HostManager
    {
        [Tooltip("Could be used for referencing ScriptableObjects and as configuration provider")]
        [SerializeField] private ConfigurationScriptableObject configuration;
        
        [Tooltip("UI instance from scene of from SampleHost GameObject hierarchy")]
        [SerializeField] private SampleUI sampleUI;

        protected override void OnAwake()
        {
            // Awake called before host build
            // if buildOnAwake is false (can be controlled through Inspector) then host needs to be build manually
            // By default builds on Awake
            buildOnAwake = false;

            // Use custom methods name for Unity Objects (default is AwakeServices)
            // The services will be injected into ctor
            // servicesInjectionMethodName = "CustomInjectionMethodName";

            // Call from the desired place to build the host if buildOnAwake = false;
            BuildManually();

            // Stops the host
            // StopManually(); or StopManuallyAsync();
        }

        protected override void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            Debug.Log("ConfigureAppConfiguration");
            
            // Add SO as configuration source, will parse all attributes from the SO and make them accessible from IConfiguration
            builder.AddScriptableObjectConfiguration<ConfigurationScriptableObject>(configuration);
        }

        protected override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            Debug.Log("ConfigureLogging");
        }

        protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // Register ordinary C# classes
            Debug.Log("ConfigureServices");

            services.AddHostedService<SampleHostedService, SampleHostedService>();
        }

        protected override void ConfigureUnityObjects(HostBuilderContext context, IUnityObjectsConfigurator services)
        {
            Debug.Log("ConfigureUnityObjects");
            
            foreach (var (type, so) in configuration.GetScriptableObjectsToRegisterAsSingeltons())
                services.AddScriptableObjectSingleton(so, type);

            // Registers already instantiated instance under HostRoot GameObject
            services.AddMonoBehaviourSingleton(sampleUI);

            // Register as hosted service (will be resolved and start automatically when host start)
            services.AddMonoBehaviourHostedService<MonoHostedService, MonoHostedService>();

            // Creates new instances under HostRoot GameObject
            services.AddMonoBehaviourSingleton<MonoSingleton>();
            services.AddMonoBehaviourTransient<MonoTransient>();
            
            // Or creates new instance in scene root
            // services.AddDetachedMonoBehaviourSingleton<MonoSingleton>();

            Debug.Log($"SampleInteger config value from registered SO: {context.Configuration["SampleInteger"]}");
            
            // Loads from Resources (spawns detached)
            services.AddMonoBehaviourPrefabTransient<RandomMoveObject>(configuration.movingObjectPrefabName);
        }
    }
}