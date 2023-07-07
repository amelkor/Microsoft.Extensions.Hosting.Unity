using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace GenericHost.Samples
{
    public class SampleUI : MonoBehaviour
    {
        private IServiceProvider _services;
        private Microsoft.Extensions.Logging.ILogger _logger;
        private UIDocument _uiDocument;
        private ColorsConfiguration _colors;

        private int _spawnsCounter;
        private Label _label;

        public string InfoText { set => _label.text = value; }

        private void AwakeServices(IServiceProvider services)
        {
            _services = services;
            _logger = _services.GetRequiredService<ILogger<SampleUI>>();
            _colors = _services.GetRequiredService<ColorsConfiguration>();
        }

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            var root = _uiDocument.rootVisualElement;

            _label = root.Q<Label>("info-label");

            root.Q<Button>("spawn-button").clicked += OnSpawnClicked;
            root.Q<Button>("transient-button").clicked += OnTransientClicked;
        }

        private void OnSpawnClicked()
        {
            var spawned = _services.GetRequiredService<RandomMoveObject>();
            var color = _colors.GetRandomColor();

            spawned.GetComponent<Renderer>().material.color = color;

            _label.text = $"Spawned {++_spawnsCounter} objects";

            _logger.LogInformation("Spawned new {ObjectType} with color {ColorCode}", nameof(RandomMoveObject), color.ToString());
        }
        
        private void OnTransientClicked()
        {
            _services.GetRequiredService<MonoTransient>();
            
            _label.text = $"Spawned {++_spawnsCounter} objects";

            _logger.LogInformation("Spawned new {ObjectType} object", nameof(MonoTransient));
        }
    }
}