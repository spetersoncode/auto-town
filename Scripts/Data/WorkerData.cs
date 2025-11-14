using autotown.Core;

namespace autotown.Data;

/// <summary>
/// Defines the state of a worker state machine.
/// </summary>
public enum WorkerState
{
    /// <summary>Worker has no task and is waiting</summary>
    Idle,

    /// <summary>Worker is moving to a location</summary>
    Moving,

    /// <summary>Worker is performing work (gathering, building, etc.)</summary>
    Working,

    /// <summary>Worker is carrying resources to a destination</summary>
    Hauling
}

/// <summary>
/// Stores worker-specific data and state.
/// </summary>
public class WorkerData
{
    /// <summary>Maximum inventory capacity for all workers</summary>
    public const int MAX_INVENTORY_CAPACITY = 20;

    /// <summary>Assigned job type for this worker</summary>
    public JobType Job { get; set; }

    /// <summary>Current state of the worker</summary>
    public WorkerState State { get; set; }

    /// <summary>Resource currently being carried (if any)</summary>
    public ResourceType? CarriedResource { get; set; }

    /// <summary>Amount of resource currently being carried</summary>
    public int CarriedAmount { get; set; }

    /// <summary>Worker's movement speed (tiles per second)</summary>
    public float MovementSpeed { get; set; }

    /// <summary>Worker's work efficiency multiplier (1.0 = 100%)</summary>
    public float Efficiency { get; set; }

    /// <summary>
    /// Initializes a new worker data instance with default values.
    /// </summary>
    public WorkerData()
    {
        Job = JobType.None;
        State = WorkerState.Idle;
        CarriedResource = null;
        CarriedAmount = 0;
        MovementSpeed = GameConfig.DEFAULT_WORKER_SPEED;
        Efficiency = GameConfig.DEFAULT_WORKER_EFFICIENCY;
    }

    /// <summary>
    /// Checks if the worker is currently carrying any resources.
    /// </summary>
    /// <returns>True if carrying resources</returns>
    public bool IsCarryingResources()
    {
        return CarriedResource.HasValue && CarriedAmount > 0;
    }

    /// <summary>
    /// Checks if the worker has room for more resources in their inventory.
    /// </summary>
    /// <returns>True if inventory has available space</returns>
    public bool CanCarryMore()
    {
        return CarriedAmount < MAX_INVENTORY_CAPACITY;
    }

    /// <summary>
    /// Gets remaining inventory space.
    /// </summary>
    /// <returns>Number of items that can still be carried</returns>
    public int GetRemainingCapacity()
    {
        return MAX_INVENTORY_CAPACITY - CarriedAmount;
    }

    /// <summary>
    /// Checks if inventory is full.
    /// </summary>
    /// <returns>True if inventory is at maximum capacity</returns>
    public bool IsInventoryFull()
    {
        return CarriedAmount >= MAX_INVENTORY_CAPACITY;
    }

    /// <summary>
    /// Picks up resources for hauling.
    /// </summary>
    /// <param name="resourceType">Type of resource to carry</param>
    /// <param name="amount">Amount to carry</param>
    public void PickupResource(ResourceType resourceType, int amount)
    {
        CarriedResource = resourceType;
        CarriedAmount = amount;
    }

    /// <summary>
    /// Adds resources to existing inventory (for production buildings).
    /// </summary>
    /// <param name="resourceType">Type of resource to add</param>
    /// <param name="amount">Amount to add</param>
    /// <returns>Actual amount added (respecting capacity)</returns>
    public int AddToInventory(ResourceType resourceType, int amount)
    {
        // If not carrying anything yet, start carrying this resource type
        if (!CarriedResource.HasValue)
        {
            CarriedResource = resourceType;
            CarriedAmount = 0;
        }

        // Can only add if carrying the same resource type
        if (CarriedResource != resourceType)
            return 0;

        int remainingSpace = GetRemainingCapacity();
        int amountToAdd = Godot.Mathf.Min(amount, remainingSpace);
        CarriedAmount += amountToAdd;

        return amountToAdd;
    }

    /// <summary>
    /// Drops carried resources.
    /// </summary>
    public void DropResource()
    {
        CarriedResource = null;
        CarriedAmount = 0;
    }
}
