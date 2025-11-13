using Godot;
using autotown.Data;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI component that displays current resource amounts.
/// </summary>
public partial class ResourceDisplayUI : PanelContainer
{
    private Label _woodLabel;
    private Label _stoneLabel;
    private Label _foodLabel;
    private ResourceManager _resourceManager;

    public override void _Ready()
    {
        // Get ResourceManager reference
        _resourceManager = GetNode<ResourceManager>("/root/ResourceManager");

        // Get child labels
        _woodLabel = GetNode<Label>("VBoxContainer/WoodRow/WoodLabel");
        _stoneLabel = GetNode<Label>("VBoxContainer/StoneRow/StoneLabel");
        _foodLabel = GetNode<Label>("VBoxContainer/FoodRow/FoodLabel");

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

    private void OnResourceChanged(ResourceType type, int newAmount)
    {
        UpdateResourceDisplay(type, newAmount);
    }

    private void UpdateResourceDisplay(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Wood:
                _woodLabel.Text = $"{amount}";
                break;
            case ResourceType.Stone:
                _stoneLabel.Text = $"{amount}";
                break;
            case ResourceType.Food:
                _foodLabel.Text = $"{amount}";
                break;
        }
    }

    private void UpdateAllResources()
    {
        UpdateResourceDisplay(ResourceType.Wood, _resourceManager.GetResourceAmount(ResourceType.Wood));
        UpdateResourceDisplay(ResourceType.Stone, _resourceManager.GetResourceAmount(ResourceType.Stone));
        UpdateResourceDisplay(ResourceType.Food, _resourceManager.GetResourceAmount(ResourceType.Food));
    }
}
