using Godot;
using autotown.Entities;
using autotown.Systems;

namespace autotown.Core;

/// <summary>
/// Testing utility for manually testing resource harvesting by clicking on resource nodes.
/// TEMPORARY: This will be removed once the worker system is implemented in Phase 4.
/// </summary>
public partial class ResourceHarvestTester : Node2D
{
    private HarvestableResource _selectedResource = null;
    private Label _debugLabel;

    public override void _Ready()
    {
        // Create a debug label to show instructions
        _debugLabel = new Label();
        _debugLabel.Position = new Vector2(10, 10);
        _debugLabel.Text = "Click on resource nodes to harvest them!\nPress H to start harvest, C to complete harvest, D to deplete.\nPress R to view ResourceManager status.";
        _debugLabel.AddThemeColorOverride("font_color", Colors.White);
        AddChild(_debugLabel);

        GD.Print("[ResourceHarvestTester] Ready! Click on resource nodes to test harvesting.");
        GD.Print("[ResourceHarvestTester] Controls: H=Start Harvest, C=Complete Harvest, D=Deplete, R=Show Resources");
    }

    public override void _Input(InputEvent @event)
    {
        // Handle mouse clicks to select resource nodes
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            // Try to find resource node at click position
            var camera = GetViewport().GetCamera2D();
            if (camera != null)
            {
                Vector2 globalClickPos = mouseEvent.Position + camera.GlobalPosition - GetViewportRect().Size / 2;

                // Find all HarvestableResource nodes
                Godot.Collections.Array<Node> resourceNodes = FindResourceNodes(GetTree().Root);

                foreach (Node node in resourceNodes)
                {
                    if (node is HarvestableResource resource)
                    {
                        float distance = resource.GlobalPosition.DistanceTo(globalClickPos);
                        if (distance < 20) // Within 20 pixels
                        {
                            _selectedResource = resource;
                            GD.Print($"[ResourceHarvestTester] Selected: {resource.GetDebugInfo()}");
                            UpdateDebugLabel();
                            return;
                        }
                    }
                }
            }
        }

        // Handle keyboard commands
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (_selectedResource != null)
            {
                switch (keyEvent.Keycode)
                {
                    case Key.H: // Start harvest
                        if (_selectedResource.ReserveForHarvest(this))
                        {
                            _selectedResource.StartHarvest(this);
                            GD.Print($"[ResourceHarvestTester] Started harvest: {_selectedResource.GetDebugInfo()}");
                        }
                        else
                        {
                            GD.Print("[ResourceHarvestTester] Cannot start harvest - resource unavailable or already reserved");
                        }
                        UpdateDebugLabel();
                        break;

                    case Key.C: // Complete harvest
                        _selectedResource.CompleteHarvest();
                        GD.Print($"[ResourceHarvestTester] Completed harvest: {_selectedResource.GetDebugInfo()}");
                        UpdateDebugLabel();
                        break;

                    case Key.D: // Deplete resource
                        _selectedResource.Deplete();
                        GD.Print($"[ResourceHarvestTester] Depleted resource: {_selectedResource.GetDebugInfo()}");
                        UpdateDebugLabel();
                        break;
                }
            }

            if (keyEvent.Keycode == Key.R) // Show resource status
            {
                ShowResourceStatus();
            }
        }
    }

    private Godot.Collections.Array<Node> FindResourceNodes(Node root)
    {
        var result = new Godot.Collections.Array<Node>();
        SearchForResources(root, result);
        return result;
    }

    private void SearchForResources(Node node, Godot.Collections.Array<Node> result)
    {
        if (node is HarvestableResource)
        {
            result.Add(node);
        }

        foreach (Node child in node.GetChildren())
        {
            SearchForResources(child, result);
        }
    }

    private void UpdateDebugLabel()
    {
        if (_selectedResource != null)
        {
            _debugLabel.Text = $"Selected Resource: {_selectedResource.GetDebugInfo()}\n" +
                              "H=Start Harvest | C=Complete Harvest | D=Deplete | R=Show Resources";
        }
    }

    private void ShowResourceStatus()
    {
        var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        var resources = resourceManager.GetAllResources();

        GD.Print("=== RESOURCE STATUS ===");
        foreach (var kvp in resources)
        {
            GD.Print($"{kvp.Key}: {kvp.Value}");
        }
        GD.Print("======================");
    }

    public override void _Process(double delta)
    {
        // Auto-complete harvests for testing
        if (_selectedResource != null && _selectedResource.State == HarvestableResource.HarvestState.BeingHarvested)
        {
            // The HarvestableResource will auto-complete when progress reaches 1.0
            // Just update the debug label
            UpdateDebugLabel();
        }
    }
}
