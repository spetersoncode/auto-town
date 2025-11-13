using System.Collections.Generic;

namespace autotown.Data;

/// <summary>
/// Stores resource quantities and provides methods for resource management.
/// </summary>
public class ResourceData
{
    private readonly Dictionary<ResourceType, int> _resources;

    /// <summary>
    /// Initializes a new instance with zero resources.
    /// </summary>
    public ResourceData()
    {
        _resources = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 0 },
            { ResourceType.Stone, 0 },
            { ResourceType.Food, 0 }
        };
    }

    /// <summary>
    /// Gets the quantity of a specific resource type.
    /// </summary>
    /// <param name="type">The resource type to query</param>
    /// <returns>The quantity of the resource</returns>
    public int GetAmount(ResourceType type)
    {
        return _resources.TryGetValue(type, out int amount) ? amount : 0;
    }

    /// <summary>
    /// Adds resources to the storage.
    /// </summary>
    /// <param name="type">The resource type to add</param>
    /// <param name="amount">The amount to add (must be positive)</param>
    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0) return;

        if (!_resources.ContainsKey(type))
            _resources[type] = 0;

        _resources[type] += amount;
    }

    /// <summary>
    /// Removes resources from storage.
    /// </summary>
    /// <param name="type">The resource type to remove</param>
    /// <param name="amount">The amount to remove (must be positive)</param>
    /// <returns>True if resources were removed, false if insufficient resources</returns>
    public bool Remove(ResourceType type, int amount)
    {
        if (amount <= 0) return false;
        if (!HasEnough(type, amount)) return false;

        _resources[type] -= amount;
        return true;
    }

    /// <summary>
    /// Checks if there are enough resources of a specific type.
    /// </summary>
    /// <param name="type">The resource type to check</param>
    /// <param name="amount">The required amount</param>
    /// <returns>True if enough resources are available</returns>
    public bool HasEnough(ResourceType type, int amount)
    {
        return GetAmount(type) >= amount;
    }

    /// <summary>
    /// Sets the exact amount of a resource (for initialization/cheats).
    /// </summary>
    /// <param name="type">The resource type</param>
    /// <param name="amount">The amount to set</param>
    public void SetAmount(ResourceType type, int amount)
    {
        _resources[type] = amount;
    }

    /// <summary>
    /// Gets a read-only view of all resources.
    /// </summary>
    /// <returns>Dictionary of all resource types and amounts</returns>
    public IReadOnlyDictionary<ResourceType, int> GetAll()
    {
        return _resources;
    }
}
