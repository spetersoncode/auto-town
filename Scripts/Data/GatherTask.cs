using Godot;
using autotown.Entities;

namespace autotown.Data;

/// <summary>
/// Task for gathering resources from harvestable resource nodes.
/// </summary>
public partial class GatherTask : Task
{
    /// <summary>
    /// The resource node to harvest from.
    /// </summary>
    public HarvestableResource ResourceNode { get; private set; }

    /// <summary>
    /// Type of resource being gathered.
    /// </summary>
    public ResourceType ResourceType { get; private set; }

    /// <summary>
    /// Expected yield from this harvest.
    /// </summary>
    public int ExpectedYield { get; private set; }

    /// <summary>
    /// Position of the stockpile where resources should be deposited.
    /// </summary>
    public Vector2 StockpilePosition { get; set; }

    public override JobType[] ValidJobTypes
    {
        get
        {
            // Map resource types to job types
            return ResourceType switch
            {
                ResourceType.Wood => new[] { JobType.Lumberjack },
                ResourceType.Stone => new[] { JobType.Miner },
                ResourceType.Food => new[] { JobType.Forager },
                _ => new[] { JobType.None }
            };
        }
    }

    public override float EstimatedDuration => ResourceNode?.HarvestDuration ?? 0f;

    /// <summary>
    /// Creates a new gather task for the specified resource node.
    /// </summary>
    public GatherTask(HarvestableResource resourceNode, Vector2 stockpilePosition)
    {
        if (resourceNode == null || !IsInstanceValid(resourceNode))
        {
            GD.PrintErr("GatherTask: Cannot create task with null or invalid resource node");
            return;
        }

        Type = TaskType.Gather;
        ResourceNode = resourceNode;
        ResourceType = resourceNode.ResourceType;
        ExpectedYield = resourceNode.YieldPerHarvest;
        Position = resourceNode.GlobalPosition;
        StockpilePosition = stockpilePosition;

        // Higher priority for closer resources (can be adjusted later)
        Priority = 0;
    }

    /// <summary>
    /// Checks if this task is still valid (resource node exists and is not depleted).
    /// Note: We don't check CanBeHarvested() here because the resource might already be reserved.
    /// </summary>
    public bool IsValid()
    {
        return ResourceNode != null
            && IsInstanceValid(ResourceNode)
            && ResourceNode.State != HarvestableResource.HarvestState.Depleted;
    }

    public override void OnStart()
    {
        if (!IsValid())
        {
            GD.PrintErr($"GatherTask: Cannot start - resource node is invalid or not harvestable");
            Cancel();
            return;
        }

        GD.Print($"GatherTask: Started harvesting {ResourceType} at {Position}");
    }

    public override void OnUpdate(double delta)
    {
        // The HarvestableResource handles harvest progress internally
        // Workers just need to wait for completion
    }

    public override void OnComplete()
    {
        GD.Print($"GatherTask: Completed harvesting {ExpectedYield} {ResourceType}");
    }

    /// <summary>
    /// Cancels the task and releases any resource reservations.
    /// </summary>
    public override void Cancel()
    {
        if (ResourceNode != null && IsInstanceValid(ResourceNode) && AssignedWorker != null && AssignedWorker is Node workerNode)
        {
            ResourceNode.ReleaseReservation(workerNode);
        }

        base.Cancel();
    }

    /// <summary>
    /// Attempts to reserve the resource node for the assigned worker.
    /// </summary>
    public bool TryReserveResource(GodotObject worker)
    {
        if (!IsValid())
            return false;

        if (worker is Node workerNode)
        {
            return ResourceNode.ReserveForHarvest(workerNode);
        }
        return false;
    }

    /// <summary>
    /// Releases the resource reservation.
    /// </summary>
    public void ReleaseResourceReservation(GodotObject worker)
    {
        if (ResourceNode != null && IsInstanceValid(ResourceNode) && worker is Node workerNode)
        {
            ResourceNode.ReleaseReservation(workerNode);
        }
    }
}
