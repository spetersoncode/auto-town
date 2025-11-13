namespace autotown.Data;

/// <summary>
/// Defines the job roles that workers can be assigned to.
/// </summary>
public enum JobType
{
    /// <summary>No job assigned - worker is idle</summary>
    None,

    /// <summary>Chops down trees to gather wood</summary>
    Lumberjack,

    /// <summary>Mines stone from rock deposits</summary>
    Miner,

    /// <summary>Gathers food from natural sources</summary>
    Forager,

    /// <summary>Grows and harvests crops for food</summary>
    Farmer,

    /// <summary>Constructs buildings and structures</summary>
    Builder
}
