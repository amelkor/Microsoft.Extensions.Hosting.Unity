// ReSharper disable once CheckNamespace

using System;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddScriptableObjectConfiguration<T>(this IConfigurationBuilder builder, string resourcesPath) where T : UnityEngine.ScriptableObject
        {
            if (string.IsNullOrWhiteSpace(resourcesPath))
                throw new ArgumentNullException(nameof(resourcesPath));
            
            return builder.Add(new ScriptableObjectConfigurationSource(typeof(T), resourcesPath));
        }
        
        public static IConfigurationBuilder AddScriptableObjectConfiguration<T>(this IConfigurationBuilder builder, T scriptableObject) where T : UnityEngine.ScriptableObject
        {
            if (!scriptableObject)
                throw new ArgumentNullException(nameof(scriptableObject));
            
            return builder.Add(new ScriptableObjectConfigurationSource(typeof(T), scriptableObject));
        }
    }
}