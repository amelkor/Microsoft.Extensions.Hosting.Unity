using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ConfigurationKeyAttribute : Attribute
    {
        public ConfigurationKeyAttribute(string name) => Name = name;

        public string Name { get; }
    }
}