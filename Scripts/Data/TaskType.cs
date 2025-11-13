namespace autotown.Data;

/// <summary>
/// Defines the types of tasks that workers can perform.
/// </summary>
public enum TaskType
{
    /// <summary>Gather resources from resource nodes (trees, rocks, etc.)</summary>
    Gather,

    /// <summary>Construct buildings and structures</summary>
    Build,

    /// <summary>Process raw resources into refined materials</summary>
    Process,

    /// <summary>Transport resources between locations</summary>
    Haul
}
