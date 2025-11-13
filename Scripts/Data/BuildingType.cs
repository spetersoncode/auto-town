namespace autotown.Data;

/// <summary>
/// Defines the types of buildings that can be constructed.
/// </summary>
public enum BuildingType
{
    /// <summary>Central administrative building where workers spawn</summary>
    TownHall,

    /// <summary>Storage building for resources</summary>
    Stockpile,

    /// <summary>Residential building that increases population capacity</summary>
    House,

    /// <summary>Processes wood into lumber</summary>
    Sawmill,

    /// <summary>Extracts stone from nearby deposits</summary>
    Mine,

    /// <summary>Grows crops for food production</summary>
    Farm
}
