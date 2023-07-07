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
        protected override void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            Debug.Log("ConfigureAppConfiguration");
        }

        protected override void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
        {
            Debug.Log("ConfigureLogging");
        }

        protected override void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            Debug.Log("ConfigureServices");
        }

        protected override void ConfigureUnityObjects(HostBuilderContext context, IUnityObjectsConfigurator services)
        {
            Debug.Log("ConfigureUnityObjects");

            services.AddMonoBehaviourHostedService<MonoHostedService>();
        }
    }
}