using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.Hosting.Unity
{
    [Serializable]
    public class AppSettings : IConfigurationProvider
    {
        internal const string ConfigDirPath = "Config";
        internal const string ConfigFilePath = "appsettings.json";
        internal const string LocalFilePath = ConfigDirPath + "/" + ConfigFilePath;
        private static readonly System.Text.Json.JsonSerializerOptions _serializerOptions = new System.Text.Json.JsonSerializerOptions {Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true};
        private readonly Type _type;
        private readonly Dictionary<string, PropertyInfo> _properties;
        private IConfigurationRoot _configurationRoot;
        private ConfigurationReloadToken _reloadToken = new ConfigurationReloadToken();

        protected AppSettings()
        {
            _type = GetType();
            _properties = _type.GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.SetProperty |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .ToDictionary(p => p.Name, p => p);
        }

        internal void SetConfiguration(IConfiguration configuration)
        {
            // ReSharper disable once InvertIf
            if (configuration != null)
            {
                _configurationRoot = (IConfigurationRoot) configuration;
                configuration.Bind(this);
            }
        }

        public bool TryGet(string key, out string value)
        {
            if (_properties.TryGetValue(key, out var property))
            {
                var v = property.GetValue(this);
                if (v != null)
                {
                    value = v.ToString();
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Set(string key, string value)
        {
            if (!_properties.TryGetValue(key, out var property))
                return;

            var v = Convert.ChangeType(value, property.PropertyType);
            property.SetValue(this, v);
        }

        public IChangeToken GetReloadToken() => _reloadToken;

        public void Load()
        {
            var json = BeforeLoad(File.ReadAllText(LocalFilePath));
            var deserialized = System.Text.Json.JsonSerializer.Deserialize(json, _type, _serializerOptions);

            foreach (var property in _properties.Values)
            {
                var value = property.GetValue(deserialized);
                property.SetValue(this, value);
            }

            OnReload();
            _configurationRoot?.Reload();
        }

        public virtual IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            var prefix = parentPath == null ? string.Empty : parentPath + ConfigurationPath.KeyDelimiter;

            return _properties.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(key => Segment(key, prefix.Length))
                .Concat(earlierKeys)
                .OrderBy(k => k, ConfigurationKeyComparer.Instance);
        }

        private static string Segment(string key, int prefixLength)
        {
            var indexOf = key.IndexOf(ConfigurationPath.KeyDelimiter, prefixLength, StringComparison.OrdinalIgnoreCase);
            return indexOf < 0 ? key.Substring(prefixLength) : key.Substring(prefixLength, indexOf - prefixLength);
        }

        public void Save()
        {
            var json = BeforeSave(System.Text.Json.JsonSerializer.Serialize(this, _type, _serializerOptions));

            Directory.CreateDirectory(ConfigDirPath);
            File.WriteAllText(LocalFilePath, json);

            _configurationRoot?.Reload();
        }

        private void OnReload()
        {
            var previousToken = Interlocked.Exchange(ref _reloadToken, new ConfigurationReloadToken());
            previousToken.OnReload();
        }

        public virtual string BeforeLoad(string json) => json;

        public virtual string BeforeSave(string json) => json;

        public override string ToString() => _type.Name;
    }
}