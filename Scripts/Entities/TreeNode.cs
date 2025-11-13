using Godot;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Tree resource node that provides wood.
/// Extends HarvestableResource with tree-specific configuration from resource files.
/// </summary>
public partial class TreeNode : HarvestableResource
{
    public override void _Ready()
    {
        // Load tree configuration from resource file
        var config = GD.Load<HarvestableResourceConfig>("res://Resources/Harvestables/TreeConfig.tres");

        // Configure tree properties from config
        ResourceType = config.ResourceType;
        MaxHarvests = config.MaxHarvests;
        YieldPerHarvest = config.YieldPerHarvest;
        HarvestDuration = config.HarvestDuration;

        // Call base _Ready to initialize
        base._Ready();
    }
}
