using Godot;
using System.Collections.Generic;
using System.Linq;
using autotown.Core;
using autotown.Data;
using autotown.Entities;

namespace autotown.Systems;

/// <summary>
/// Manages all buildings and construction sites in the game.
/// Handles building placement, construction task creation, and building lifecycle.
/// </summary>
public partial class BuildingManager : Node
{
    // === Signals ===

    [Signal]
    public delegate void BuildingPlacedEventHandler(Building building);

    [Signal]
    public delegate void ConstructionStartedEventHandler(ConstructionSite site);

    [Signal]
    public delegate void ConstructionCompletedEventHandler(Building building);

    [Signal]
    public delegate void BuildingActivatedEventHandler(Building building);

    [Signal]
    public delegate void BuildingDestroyedEventHandler(Building building);

    // === Private Fields ===

    private List<Building> _buildings = new List<Building>();
    private List<ConstructionSite> _constructionSites = new List<ConstructionSite>();
    private TaskManager _taskManager;
    private ResourceManager _resourceManager;
    private Stockpile _mainStockpile;

    // === Lifecycle ===

    public override void _Ready()
    {
        _taskManager = GetNode<TaskManager>("/root/TaskManager");
        _resourceManager = GetNode<ResourceManager>("/root/ResourceManager");

        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, "[BuildingManager] Initialized");
    }

    /// <summary>
    /// Sets the main stockpile reference for resource hauling.
    /// Should be called after the world is initialized.
    /// </summary>
    public void SetMainStockpile(Stockpile stockpile)
    {
        _mainStockpile = stockpile;
        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Main stockpile set at {stockpile.GlobalPosition}");
    }

    // === Building Placement ===

    /// <summary>
    /// Places a building at the specified position.
    /// Creates a construction site and generates haul resource tasks.
    /// </summary>
    /// <param name="buildingType">Type of building to place</param>
    /// <param name="position">World position for the building</param>
    /// <param name="parent">Parent node to add the construction site to</param>
    /// <param name="buildingScenePath">Path to the building scene to spawn when complete</param>
    /// <returns>The created construction site, or null if placement failed</returns>
    public ConstructionSite PlaceBuilding(BuildingType buildingType, Vector2 position, Node parent, string buildingScenePath)
    {
        var buildingData = BuildingDefinitions.GetBuildingData(buildingType);

        // Validate placement (basic check for now)
        if (!IsValidPlacement(position))
        {
            LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Invalid placement position: {position}");
            return null;
        }

        // Check if we can afford it
        var requiredResources = new ResourceData();
        foreach (var cost in buildingData.Cost)
        {
            requiredResources.Add(cost.Key, cost.Value);
        }

        if (!_resourceManager.HasEnoughResources(requiredResources))
        {
            LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Cannot afford {buildingData.Name}");
            return null;
        }

        // DON'T deduct resources here - builders will withdraw them during hauling
        // Resources are withdrawn when workers pick up haul tasks

        // Load construction site scene
        var siteScene = GD.Load<PackedScene>("res://Scenes/Entities/ConstructionSite.tscn");
        if (siteScene == null)
        {
            GD.PushError("[BuildingManager] Failed to load ConstructionSite scene");
            return null;
        }

        // Instantiate construction site
        var site = siteScene.Instantiate<ConstructionSite>();
        site.BuildingType = buildingType;
        site.BuildingScenePath = buildingScenePath;
        site.GlobalPosition = position;

        parent.AddChild(site);
        _constructionSites.Add(site);

        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Placed {buildingData.Name} construction site at {position}");

        // Subscribe to construction site signals
        site.ResourcesFullyDelivered += () => OnResourcesFullyDelivered(site);
        site.ConstructionCompleted += () => OnConstructionCompleted(site);

        EmitSignal(SignalName.ConstructionStarted, site);

        // Generate haul resource tasks for all required resources
        GenerateHaulTasksForSite(site);

        return site;
    }

    /// <summary>
    /// Validates if a building can be placed at the specified position.
    /// </summary>
    private bool IsValidPlacement(Vector2 position)
    {
        // TODO: Add proper validation (check terrain, overlap with other buildings, etc.)
        // For now, just do basic checks
        return true;
    }

    // === Task Generation ===

    /// <summary>
    /// Generates haul resource tasks for a construction site.
    /// </summary>
    private void GenerateHaulTasksForSite(ConstructionSite site)
    {
        if (_mainStockpile == null)
        {
            LogManager.Warning("[BuildingManager] No main stockpile set, cannot generate haul tasks");
            return;
        }

        foreach (var resource in site.RequiredResources)
        {
            var resourceType = resource.Key;
            var totalAmount = resource.Value;

            // Calculate number of haul trips needed
            int haulAmount = GameConfig.CONSTRUCTION_HAUL_AMOUNT;
            int numTrips = Mathf.CeilToInt((float)totalAmount / haulAmount);

            // Create haul tasks
            for (int i = 0; i < numTrips; i++)
            {
                int amountThisTrip = Mathf.Min(haulAmount, totalAmount - (i * haulAmount));

                // Create HaulResourceTask (will be implemented next)
                var haulTask = new HaulResourceTask(
                    resourceType,
                    amountThisTrip,
                    _mainStockpile.DepositPosition,
                    site.WorkPosition,
                    site
                );

                _taskManager.AddTask(haulTask);
            }

            LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Generated {numTrips} haul tasks for {totalAmount} {resourceType}");
        }
    }

    /// <summary>
    /// Called when all resources have been delivered to a construction site.
    /// Generates a build task for the construction work.
    /// </summary>
    private void OnResourcesFullyDelivered(ConstructionSite site)
    {
        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Resources fully delivered for {site.Data.Name}, generating build task");

        // Create BuildTask (will be implemented next)
        var buildTask = new BuildTask(
            site.WorkPosition,
            site.Data.BuildTime,
            site
        );

        _taskManager.AddTask(buildTask);
    }

    /// <summary>
    /// Called when construction is completed.
    /// </summary>
    private void OnConstructionCompleted(ConstructionSite site)
    {
        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Construction completed for {site.Data.Name}");

        _constructionSites.Remove(site);

        // The building will be spawned by the construction site itself
        // We'll track it when it emits BuildingCompleted signal
    }

    /// <summary>
    /// Registers a building with the manager.
    /// Should be called when a building is spawned.
    /// </summary>
    public void RegisterBuilding(Building building)
    {
        if (_buildings.Contains(building))
            return;

        _buildings.Add(building);

        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] Registered {building.Data.Name} at {building.GlobalPosition}");

        // Subscribe to building signals
        building.BuildingCompleted += () => OnBuildingCompleted(building);

        EmitSignal(SignalName.BuildingPlaced, building);
    }

    /// <summary>
    /// Called when a building becomes operational.
    /// </summary>
    private void OnBuildingCompleted(Building building)
    {
        LogManager.Log(LogManager.DEBUG_BUILDING_MANAGER, $"[BuildingManager] {building.Data.Name} is now operational");

        EmitSignal(SignalName.ConstructionCompleted, building);
        EmitSignal(SignalName.BuildingActivated, building);
    }

    // === Queries ===

    /// <summary>
    /// Gets all buildings of a specific type.
    /// </summary>
    public List<Building> GetBuildingsOfType(BuildingType type)
    {
        return _buildings.Where(b => b.Type == type).ToList();
    }

    /// <summary>
    /// Gets all active buildings.
    /// </summary>
    public List<Building> GetActiveBuildings()
    {
        return _buildings.Where(b => b.State == BuildingState.Active).ToList();
    }

    /// <summary>
    /// Gets all construction sites.
    /// </summary>
    public List<ConstructionSite> GetConstructionSites()
    {
        return new List<ConstructionSite>(_constructionSites);
    }

    /// <summary>
    /// Gets the total number of buildings.
    /// </summary>
    public int GetBuildingCount()
    {
        return _buildings.Count;
    }

    /// <summary>
    /// Gets the total number of active construction sites.
    /// </summary>
    public int GetConstructionSiteCount()
    {
        return _constructionSites.Count;
    }

    /// <summary>
    /// Checks if a building type can be afforded with current resources.
    /// </summary>
    public bool CanAfford(BuildingType type)
    {
        var buildingData = BuildingDefinitions.GetBuildingData(type);
        var requiredResources = new ResourceData();
        foreach (var cost in buildingData.Cost)
        {
            requiredResources.Add(cost.Key, cost.Value);
        }
        return _resourceManager.HasEnoughResources(requiredResources);
    }
}
