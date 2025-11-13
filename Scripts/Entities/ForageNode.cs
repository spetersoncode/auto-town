using Godot;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Forage resource node that provides food.
/// Extends HarvestableResource with forage-specific configuration from resource files.
/// </summary>
public partial class ForageNode : HarvestableResource
{
    public override void _Ready()
    {
        // Load forage configuration from resource file
        var config = GD.Load<HarvestableResourceConfig>("res://Resources/Harvestables/ForageConfig.tres");

        // Configure forage properties from config
        ResourceType = config.ResourceType;
        MaxHarvests = config.MaxHarvests;
        YieldPerHarvest = config.YieldPerHarvest;
        HarvestDuration = config.HarvestDuration;

        // Call base _Ready to initialize
        base._Ready();
    }
}
