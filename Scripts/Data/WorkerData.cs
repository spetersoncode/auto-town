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
    /// Drops carried resources.
    /// </summary>
    public void DropResource()
    {
        CarriedResource = null;
        CarriedAmount = 0;
    }
}
