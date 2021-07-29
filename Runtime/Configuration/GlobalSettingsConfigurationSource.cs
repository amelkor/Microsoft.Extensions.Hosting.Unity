using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting.Unity.Configuration
{
    public class GlobalSettingsConfigurationSource<TSettings> : IConfigurationSource where TSettings : GlobalSettings, new()
    {
        internal TSettings Provider { get; private set; }
        
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var settings = new TSettings();

            if (!File.Exists(GlobalSettings.LocalFilePath))
                settings.Save();
            else
                settings.Load();

            Provider = settings;
            return settings;
        }
    }
}