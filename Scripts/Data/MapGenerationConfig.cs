using Godot;

namespace autotown.Data;

/// <summary>
/// Configuration for procedural map generation.
/// Controls map dimensions, terrain generation, and resource spawning.
/// </summary>
[GlobalClass]
public partial class MapGenerationConfig : Resource
{
    // === Map Dimensions ===

    /// <summary>
    /// Width of the generated map in tiles.
    /// </summary>
    [ExportGroup("Map Dimensions")]
    [Export]
    public int MapWidth { get; set; } = 100;

    /// <summary>
    /// Height of the generated map in tiles.
    /// </summary>
    [Export]
    public int MapHeight { get; set; } = 100;

    /// <summary>
    /// Size of each tile in pixels.
    /// </summary>
    [Export]
    public int TileSize { get; set; } = 16;

    // === Terrain Generation ===

    /// <summary>
    /// Noise frequency for terrain generation (higher = more varied).
    /// </summary>
    [ExportGroup("Terrain Generation")]
    [Export(PropertyHint.Range, "0.0,1.0,0.01")]
    public float NoiseFrequency { get; set; } = 0.05f;

    /// <summary>
    /// Number of noise octaves for terrain detail.
    /// </summary>
    [Export(PropertyHint.Range, "1,8,1")]
    public int NoiseOctaves { get; set; } = 3;

    /// <summary>
    /// Threshold for water generation (-1 to 1, values below this become water).
    /// </summary>
    [Export(PropertyHint.Range, "-1.0,1.0,0.1")]
    public float WaterThreshold { get; set; } = -0.3f;

    /// <summary>
    /// Threshold for mountain generation (-1 to 1, values above this become mountains).
    /// </summary>
    [Export(PropertyHint.Range, "-1.0,1.0,0.1")]
    public float MountainThreshold { get; set; } = 0.5f;

    // === Tree Spawning ===

    /// <summary>
    /// Number of tree clusters to spawn across the map.
    /// </summary>
    [ExportGroup("Tree Spawning")]
    [Export]
    public int TreeClusterCount { get; set; } = 15;

    /// <summary>
    /// Number of trees per cluster (approximate).
    /// </summary>
    [Export]
    public int TreesPerCluster { get; set; } = 8;

    /// <summary>
    /// Radius of tree cluster spawn area in tiles.
    /// </summary>
    [Export]
    public int TreeClusterRadius { get; set; } = 4;

    // === Stone Spawning ===

    /// <summary>
    /// Number of stone deposit clusters to spawn.
    /// </summary>
    [ExportGroup("Stone Spawning")]
    [Export]
    public int StoneClusterCount { get; set; } = 10;

    /// <summary>
    /// Number of stone nodes per cluster (approximate).
    /// </summary>
    [Export]
    public int StonesPerCluster { get; set; } = 5;

    /// <summary>
    /// Radius of stone cluster spawn area in tiles.
    /// </summary>
    [Export]
    public int StoneClusterRadius { get; set; } = 3;

    // === Forage Spawning ===

    /// <summary>
    /// Number of foraging areas to spawn.
    /// </summary>
    [ExportGroup("Forage Spawning")]
    [Export]
    public int ForageAreaCount { get; set; } = 12;

    /// <summary>
    /// Number of forage nodes per area (approximate).
    /// </summary>
    [Export]
    public int ForagesPerArea { get; set; } = 6;

    /// <summary>
    /// Radius of foraging area spawn in tiles.
    /// </summary>
    [Export]
    public int ForageAreaRadius { get; set; } = 3;

    // === Global Settings ===

    /// <summary>
    /// Minimum spacing between resource clusters in tiles.
    /// </summary>
    [ExportGroup("Global Settings")]
    [Export]
    public int ResourceMinSpacing { get; set; } = 8;
}
