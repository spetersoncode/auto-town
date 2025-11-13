using Godot;
using autotown.Core;
using autotown.Entities;
using autotown.Systems;

namespace autotown.Data;

/// <summary>
/// Task for processing resources at a building (Sawmill, Mine, Farm).
/// Generated periodically by active processing buildings.
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
    /// Amount of resource to produce.
    /// </summary>
    public int OutputAmount { get; private set; }

    /// <summary>
    /// Time required to complete processing (in seconds).
    /// </summary>
    private float _processTime;

    /// <summary>
    /// Progress of processing (0.0 to 1.0).
    /// </summary>
    public float Progress { get; private set; } = 0f;

    /// <summary>
    /// Time elapsed working on processing.
    /// </summary>
    private float _elapsedTime = 0f;

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

    public override void OnStart()
    {
        if (!IsValid())
        {
            GD.Print($"[ProcessTask] Cannot start - building is invalid or not active");
            Cancel();
            return;
        }

        GD.Print($"[ProcessTask] Started processing at {TargetBuilding.Data.Name} to produce {OutputAmount} {OutputResourceType}");
    }

    public override void OnUpdate(double delta)
    {
        if (!IsValid())
        {
            Cancel();
            return;
        }

        // Update processing progress
        _elapsedTime += (float)delta;
        Progress = Mathf.Clamp(_elapsedTime / _processTime, 0f, 1f);

        // Log progress at 25% intervals
        int progressPercent = Mathf.FloorToInt(Progress * 100);
        if (progressPercent > 0 && progressPercent % 25 == 0)
        {
            // Only log once per 25% milestone
            int lastPercent = Mathf.FloorToInt((_elapsedTime - (float)delta) / _processTime * 100);
            if (lastPercent < progressPercent && progressPercent % 25 == 0)
            {
                GD.Print($"[ProcessTask] Processing progress: {progressPercent}%");
            }
        }

        // Check if processing is complete
        if (Progress >= 1.0f)
        {
            Complete();
        }
    }

    public override void OnComplete()
    {
        if (!IsValid())
        {
            GD.PushWarning("[ProcessTask] Building became invalid before completion");
            return;
        }

        GD.Print($"[ProcessTask] Completed processing at {TargetBuilding.Data.Name}, produced {OutputAmount} {OutputResourceType}");

        // Add the produced resource to the global resource manager
        var resourceManager = TargetBuilding.GetNode<ResourceManager>("/root/ResourceManager");
        if (resourceManager != null)
        {
            resourceManager.AddResource(OutputResourceType, OutputAmount);
            GD.Print($"[ProcessTask] Added {OutputAmount} {OutputResourceType} to global stockpile");
        }
        else
        {
            GD.PushError("[ProcessTask] Could not find ResourceManager");
        }
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
