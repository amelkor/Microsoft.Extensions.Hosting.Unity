using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using UnityEngine;
using UnityEngine.Scripting;

namespace GenericHost.Samples
{
    [CreateAssetMenu(fileName = "ConfigurationScriptableObject", menuName = "Generic Host Sample/Configuration SO", order = 0)]
    public class ConfigurationScriptableObject : ScriptableObject
    {
        // Marking a field with ConfigurationKeyAttribute makes it available via IConfiguration
        [ConfigurationKey("Server.BaseUrl"), Tooltip("Backend server URL")]
        [SerializeField, Preserve] private string serverbaseUrl = "http://localhost:5001";
        
        [ConfigurationKey("SampleInteger"), Tooltip("Sample value")]
        [SerializeField, Preserve] private int sampleInt = 300;

        // This object marked to be registered as ScriptableObject singleton
        [RegisterScriptableObjectAsSingleton]
        [SerializeField, Preserve] private ColorsConfiguration colorsData;

        [Tooltip("Moving object prefab Resources path")]
        public string movingObjectPrefabName = "Gameplay/RandomMoveCube";
        
        /// <note>
        /// For HostBuilder. Get all objects marked with <see cref="RegisterScriptableObjectAsSingletonAttribute"/>.
        /// </note>
        internal Dictionary<Type, ScriptableObject> GetScriptableObjectsToRegisterAsSingeltons()
        {
            var objects = new Dictionary<Type, ScriptableObject>();

            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (fieldInfo.GetCustomAttribute<RegisterScriptableObjectAsSingletonAttribute>() == null)
                    continue;
                
                if (fieldInfo.FieldType.IsSubclassOf(typeof(ScriptableObject)))
                {
                    objects.Add(fieldInfo.FieldType, (ScriptableObject) fieldInfo.GetValue(this));
                }
            }

            return objects;
        }
    }
}