using System;

namespace Microsoft.Extensions.Hosting.Unity
{
    public interface IMonoHostBuilder : IHostBuilder
    {
        public IMonoHostBuilder ConfigureUnityObjects(Action<HostBuilderContext, IMonoObjectsBuilder> configureDelegate);
    }
}