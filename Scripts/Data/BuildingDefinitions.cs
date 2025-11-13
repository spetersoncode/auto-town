using System.Collections.Generic;
using autotown.Core;

namespace autotown.Data;

/// <summary>
/// Static definitions for all building types in the game.
/// Provides centralized building data to avoid duplication.
/// </summary>
public static class BuildingDefinitions
{
    /// <summary>
    /// Gets the building data for a specific building type.
    /// </summary>
    /// <param name="type">The type of building</param>
    /// <returns>BuildingData instance with costs, build time, and properties</returns>
    public static BuildingData GetBuildingData(BuildingType type)
    {
        return type switch
        {
            BuildingType.House => CreateHouseData(),
            BuildingType.Sawmill => CreateSawmillData(),
            BuildingType.Mine => CreateMineData(),
            BuildingType.Farm => CreateFarmData(),
            BuildingType.TownHall => CreateTownHallData(),
            BuildingType.Stockpile => CreateStockpileData(),
            _ => new BuildingData()
        };
    }

    /// <summary>
    /// Creates building data for a House.
    /// </summary>
    private static BuildingData CreateHouseData()
    {
        return new BuildingData
        {
            Type = BuildingType.House,
            Name = "House",
            Description = "Provides housing for population growth",
            BuildTime = GameConfig.HOUSE_BUILD_TIME,
            Cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, GameConfig.HOUSE_COST_WOOD },
                { ResourceType.Stone, GameConfig.HOUSE_COST_STONE }
            },
            Width = 2,
            Height = 2
        };
    }

    /// <summary>
    /// Creates building data for a Sawmill.
    /// </summary>
    private static BuildingData CreateSawmillData()
    {
        return new BuildingData
        {
            Type = BuildingType.Sawmill,
            Name = "Sawmill",
            Description = "Processes wood periodically",
            BuildTime = GameConfig.SAWMILL_BUILD_TIME,
            Cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, GameConfig.SAWMILL_COST_WOOD },
                { ResourceType.Stone, GameConfig.SAWMILL_COST_STONE }
            },
            Width = 2,
            Height = 2
        };
    }

    /// <summary>
    /// Creates building data for a Mine.
    /// </summary>
    private static BuildingData CreateMineData()
    {
        return new BuildingData
        {
            Type = BuildingType.Mine,
            Name = "Mine",
            Description = "Extracts stone periodically",
            BuildTime = GameConfig.MINE_BUILD_TIME,
            Cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, GameConfig.MINE_COST_WOOD },
                { ResourceType.Stone, GameConfig.MINE_COST_STONE }
            },
            Width = 2,
            Height = 2
        };
    }

    /// <summary>
    /// Creates building data for a Farm.
    /// </summary>
    private static BuildingData CreateFarmData()
    {
        return new BuildingData
        {
            Type = BuildingType.Farm,
            Name = "Farm",
            Description = "Produces food periodically",
            BuildTime = GameConfig.FARM_BUILD_TIME,
            Cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, GameConfig.FARM_COST_WOOD },
                { ResourceType.Stone, GameConfig.FARM_COST_STONE }
            },
            Width = 3,
            Height = 2
        };
    }

    /// <summary>
    /// Creates building data for a TownHall.
    /// </summary>
    private static BuildingData CreateTownHallData()
    {
        return new BuildingData
        {
            Type = BuildingType.TownHall,
            Name = "Town Hall",
            Description = "Central building where workers spawn",
            BuildTime = GameConfig.TOWNHALL_BUILD_TIME,
            Cost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Wood, GameConfig.TOWNHALL_COST_WOOD },
                { ResourceType.Stone, GameConfig.TOWNHALL_COST_STONE }
            },
            Width = 3,
            Height = 3
        };
    }

    /// <summary>
    /// Creates building data for a Stockpile.
    /// </summary>
    private static BuildingData CreateStockpileData()
    {
        return new BuildingData
        {
            Type = BuildingType.Stockpile,
            Name = "Stockpile",
            Description = "Stores resources",
            BuildTime = 0f, // No construction time for starting stockpile
            Cost = new Dictionary<ResourceType, int>(), // Free
            Width = 2,
            Height = 2
        };
    }

    /// <summary>
    /// Gets all buildable building types (excludes special buildings like TownHall, Stockpile).
    /// </summary>
    /// <returns>List of building types that can be placed by the player</returns>
    public static List<BuildingType> GetBuildableTypes()
    {
        return new List<BuildingType>
        {
            BuildingType.House,
            BuildingType.Sawmill,
            BuildingType.Mine,
            BuildingType.Farm
        };
    }
}
