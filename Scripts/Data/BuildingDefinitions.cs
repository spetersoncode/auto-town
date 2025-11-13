using System.Collections.Generic;
using Godot;
using autotown.Core;

namespace autotown.Data;

/// <summary>
/// Static definitions for all building types in the game.
/// Loads building data from resource files for inspector-based configuration.
/// </summary>
public static class BuildingDefinitions
{
    // Cache for loaded building resources
    private static readonly Dictionary<BuildingType, BuildingResource> _resourceCache = new();

    /// <summary>
    /// Gets the building data for a specific building type.
    /// </summary>
    /// <param name="type">The type of building</param>
    /// <returns>BuildingData instance with costs, build time, and properties</returns>
    public static BuildingData GetBuildingData(BuildingType type)
    {
        return type switch
        {
            BuildingType.House => CreateBuildingDataFromResource(type, "res://Resources/Buildings/House.tres", 2, 2, "Provides housing for population growth"),
            BuildingType.Sawmill => CreateBuildingDataFromResource(type, "res://Resources/Buildings/Sawmill.tres", 2, 2, "Processes wood periodically"),
            BuildingType.Mine => CreateBuildingDataFromResource(type, "res://Resources/Buildings/Mine.tres", 2, 2, "Extracts stone periodically"),
            BuildingType.Farm => CreateBuildingDataFromResource(type, "res://Resources/Buildings/Farm.tres", 3, 2, "Produces food periodically"),
            BuildingType.TownHall => CreateBuildingDataFromResource(type, "res://Resources/Buildings/TownHall.tres", 3, 3, "Central building where workers spawn"),
            BuildingType.Stockpile => CreateStockpileData(),
            _ => new BuildingData()
        };
    }

    /// <summary>
    /// Creates BuildingData from a BuildingResource file.
    /// </summary>
    private static BuildingData CreateBuildingDataFromResource(BuildingType type, string resourcePath, int width, int height, string description)
    {
        var resource = LoadBuildingResource(type, resourcePath);

        var data = new BuildingData
        {
            Type = type,
            Name = resource.BuildingName,
            Description = description,
            BuildTime = resource.BuildTime,
            Cost = new Dictionary<ResourceType, int>(),
            Width = width,
            Height = height
        };

        // Add costs to dictionary (only if > 0)
        if (resource.WoodCost > 0)
            data.Cost[ResourceType.Wood] = resource.WoodCost;
        if (resource.StoneCost > 0)
            data.Cost[ResourceType.Stone] = resource.StoneCost;
        if (resource.FoodCost > 0)
            data.Cost[ResourceType.Food] = resource.FoodCost;

        return data;
    }

    /// <summary>
    /// Loads a BuildingResource from file, with caching.
    /// </summary>
    private static BuildingResource LoadBuildingResource(BuildingType type, string path)
    {
        if (_resourceCache.TryGetValue(type, out var cached))
            return cached;

        var resource = GD.Load<BuildingResource>(path);
        if (resource == null)
        {
            GD.PushError($"[BuildingDefinitions] Failed to load resource at {path}");
            return new BuildingResource(); // Return default
        }

        _resourceCache[type] = resource;
        return resource;
    }

    /// <summary>
    /// Gets the BuildingResource for a specific building type.
    /// Useful for accessing production data and other resource-specific properties.
    /// </summary>
    public static BuildingResource GetBuildingResource(BuildingType type)
    {
        return type switch
        {
            BuildingType.House => LoadBuildingResource(type, "res://Resources/Buildings/House.tres"),
            BuildingType.Sawmill => LoadBuildingResource(type, "res://Resources/Buildings/Sawmill.tres"),
            BuildingType.Mine => LoadBuildingResource(type, "res://Resources/Buildings/Mine.tres"),
            BuildingType.Farm => LoadBuildingResource(type, "res://Resources/Buildings/Farm.tres"),
            BuildingType.TownHall => LoadBuildingResource(type, "res://Resources/Buildings/TownHall.tres"),
            _ => new BuildingResource()
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
