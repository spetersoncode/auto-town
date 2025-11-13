using Godot;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Stone resource node that provides stone.
/// Extends HarvestableResource with stone-specific configuration from resource files.
/// </summary>
public partial class StoneNode : HarvestableResource
{
    public override void _Ready()
    {
        // Load stone configuration from resource file
        var config = GD.Load<HarvestableResourceConfig>("res://Resources/Harvestables/StoneConfig.tres");

        // Configure stone properties from config
        ResourceType = config.ResourceType;
        MaxHarvests = config.MaxHarvests;
        YieldPerHarvest = config.YieldPerHarvest;
        HarvestDuration = config.HarvestDuration;

        // Call base _Ready to initialize
        base._Ready();
    }
}
