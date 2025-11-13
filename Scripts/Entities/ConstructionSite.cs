using Godot;
using autotown.Data;
using autotown.Core;
using System.Collections.Generic;
using System.Linq;

namespace autotown.Entities;

/// <summary>
/// Represents a building under construction.
/// Tracks resource delivery progress and converts to actual building when complete.
/// </summary>
public partial class ConstructionSite : Node2D
{
    // === Signals ===

    /// <summary>
    /// Emitted when resources are delivered to the construction site.
    /// Parameters: ResourceType type, int amount
    /// </summary>
    [Signal]
    public delegate void ResourceDeliveredEventHandler(ResourceType type, int amount);

    /// <summary>
    /// Emitted when all resources have been delivered and construction can begin.
    /// </summary>
    [Signal]
    public delegate void ResourcesFullyDeliveredEventHandler();

    /// <summary>
    /// Emitted when construction work is completed.
    /// </summary>
    [Signal]
    public delegate void ConstructionCompletedEventHandler();

    // === Properties ===

    /// <summary>
    /// The type of building being constructed.
    /// </summary>
    public BuildingType BuildingType { get; set; }

    /// <summary>
    /// Building data containing required resources and build time.
    /// </summary>
    public BuildingData Data { get; private set; }

    /// <summary>
    /// Resources required for construction.
    /// </summary>
    public Dictionary<ResourceType, int> RequiredResources { get; private set; }

    /// <summary>
    /// Resources delivered so far.
    /// </summary>
    public Dictionary<ResourceType, int> DeliveredResources { get; private set; }

    /// <summary>
    /// Scene path for the building to spawn when construction is complete.
    /// </summary>
    public string BuildingScenePath { get; set; }

    /// <summary>
    /// Position where workers deliver resources and perform construction work.
    /// </summary>
    public Vector2 WorkPosition => GlobalPosition;

    /// <summary>
    /// Whether all required resources have been delivered.
    /// </summary>
    public bool AreResourcesFullyDelivered { get; private set; } = false;

    /// <summary>
    /// Whether construction work has started.
    /// </summary>
    public bool IsConstructionInProgress { get; set; } = false;

    /// <summary>
    /// Whether construction has been completed.
    /// </summary>
    public bool IsConstructionComplete { get; private set; } = false;

    // === Lifecycle ===

    public override void _Ready()
    {
        // Load building data
        Data = BuildingDefinitions.GetBuildingData(BuildingType);

        // Initialize resource tracking
        RequiredResources = new Dictionary<ResourceType, int>(Data.Cost);
        DeliveredResources = new Dictionary<ResourceType, int>();

        // Initialize delivered resources to zero
        foreach (var resource in RequiredResources.Keys)
        {
            DeliveredResources[resource] = 0;
        }

        LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] {Data.Name} site created at {GlobalPosition}");
        PrintResourceStatus();
    }

    // === Resource Delivery ===

    /// <summary>
    /// Delivers resources to the construction site.
    /// </summary>
    /// <param name="type">Type of resource to deliver</param>
    /// <param name="amount">Amount to deliver</param>
    /// <returns>True if successful</returns>
    public bool DeliverResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            LogManager.Warning($"[ConstructionSite] Attempted to deliver non-positive amount: {amount}");
            return false;
        }

        if (!RequiredResources.ContainsKey(type))
        {
            LogManager.Warning($"[ConstructionSite] Resource {type} not required for this building");
            return false;
        }

        // Calculate how much can actually be delivered (don't exceed requirement)
        int currentAmount = DeliveredResources[type];
        int requiredAmount = RequiredResources[type];
        int remainingNeeded = requiredAmount - currentAmount;
        int actualDelivery = Mathf.Min(amount, remainingNeeded);

        if (actualDelivery <= 0)
        {
            LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] {type} already fully delivered ({currentAmount}/{requiredAmount})");
            return false;
        }

        // Add to delivered resources
        DeliveredResources[type] += actualDelivery;

        LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] Delivered {actualDelivery} {type} to {Data.Name} site " +
                 $"({DeliveredResources[type]}/{requiredAmount})");

        EmitSignal(SignalName.ResourceDelivered, (int)type, actualDelivery);

        // Check if all resources are now delivered
        CheckResourcesFullyDelivered();

        return true;
    }

    /// <summary>
    /// Gets the remaining amount needed for a specific resource type.
    /// </summary>
    public int GetRemainingAmount(ResourceType type)
    {
        if (!RequiredResources.ContainsKey(type))
            return 0;

        int delivered = DeliveredResources.ContainsKey(type) ? DeliveredResources[type] : 0;
        return Mathf.Max(0, RequiredResources[type] - delivered);
    }

    /// <summary>
    /// Checks if a specific resource still needs to be delivered.
    /// </summary>
    public bool NeedsResource(ResourceType type)
    {
        return GetRemainingAmount(type) > 0;
    }

    /// <summary>
    /// Gets a list of resource types that still need to be delivered.
    /// </summary>
    public List<ResourceType> GetNeededResourceTypes()
    {
        var needed = new List<ResourceType>();
        foreach (var resource in RequiredResources)
        {
            if (NeedsResource(resource.Key))
            {
                needed.Add(resource.Key);
            }
        }
        return needed;
    }

    /// <summary>
    /// Checks if all required resources have been delivered.
    /// </summary>
    private void CheckResourcesFullyDelivered()
    {
        if (AreResourcesFullyDelivered)
            return;

        bool allDelivered = RequiredResources.All(resource =>
            DeliveredResources[resource.Key] >= resource.Value
        );

        if (allDelivered)
        {
            AreResourcesFullyDelivered = true;
            LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] All resources delivered for {Data.Name}! Ready for construction.");
            EmitSignal(SignalName.ResourcesFullyDelivered);
        }
    }

    // === Construction ===

    /// <summary>
    /// Completes the construction and spawns the actual building.
    /// </summary>
    public void CompleteConstruction()
    {
        if (IsConstructionComplete)
        {
            LogManager.Warning($"[ConstructionSite] Construction already complete");
            return;
        }

        IsConstructionComplete = true;

        LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] Construction of {Data.Name} completed at {GlobalPosition}");

        EmitSignal(SignalName.ConstructionCompleted);

        // Spawn the actual building if scene path is provided
        if (!string.IsNullOrEmpty(BuildingScenePath))
        {
            SpawnBuilding();
        }
        else
        {
            LogManager.Warning($"[ConstructionSite] No building scene path provided for {Data.Name}");
        }

        // Remove construction site
        QueueFree();
    }

    /// <summary>
    /// Spawns the actual building at this location.
    /// </summary>
    private void SpawnBuilding()
    {
        var buildingScene = GD.Load<PackedScene>(BuildingScenePath);
        if (buildingScene == null)
        {
            LogManager.Error($"[ConstructionSite] Failed to load building scene: {BuildingScenePath}");
            return;
        }

        var building = buildingScene.Instantiate<Building>();
        building.GlobalPosition = GlobalPosition;

        // Add to the same parent as the construction site
        var parent = GetParent();
        parent.CallDeferred(Node.MethodName.AddChild, building);

        // Activate the building
        building.CallDeferred(nameof(Building.OnConstructionComplete));

        LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] Spawned {Data.Name} building at {GlobalPosition}");
    }

    // === Helper Methods ===

    /// <summary>
    /// Prints the current resource delivery status (for debugging).
    /// </summary>
    private void PrintResourceStatus()
    {
        LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"[ConstructionSite] Resource requirements for {Data.Name}:");
        foreach (var resource in RequiredResources)
        {
            int delivered = DeliveredResources.ContainsKey(resource.Key) ? DeliveredResources[resource.Key] : 0;
            LogManager.Log(LogManager.DEBUG_CONSTRUCTION_SITE, $"  {resource.Key}: {delivered}/{resource.Value}");
        }
    }

    /// <summary>
    /// Gets construction progress as a percentage (0-1).
    /// </summary>
    public float GetResourceProgress()
    {
        if (RequiredResources.Count == 0)
            return 1.0f;

        int totalRequired = RequiredResources.Values.Sum();
        int totalDelivered = DeliveredResources.Values.Sum();

        return totalRequired > 0 ? (float)totalDelivered / totalRequired : 0f;
    }
}
