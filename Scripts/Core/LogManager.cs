using Godot;

namespace autotown.Core;

/// <summary>
/// Centralized logging manager with per-system debug flags.
/// Set flags to true to enable debug logging for specific systems.
/// </summary>
public static class LogManager
{
    // === System Debug Flags ===

    /// <summary>
    /// Enable debug logs for BuildingManager (placement, construction workflow)
    /// </summary>
    public const bool DEBUG_BUILDING_MANAGER = false;

    /// <summary>
    /// Enable debug logs for ConstructionSite (resource delivery, construction progress)
    /// </summary>
    public const bool DEBUG_CONSTRUCTION_SITE = false;

    /// <summary>
    /// Enable debug logs for BuildTask (construction work progress)
    /// </summary>
    public const bool DEBUG_BUILD_TASK = false;

    /// <summary>
    /// Enable debug logs for HaulResourceTask (hauling resources to construction sites)
    /// </summary>
    public const bool DEBUG_HAUL_TASK = false;

    /// <summary>
    /// Enable debug logs for Worker (movement, task assignment, state changes)
    /// </summary>
    public const bool DEBUG_WORKER = false;

    /// <summary>
    /// Enable debug logs for WorkerManager (spawning, job assignment)
    /// </summary>
    public const bool DEBUG_WORKER_MANAGER = false;

    /// <summary>
    /// Enable debug logs for TaskManager (task creation, assignment, completion)
    /// </summary>
    public const bool DEBUG_TASK_MANAGER = false;

    /// <summary>
    /// Enable debug logs for GatherTask (resource harvesting)
    /// </summary>
    public const bool DEBUG_GATHER_TASK = false;

    /// <summary>
    /// Enable debug logs for HarvestableResource (resource nodes, depletion)
    /// </summary>
    public const bool DEBUG_HARVESTABLE_RESOURCE = false;

    /// <summary>
    /// Enable debug logs for ResourceManager (resource tracking, stockpile)
    /// </summary>
    public const bool DEBUG_RESOURCE_MANAGER = false;

    /// <summary>
    /// Enable debug logs for Stockpile (deposits, withdrawals)
    /// </summary>
    public const bool DEBUG_STOCKPILE = false;

    /// <summary>
    /// Enable debug logs for ProcessTask (building processing)
    /// </summary>
    public const bool DEBUG_PROCESS_TASK = false;

    /// <summary>
    /// Enable debug logs for WorldController (initialization, setup)
    /// </summary>
    public const bool DEBUG_WORLD_CONTROLLER = false;

    /// <summary>
    /// Enable debug logs for BuildingPlacementUI (keyboard input, placement)
    /// </summary>
    public const bool DEBUG_PLACEMENT_UI = false;

    // === Helper Methods ===

    /// <summary>
    /// Prints a debug message if the specified flag is enabled.
    /// </summary>
    /// <param name="enabled">The debug flag for this system</param>
    /// <param name="message">The message to print</param>
    public static void Log(bool enabled, string message)
    {
        if (enabled)
        {
            GD.Print(message);
        }
    }

    /// <summary>
    /// Prints a warning message (always shown, regardless of debug flags).
    /// </summary>
    public static void Warning(string message)
    {
        GD.PushWarning(message);
    }

    /// <summary>
    /// Prints an error message (always shown, regardless of debug flags).
    /// </summary>
    public static void Error(string message)
    {
        GD.PushError(message);
    }
}
