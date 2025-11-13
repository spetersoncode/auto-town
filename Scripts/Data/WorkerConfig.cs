using Godot;

namespace autotown.Data;

/// <summary>
/// Configuration for worker behavior and navigation.
/// Controls movement speed, interaction range, and pathfinding parameters.
/// </summary>
[GlobalClass]
public partial class WorkerConfig : Resource
{
    // === Worker Movement ===

    /// <summary>
    /// Default movement speed for workers in pixels per second.
    /// </summary>
    [ExportGroup("Worker Movement")]
    [Export]
    public float MovementSpeed { get; set; } = 100.0f;

    /// <summary>
    /// Range in pixels within which a worker can interact with resources/buildings.
    /// </summary>
    [Export]
    public float InteractionRange { get; set; } = 24.0f;

    /// <summary>
    /// Default work efficiency multiplier (1.0 = 100% efficiency).
    /// </summary>
    [Export(PropertyHint.Range, "0.0,2.0,0.1")]
    public float Efficiency { get; set; } = 1.0f;

    // === Navigation Agent ===

    /// <summary>
    /// Maximum speed for NavigationAgent2D in pixels per second.
    /// </summary>
    [ExportGroup("Navigation Agent")]
    [Export]
    public float NavAgentMaxSpeed { get; set; } = 100.0f;

    /// <summary>
    /// Path desired distance - how close the agent gets to target before considering it reached.
    /// </summary>
    [Export]
    public float PathDesiredDistance { get; set; } = 8.0f;

    /// <summary>
    /// Target desired distance - how close to final target before stopping.
    /// </summary>
    [Export]
    public float TargetDesiredDistance { get; set; } = 10.0f;

    /// <summary>
    /// Radius of the navigation agent for collision avoidance.
    /// </summary>
    [Export]
    public float NavAgentRadius { get; set; } = 8.0f;

    // === Navigation Mesh ===

    /// <summary>
    /// Navigation mesh cell size in pixels (affects pathfinding precision).
    /// </summary>
    [ExportGroup("Navigation Mesh")]
    [Export]
    public float NavMeshCellSize { get; set; } = 8.0f;

    /// <summary>
    /// Border size around navigation mesh obstacles.
    /// </summary>
    [Export]
    public float NavMeshBorderSize { get; set; } = 4.0f;
}
