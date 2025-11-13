using Godot;
using autotown.Core;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Tree resource node that provides wood.
/// Extends HarvestableResource with tree-specific configuration from GameConfig.
/// </summary>
public partial class TreeNode : HarvestableResource
{
    public override void _Ready()
    {
        // Configure tree-specific properties from GameConfig
        ResourceType = ResourceType.Wood;
        MaxHarvests = GameConfig.TREE_MAX_HARVESTS;
        YieldPerHarvest = GameConfig.TREE_YIELD_PER_HARVEST;
        HarvestDuration = GameConfig.TREE_HARVEST_DURATION;

        // Call base _Ready to initialize
        base._Ready();
    }
}
