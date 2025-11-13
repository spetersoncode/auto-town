using Godot;

namespace autotown.Data;

/// <summary>
/// Configuration for game balance parameters.
/// Controls starting resources, stockpile capacity, and construction parameters.
/// </summary>
[GlobalClass]
public partial class GameBalanceConfig : Resource
{
    // === Starting Resources ===

    /// <summary>
    /// Starting wood when a new game begins.
    /// </summary>
    [ExportGroup("Starting Resources")]
    [Export]
    public int StartingWood { get; set; } = 50;

    /// <summary>
    /// Starting stone when a new game begins.
    /// </summary>
    [Export]
    public int StartingStone { get; set; } = 30;

    /// <summary>
    /// Starting food when a new game begins.
    /// </summary>
    [Export]
    public int StartingFood { get; set; } = 100;

    // === Storage ===

    /// <summary>
    /// Maximum capacity per resource type in stockpile (0 = unlimited).
    /// </summary>
    [ExportGroup("Storage")]
    [Export]
    public int StockpileCapacity { get; set; } = 0;

    // === Construction ===

    /// <summary>
    /// Amount of resources hauled per trip to construction site.
    /// </summary>
    [ExportGroup("Construction")]
    [Export]
    public int ConstructionHaulAmount { get; set; } = 20;
}
