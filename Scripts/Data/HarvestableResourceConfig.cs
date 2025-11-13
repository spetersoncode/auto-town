using Godot;

namespace autotown.Data;

/// <summary>
/// Resource class defining harvesting parameters for resource nodes (trees, stone, forage).
/// Allows inspector-based configuration and runtime tweaking of resource harvesting.
/// </summary>
[GlobalClass]
public partial class HarvestableResourceConfig : Resource
{
    /// <summary>
    /// Type of resource this config represents (Wood, Stone, Food).
    /// Note: Godot can't export enums in Resources, so this is stored as int.
    /// 0 = Wood, 1 = Stone, 2 = Food
    /// </summary>
    [Export]
    public int ResourceTypeIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets the resource type (mapped from ResourceTypeIndex).
    /// </summary>
    public ResourceType ResourceType
    {
        get => (ResourceType)ResourceTypeIndex;
        set => ResourceTypeIndex = (int)value;
    }

    /// <summary>
    /// Maximum number of times this resource can be harvested before depletion.
    /// </summary>
    [Export]
    public int MaxHarvests { get; set; } = 5;

    /// <summary>
    /// Amount of resource gathered per harvest action.
    /// </summary>
    [Export]
    public int YieldPerHarvest { get; set; } = 10;

    /// <summary>
    /// Time required to complete one harvest action in seconds.
    /// </summary>
    [Export]
    public float HarvestDuration { get; set; } = 2.0f;
}
