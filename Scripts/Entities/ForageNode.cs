using Godot;
using autotown.Core;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Forage resource node that provides food.
/// Extends HarvestableResource with forage-specific configuration from GameConfig.
/// </summary>
public partial class ForageNode : HarvestableResource
{
    public override void _Ready()
    {
        // Configure forage-specific properties from GameConfig
        ResourceType = ResourceType.Food;
        MaxHarvests = GameConfig.FORAGE_MAX_HARVESTS;
        YieldPerHarvest = GameConfig.FORAGE_YIELD_PER_HARVEST;
        HarvestDuration = GameConfig.FORAGE_HARVEST_DURATION;

        // Call base _Ready to initialize
        base._Ready();
    }
}
