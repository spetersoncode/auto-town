using Godot;

namespace autotown.Data;

/// <summary>
/// Resource class defining the properties and costs for a building type.
/// Allows inspector-based configuration and runtime tweaking of building parameters.
/// </summary>
[GlobalClass]
public partial class BuildingResource : Resource
{
    /// <summary>
    /// Display name of the building.
    /// </summary>
    [Export]
    public string BuildingName { get; set; } = "Building";

    // === Construction Costs ===

    /// <summary>
    /// Wood cost for building construction.
    /// </summary>
    [Export]
    public int WoodCost { get; set; } = 0;

    /// <summary>
    /// Stone cost for building construction.
    /// </summary>
    [Export]
    public int StoneCost { get; set; } = 0;

    /// <summary>
    /// Food cost for building construction (currently unused, future-proofing).
    /// </summary>
    [Export]
    public int FoodCost { get; set; } = 0;

    /// <summary>
    /// Time required to construct the building in seconds.
    /// </summary>
    [Export]
    public float BuildTime { get; set; } = 5.0f;

    // === Production Configuration ===

    /// <summary>
    /// Housing capacity provided by this building (0 if not a house).
    /// </summary>
    [Export]
    public int HousingCapacity { get; set; } = 0;

    /// <summary>
    /// Amount of resources produced per processing task (0 if not a production building).
    /// </summary>
    [Export]
    public int ProductionOutput { get; set; } = 0;

    /// <summary>
    /// Time required to complete one production task in seconds (0 if not a production building).
    /// </summary>
    [Export]
    public float ProductionTime { get; set; } = 0f;

    /// <summary>
    /// Type of resource produced (Wood, Stone, Food). Empty if not a production building.
    /// </summary>
    [Export]
    public string ProductionResourceType { get; set; } = "";
}
