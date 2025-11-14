using Godot;
using autotown.Core;
using autotown.Entities;
using autotown.Systems;

namespace autotown.Data;

/// <summary>
/// Task for processing resources at a building (Sawmill, Mine, Farm).
/// Generated periodically by active processing buildings.
/// Worker continues production cycles until their inventory is full.
/// </summary>
public partial class ProcessTask : Task
{
    /// <summary>
    /// The building where processing occurs.
    /// </summary>
    public Building TargetBuilding { get; private set; }

    /// <summary>
    /// Type of resource to produce.
    /// </summary>
    public ResourceType OutputResourceType { get; private set; }

    /// <summary>
    /// Amount of resource to produce per cycle.
    /// </summary>
    public int OutputAmount { get; private set; }

    /// <summary>
    /// Time required to complete one production cycle (in seconds).
    /// </summary>
    private float _processTime;

    /// <summary>
    /// Progress of current production cycle (0.0 to 1.0).
    /// </summary>
    public float Progress { get; private set; } = 0f;

    /// <summary>
    /// Time elapsed working on current cycle.
    /// </summary>
    private float _elapsedTime = 0f;

    /// <summary>
    /// Reference to the worker performing this task.
    /// </summary>
    private WorkerData _assignedWorker = null;

    /// <summary>
    /// Valid job types depend on the building type.
    /// </summary>
    public override JobType[] ValidJobTypes { get; }

    /// <summary>
    /// Estimated duration is the processing time.
    /// </summary>
    public override float EstimatedDuration => _processTime;

    /// <summary>
    /// Creates a new process task for a building.
    /// </summary>
    /// <param name="building">The building performing the processing</param>
    /// <param name="outputResourceType">Type of resource to produce</param>
    /// <param name="outputAmount">Amount of resource to produce</param>
    /// <param name="processTime">Time required to complete processing</param>
    /// <param name="validJobs">Which job types can perform this task</param>
    public ProcessTask(Building building, ResourceType outputResourceType, int outputAmount, float processTime, JobType[] validJobs)
    {
        Type = TaskType.Process;
        TargetBuilding = building;
        OutputResourceType = outputResourceType;
        OutputAmount = outputAmount;
        _processTime = processTime;
        ValidJobTypes = validJobs;

        Position = building.GetInteractionPosition();

        // Processing tasks have lower priority than building
        Priority = 0;
    }

    /// <summary>
    /// Checks if this task is still valid (building exists and is active).
    /// </summary>
    public bool IsValid()
    {
        return TargetBuilding != null
            && IsInstanceValid(TargetBuilding)
            && TargetBuilding.State == BuildingState.Active;
    }

    /// <summary>
    /// Sets the worker assigned to this task.
    /// </summary>
    public void SetAssignedWorker(WorkerData worker)
    {
        _assignedWorker = worker;
    }

    public override void OnStart()
    {
        if (!IsValid())
        {
            GD.Print($"[ProcessTask] Cannot start - building is invalid or not active");
            Cancel();
            return;
        }

        GD.Print($"[ProcessTask] Started processing at {TargetBuilding.Data.Name} to produce {OutputResourceType}");
    }

    public override void OnUpdate(double delta)
    {
        if (!IsValid())
        {
            Cancel();
            return;
        }

        if (_assignedWorker == null)
        {
            GD.PushWarning("[ProcessTask] No worker assigned to task");
            return;
        }

        // Update processing progress
        _elapsedTime += (float)delta;
        Progress = Mathf.Clamp(_elapsedTime / _processTime, 0f, 1f);

        // Check if one production cycle is complete
        if (Progress >= 1.0f)
        {
            // Add resources to worker's inventory
            int amountAdded = _assignedWorker.AddToInventory(OutputResourceType, OutputAmount);

            if (amountAdded > 0)
            {
                GD.Print($"[ProcessTask] Produced {amountAdded} {OutputResourceType}, worker inventory: {_assignedWorker.CarriedAmount}/{WorkerData.MAX_INVENTORY_CAPACITY}");
            }

            // Check if worker's inventory is full
            if (_assignedWorker.IsInventoryFull())
            {
                GD.Print($"[ProcessTask] Worker inventory full ({_assignedWorker.CarriedAmount}/{WorkerData.MAX_INVENTORY_CAPACITY}), completing task");
                Complete();
                return;
            }

            // If worker can carry more, reset for another production cycle
            if (_assignedWorker.CanCarryMore())
            {
                _elapsedTime = 0f;
                Progress = 0f;
                GD.Print($"[ProcessTask] Starting new production cycle");
            }
            else
            {
                // Inventory full, complete the task
                Complete();
            }
        }
    }

    public override void OnComplete()
    {
        if (!IsValid())
        {
            GD.PushWarning("[ProcessTask] Building became invalid before completion");
            return;
        }

        if (_assignedWorker != null)
        {
            GD.Print($"[ProcessTask] Completed processing at {TargetBuilding.Data.Name}, worker carrying {_assignedWorker.CarriedAmount} {OutputResourceType}");
        }
        else
        {
            GD.Print($"[ProcessTask] Completed processing at {TargetBuilding.Data.Name}");
        }

        // Worker will now haul the resources to stockpile
        // Resources are in worker's inventory, not added directly to global stockpile
    }

    /// <summary>
    /// Cancels the processing task.
    /// </summary>
    public override void Cancel()
    {
        GD.Print($"[ProcessTask] Processing cancelled at {TargetBuilding?.Data.Name ?? "unknown building"}");
        base.Cancel();
    }

    /// <summary>
    /// Gets the remaining processing time in seconds.
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, _processTime - _elapsedTime);
    }
}
