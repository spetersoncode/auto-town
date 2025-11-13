using Godot;
using autotown.Core;
using autotown.Data;

namespace autotown.Systems;

/// <summary>
/// Global resource manager singleton.
/// Tracks all resources in the game and provides centralized access.
/// Communicates changes via signals for decoupled architecture.
/// </summary>
public partial class ResourceManager : Node
{
    // === Signals ===

    /// <summary>
    /// Emitted when a resource amount changes.
    /// Parameters: ResourceType type, int newAmount
    /// </summary>
    [Signal]
    public delegate void ResourceChangedEventHandler(ResourceType type, int newAmount);

    /// <summary>
    /// Emitted when any resource is added to the stockpile.
    /// Parameters: ResourceType type, int amountAdded
    /// </summary>
    [Signal]
    public delegate void ResourceAddedEventHandler(ResourceType type, int amountAdded);

    /// <summary>
    /// Emitted when any resource is removed from the stockpile.
    /// Parameters: ResourceType type, int amountRemoved
    /// </summary>
    [Signal]
    public delegate void ResourceRemovedEventHandler(ResourceType type, int amountRemoved);

    /// <summary>
    /// Emitted when resources are updated (batch signal for UI refresh).
    /// </summary>
    [Signal]
    public delegate void ResourcesUpdatedEventHandler();

    // === Properties ===

    /// <summary>
    /// Global resource storage using ResourceData.
    /// </summary>
    private ResourceData _resources;

    /// <summary>
    /// Gets the current amount of a specific resource.
    /// </summary>
    public int GetResourceAmount(ResourceType type)
    {
        return _resources.GetAmount(type);
    }

    /// <summary>
    /// Gets all resources as a read-only dictionary.
    /// </summary>
    public System.Collections.Generic.IReadOnlyDictionary<ResourceType, int> GetAllResources()
    {
        return _resources.GetAll();
    }

    /// <summary>
    /// Checks if we have enough of a specific resource.
    /// </summary>
    public bool HasEnough(ResourceType type, int amount)
    {
        return _resources.HasEnough(type, amount);
    }

    /// <summary>
    /// Checks if we have enough resources to meet a requirement (ResourceData).
    /// </summary>
    public bool HasEnoughResources(ResourceData required)
    {
        foreach (var resourceType in System.Enum.GetValues<ResourceType>())
        {
            if (!HasEnough(resourceType, required.GetAmount(resourceType)))
            {
                return false;
            }
        }
        return true;
    }

    // === Lifecycle ===

    public override void _Ready()
    {
        // Initialize resource storage
        _resources = new ResourceData();

        // Set starting resources from GameConfig
        _resources.SetAmount(ResourceType.Wood, GameConfig.STARTING_WOOD);
        _resources.SetAmount(ResourceType.Stone, GameConfig.STARTING_STONE);
        _resources.SetAmount(ResourceType.Food, GameConfig.STARTING_FOOD);

        GD.Print($"[ResourceManager] Initialized with starting resources: " +
                 $"Wood={GameConfig.STARTING_WOOD}, " +
                 $"Stone={GameConfig.STARTING_STONE}, " +
                 $"Food={GameConfig.STARTING_FOOD}");

        EmitSignal(SignalName.ResourcesUpdated);
    }

    // === Public Methods ===

    /// <summary>
    /// Adds resources to the global stockpile.
    /// </summary>
    /// <param name="type">The type of resource to add</param>
    /// <param name="amount">The amount to add (must be positive)</param>
    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            GD.PushWarning($"[ResourceManager] Attempted to add non-positive amount: {amount}");
            return;
        }

        _resources.Add(type, amount);
        int newAmount = _resources.GetAmount(type);

        GD.Print($"[ResourceManager] Added {amount} {type}. New total: {newAmount}");

        EmitSignal(SignalName.ResourceAdded, (int)type, amount);
        EmitSignal(SignalName.ResourceChanged, (int)type, newAmount);
        EmitSignal(SignalName.ResourcesUpdated);
    }

    /// <summary>
    /// Removes resources from the global stockpile.
    /// </summary>
    /// <param name="type">The type of resource to remove</param>
    /// <param name="amount">The amount to remove</param>
    /// <returns>True if successful, false if insufficient resources</returns>
    public bool RemoveResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            GD.PushWarning($"[ResourceManager] Attempted to remove non-positive amount: {amount}");
            return false;
        }

        if (!_resources.HasEnough(type, amount))
        {
            GD.PushWarning($"[ResourceManager] Insufficient {type}. Requested: {amount}, Available: {_resources.GetAmount(type)}");
            return false;
        }

        _resources.Remove(type, amount);
        int newAmount = _resources.GetAmount(type);

        GD.Print($"[ResourceManager] Removed {amount} {type}. New total: {newAmount}");

        EmitSignal(SignalName.ResourceRemoved, (int)type, amount);
        EmitSignal(SignalName.ResourceChanged, (int)type, newAmount);
        EmitSignal(SignalName.ResourcesUpdated);

        return true;
    }

    /// <summary>
    /// Removes multiple resources at once (e.g., for building costs).
    /// </summary>
    /// <param name="costs">ResourceData containing the amounts to remove</param>
    /// <returns>True if successful, false if any resource is insufficient</returns>
    public bool RemoveResources(ResourceData costs)
    {
        // First, check if we have enough of all resources
        if (!HasEnoughResources(costs))
        {
            GD.PushWarning("[ResourceManager] Insufficient resources for batch removal");
            return false;
        }

        // Remove each resource
        foreach (var resourceType in System.Enum.GetValues<ResourceType>())
        {
            int amount = costs.GetAmount(resourceType);
            if (amount > 0)
            {
                RemoveResource(resourceType, amount);
            }
        }

        return true;
    }

    /// <summary>
    /// Sets a resource to a specific amount (for debugging/testing).
    /// </summary>
    public void SetResourceAmount(ResourceType type, int amount)
    {
        _resources.SetAmount(type, amount);
        int newAmount = _resources.GetAmount(type);

        GD.Print($"[ResourceManager] Set {type} to {newAmount}");

        EmitSignal(SignalName.ResourceChanged, (int)type, newAmount);
        EmitSignal(SignalName.ResourcesUpdated);
    }

    /// <summary>
    /// Resets all resources to starting values.
    /// </summary>
    public void ResetResources()
    {
        _resources.SetAmount(ResourceType.Wood, GameConfig.STARTING_WOOD);
        _resources.SetAmount(ResourceType.Stone, GameConfig.STARTING_STONE);
        _resources.SetAmount(ResourceType.Food, GameConfig.STARTING_FOOD);

        GD.Print("[ResourceManager] Resources reset to starting values");

        EmitSignal(SignalName.ResourcesUpdated);
    }
}
