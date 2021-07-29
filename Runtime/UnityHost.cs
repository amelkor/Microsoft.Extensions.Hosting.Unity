#define MICROSOFT_EXTENSIONS_HOSTING

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace Microsoft.Extensions.Hosting.Unity
{
    public static class UnityHost
    {
        public static IHostBuilder CreateDefaultBuilder(string monoBehaviourInjectMethodName, string[] args = default)
        {
            if (string.IsNullOrEmpty(monoBehaviourInjectMethodName))
                throw new ArgumentNullException(nameof(monoBehaviourInjectMethodName), "MonoBehaviour ingection method name should be specified");

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => { builder.DisableFileConfigurationSourceReloadOnChange(); })
                .ConfigureLogging((_, loggingBuilder) => { loggingBuilder.ClearProviders(); })
                .UseMonoBehaviourServiceCollection(monoBehaviourInjectMethodName)
                .ConfigureServices((context, services) =>
                {
                    var root = new GameObject($"{nameof(MonoBehaviourHostRoot)} (host root)");
                    var component = root.AddComponent<MonoBehaviourHostRoot>();

                    context.Properties.Add(Constants.MonoBehaviourHostRootInstanceKey, component);

                    // todo handle lifetime ?
                    // services.AddSingleton<IMonoBehaviourHostRoot, MonoBehaviourHostRoot>(provider =>
                    // {
                    //     var lifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    //
                    //     var root = new GameObject($"{nameof(MonoBehaviourHostRoot)} (host root)");
                    //     var component = root.AddComponent<MonoBehaviourHostRoot>();
                    //
                    //     lifetime.ApplicationStopped.Register(() =>
                    //     {
                    //         if (!root)
                    //             return;
                    //
                    //         UnityEngine.Object.Destroy(root.gameObject);
                    //     });
                    //
                    //     return component;
                    // });
                });
        }
    }
}