using Godot;
using autotown.Core;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Stone resource node that provides stone.
/// Extends HarvestableResource with stone-specific configuration from GameConfig.
/// </summary>
public partial class StoneNode : HarvestableResource
{
    public override void _Ready()
    {
        // Configure stone-specific properties from GameConfig
        ResourceType = ResourceType.Stone;
        MaxHarvests = GameConfig.STONE_MAX_HARVESTS;
        YieldPerHarvest = GameConfig.STONE_YIELD_PER_HARVEST;
        HarvestDuration = GameConfig.STONE_HARVEST_DURATION;

        // Call base _Ready to initialize
        base._Ready();
    }
}
