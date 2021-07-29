![Preview](Editor/Gizmos/host_service_icon.png)
 
[![Twitter Follow](https://img.shields.io/twitter/follow/AlekseyMelkor?color=blue&label=Follow%20on%20Twitter&logo=%20&logoColor=%20&style=flat-square)](https://twitter.com/AlekseyMelkor)

# Microsoft.Extensions.Hosting for Unity3D
#### This package allows to use DI Hosting in Unity projects.
The documentation can be found here: [Tutorial: Use dependency injection in .NET](https://docs.microsoft.com/ru-ru/dotnet/core/extensions/dependency-injection-usage)

## Install

### UPM Package install via npmjs:

Add the entry below to the project's manifest.json

```json
    "scopedRegistries": [
      {
        "name": "microsoft.extensions.hosting",
        "url": "https://registry.npmjs.org",
        "scopes": [
          "com.blacksmallriver.hosting",
          "com.blacksmallriver"
        ]
      }
    ]
```
Or add it in Unity Editor:

> Project Settings -> Package Manager -> Scoped Registries
> 
> name: microsoft.extensions.hosting
> 
> URL: https://registry.npmjs.org
> 
> scope: com.blacksmallriver.hosting

### UPM Package install via git URL (no package updates will be available this way):

> Requires a version of unity that supports path query parameter for git packages (Unity >= 2019.3.4f1).

Add https://github.com/amelkor/Microsoft.Extensions.Hosting.Unity.git to Unity Package Manager

## Getting started

> Check the [Demo Project](https://github.com/amelkor/Microsoft.Extensions.Hosting.Unity-Demo)

Injection into `Monobehaviour` classes happens via custom defined private method which name is specified as `Services Injection Method Name` parameter. The default name is `AwakeServices`. Could be adjusted in the Unity Inspector window.

To get started, you need to implement a `HostManager`

A new instance of `Host` could be created like:
```cs
            var hostBuilder = Host.CreateDefaultBuilder()
                .ConfigureLogging((_, loggingBuilder) => { loggingBuilder.ClearProviders(); })
                .UseMonoBehaviourServiceCollection(/*Services Injection Method Name for MonoBehaviour*/)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IMonoBehaviourHostRoot, MonoBehaviourHostRoot>(provider =>
                    {
                        var lifetime = provider.GetRequiredService<IHostApplicationLifetime>();

                        var root = new GameObject($"{nameof(MonoBehaviourHostRoot)} (host root)");
                        var component = root.AddComponent<MonoBehaviourHostRoot>();

                        lifetime.ApplicationStopped.Register(() =>
                        {
                            if (!root)
                                return;

                            UnityEngine.Object.Destroy(root.gameObject);
                        });

                        return component;
                    });
                });

            // add services
                // hostBuilder.ConfigureServices
                // hostBuilder.ConfigureMonoBehaviours

            // build the host
            var host = hostBuilder.Build();

            // start the host
            await host.StartAsync();

            // stop the host
            await host.StopAsync()
```

`loggingBuilder.ClearProviders();` is needed since `DefaultHostBuilder` uses some default logging stuff not available in Unity.

> to log into Unity's console use `hostBuilder.ConfigureLogging(builder => builder.AddUnityLogger());`

Also there's a way to quickly setup a new Host via deriving from the default template MonoBehaviour class `HostManager` and attach it to a gameobject. 

The settings are available through the Inspector window.

```cs
    /// <summary>
    /// Example implementation.
    /// </summary>
    public class DemoHostManager : HostManager
    {
        protected override void ConfigureAppConfiguration(IConfigurationBuilder builder)
        {
            // add a configuration providers here
        }

        protected override void ConfigureLogging(ILoggingBuilder builder)
        {
            // add a logger library here
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            // add ordinary C# classes services here
            
            // transient is also supported
            //services.AddTransient<JustService>();
            
            services.AddSingleton<JustService>();
            
            // hosted services could be added by their interfaces as well
            services.AddHostedService<IHostedServiceContract, PocHostedService>();
        }

        protected override void ConfigureMonoBehaviours(IMonoBehaviourServiceCollectionBuilder services)
        {
            // add MonoBehaviour classes services here
            // all MonoBehaviour singltones will be attached to a single gameObject created at runtime
            
            // transient will be created as new gameObject
            // services.AddMonoBehaviourTransient<DemoMono>();
            
            services.AddMonoBehaviourSingleton<DemoMono>();
            services.AddMonoBehaviourSingleton<BehaviourTestComponent>();
            
            // ScriptableObject's must be passed as an existing instance
            services.services.AddScriptableObjectSingleton(instance, typeof(MyScriptableObject));
            
            // a MonoBehaviour with IHostedService implementation
            services.AddMonoBehaviourHostedService<GameModeService>();
        }
```

Then create a new GameObject and attach the script.

### GlobalSettings for Host

Inheriting from `GlobalSettings` class allows to have a settings file under `Config/globalsettings.json` path of the Unity application. Any property in the derived class would be saved into the file and read on the host starting.

## Licensing
- The asset: MIT License, see  [LICENSE.md](LICENSE.md) for more information.
- .NET libraries: MIT License, see  [Runtime/lib/LICENSE.md](Runtime/lib/LICENSE.txt) for more information.
