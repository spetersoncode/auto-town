using Godot;
using System.Collections.Generic;
using autotown.Data;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI component that displays current resource amounts in a compact horizontal format.
/// Shows format: 🌲100 🪨50 🍓25
/// </summary>
public partial class ResourceDisplayUI : HBoxContainer
{
    private ResourceManager _resourceManager;
    private Dictionary<ResourceType, Label> _resourceLabels = new Dictionary<ResourceType, Label>();

    // Resource icons and colors
    private readonly Dictionary<ResourceType, string> _resourceIcons = new Dictionary<ResourceType, string>
    {
        { ResourceType.Wood, "🌲" },
        { ResourceType.Stone, "🪨" },
        { ResourceType.Food, "🍓" }
    };

    private readonly Dictionary<ResourceType, Color> _resourceColors = new Dictionary<ResourceType, Color>
    {
        { ResourceType.Wood, new Color(0.05f, 0.55f, 0.05f) },  // Green
        { ResourceType.Stone, new Color(0.5f, 0.5f, 0.5f) },    // Gray
        { ResourceType.Food, new Color(1f, 0.84f, 0f) }         // Gold
    };

    public override void _Ready()
    {
        // Get ResourceManager reference
        _resourceManager = GetNode<ResourceManager>("/root/ResourceManager");

        // Create resource labels
        CreateResourceLabels();

        // Subscribe to resource change signals
        _resourceManager.ResourceChanged += OnResourceChanged;

        // Initial update
        UpdateAllResources();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (_resourceManager != null)
        {
            _resourceManager.ResourceChanged -= OnResourceChanged;
        }
    }

    private void CreateResourceLabels()
    {
        // Add spacing control at start
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(10, 0);
        AddChild(spacer);

        // Create labels for each resource type
        ResourceType[] displayOrder = { ResourceType.Wood, ResourceType.Stone, ResourceType.Food };

        foreach (var resourceType in displayOrder)
        {
            var label = new Label();
            label.Text = $"{_resourceIcons[resourceType]}0";
            label.Modulate = _resourceColors[resourceType];

            // Add margin between labels
            if (_resourceLabels.Count > 0)
            {
                var margin = new Control();
                margin.CustomMinimumSize = new Vector2(8, 0);
                AddChild(margin);
            }

            AddChild(label);
            _resourceLabels[resourceType] = label;
        }
    }

    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        UpdateResourceDisplay(type, newAmount);
    }

    private void UpdateResourceDisplay(ResourceType type, int amount)
    {
        if (_resourceLabels.ContainsKey(type))
        {
            _resourceLabels[type].Text = $"{_resourceIcons[type]}{amount}";
        }
    }

    private void UpdateAllResources()
    {
        UpdateResourceDisplay(ResourceType.Wood, _resourceManager.GetResourceAmount(ResourceType.Wood));
        UpdateResourceDisplay(ResourceType.Stone, _resourceManager.GetResourceAmount(ResourceType.Stone));
        UpdateResourceDisplay(ResourceType.Food, _resourceManager.GetResourceAmount(ResourceType.Food));
    }
}
