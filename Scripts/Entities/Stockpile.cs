using Godot;
using autotown.Core;
using autotown.Data;
using autotown.Systems;

namespace autotown.Entities;

/// <summary>
/// Stockpile building for storing resources.
/// Automatically integrates with ResourceManager to update global resource counts.
/// </summary>
public partial class Stockpile : Node2D
{
    // === Signals ===

    /// <summary>
    /// Emitted when a resource is deposited.
    /// Parameters: ResourceType type, int amount
    /// </summary>
    [Signal]
    public delegate void ResourceDepositedEventHandler(ResourceType type, int amount);

    /// <summary>
    /// Emitted when a resource is withdrawn.
    /// Parameters: ResourceType type, int amount
    /// </summary>
    [Signal]
    public delegate void ResourceWithdrawnEventHandler(ResourceType type, int amount);

    // === Exported Properties ===

    /// <summary>
    /// Maximum capacity per resource type (0 = unlimited).
    /// </summary>
    [Export]
    public int CapacityPerResource { get; set; } = GameConfig.STOCKPILE_CAPACITY_PER_RESOURCE;

    // === Private Fields ===

    /// <summary>
    /// Local resource storage (optional, for future use if we want local stockpiles).
    /// For now, we'll directly update the global ResourceManager.
    /// </summary>
    private ResourceData _localStorage;

    /// <summary>
    /// Position where workers deposit resources (centered on the stockpile).
    /// </summary>
    public Vector2 DepositPosition => GlobalPosition;

    // === Lifecycle ===

    public override void _Ready()
    {
        // Initialize local storage (for future use)
        _localStorage = new ResourceData();

        GD.Print($"[Stockpile] Initialized at {GlobalPosition}. Capacity per resource: " +
                 $"{(CapacityPerResource == 0 ? "Unlimited" : CapacityPerResource.ToString())}");
    }

    // === Public Methods ===

    /// <summary>
    /// Deposits resources into the stockpile.
    /// Automatically updates the global ResourceManager.
    /// </summary>
    /// <param name="type">Type of resource to deposit</param>
    /// <param name="amount">Amount to deposit</param>
    /// <returns>True if successful, false if capacity exceeded</returns>
    public bool DepositResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            GD.PushWarning($"[Stockpile] Attempted to deposit non-positive amount: {amount}");
            return false;
        }

        // Check capacity (if limited)
        if (CapacityPerResource > 0)
        {
            var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
            int currentAmount = resourceManager.GetResourceAmount(type);

            if (currentAmount + amount > CapacityPerResource)
            {
                GD.PushWarning($"[Stockpile] Capacity exceeded for {type}. " +
                              $"Current: {currentAmount}, Deposit: {amount}, Max: {CapacityPerResource}");
                return false;
            }
        }

        // Add to global ResourceManager
        var resourceManager2 = GetNode<ResourceManager>("/root/ResourceManager");
        resourceManager2.AddResource(type, amount);

        // Update local storage (for future use)
        _localStorage.Add(type, amount);

        GD.Print($"[Stockpile] Deposited {amount} {type}");

        // Emit signal
        EmitSignal(SignalName.ResourceDeposited, (int)type, amount);

        return true;
    }

    /// <summary>
    /// Withdraws resources from the stockpile.
    /// Automatically updates the global ResourceManager.
    /// </summary>
    /// <param name="type">Type of resource to withdraw</param>
    /// <param name="amount">Amount to withdraw</param>
    /// <returns>True if successful, false if insufficient resources</returns>
    public bool WithdrawResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            GD.PushWarning($"[Stockpile] Attempted to withdraw non-positive amount: {amount}");
            return false;
        }

        // Remove from global ResourceManager
        var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        bool success = resourceManager.RemoveResource(type, amount);

        if (success)
        {
            // Update local storage (for future use)
            _localStorage.Remove(type, amount);

            GD.Print($"[Stockpile] Withdrew {amount} {type}");

            // Emit signal
            EmitSignal(SignalName.ResourceWithdrawn, (int)type, amount);
        }

        return success;
    }

    /// <summary>
    /// Gets the stored amount of a specific resource (from global ResourceManager).
    /// </summary>
    public int GetStoredAmount(ResourceType type)
    {
        var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        return resourceManager.GetResourceAmount(type);
    }

    /// <summary>
    /// Checks if the stockpile is full for a specific resource type.
    /// </summary>
    public bool IsFull(ResourceType type)
    {
        if (CapacityPerResource == 0)
        {
            return false; // Unlimited capacity
        }

        var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        return resourceManager.GetResourceAmount(type) >= CapacityPerResource;
    }

    /// <summary>
    /// Checks if the stockpile can accept a deposit of the specified amount.
    /// </summary>
    public bool CanAcceptDeposit(ResourceType type, int amount)
    {
        if (CapacityPerResource == 0)
        {
            return true; // Unlimited capacity
        }

        var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        int currentAmount = resourceManager.GetResourceAmount(type);
        return currentAmount + amount <= CapacityPerResource;
    }

    /// <summary>
    /// Gets the remaining capacity for a specific resource type.
    /// </summary>
    public int GetRemainingCapacity(ResourceType type)
    {
        if (CapacityPerResource == 0)
        {
            return int.MaxValue; // Unlimited
        }

        var resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        int currentAmount = resourceManager.GetResourceAmount(type);
        return Mathf.Max(0, CapacityPerResource - currentAmount);
    }
}
