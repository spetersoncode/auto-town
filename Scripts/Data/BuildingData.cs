using System.Collections.Generic;

namespace autotown.Data;

/// <summary>
/// Defines the properties and requirements for a building type.
/// </summary>
public class BuildingData
{
    /// <summary>Type of building</summary>
    public BuildingType Type { get; set; }

    /// <summary>Display name for the building</summary>
    public string Name { get; set; }

    /// <summary>Description of the building's function</summary>
    public string Description { get; set; }

    /// <summary>Time required to construct the building (in seconds)</summary>
    public float BuildTime { get; set; }

    /// <summary>Resources required to construct the building</summary>
    public Dictionary<ResourceType, int> Cost { get; set; }

    /// <summary>Width of the building in tiles</summary>
    public int Width { get; set; }

    /// <summary>Height of the building in tiles</summary>
    public int Height { get; set; }

    /// <summary>
    /// Initializes a new building data instance.
    /// </summary>
    public BuildingData()
    {
        Name = string.Empty;
        Description = string.Empty;
        BuildTime = 5.0f;
        Cost = new Dictionary<ResourceType, int>();
        Width = 1;
        Height = 1;
    }

    /// <summary>
    /// Checks if the provided resources are sufficient to build this building.
    /// </summary>
    /// <param name="resources">The available resources to check</param>
    /// <returns>True if sufficient resources are available</returns>
    public bool CanAfford(ResourceData resources)
    {
        foreach (var requirement in Cost)
        {
            if (!resources.HasEnough(requirement.Key, requirement.Value))
                return false;
        }
        return true;
    }
}
