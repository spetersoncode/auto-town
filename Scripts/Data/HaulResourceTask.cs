using Godot;
using autotown.Core;
using autotown.Entities;

namespace autotown.Data;

/// <summary>
/// Task for hauling resources from one location to another.
/// Primarily used for delivering resources from stockpile to construction sites.
/// </summary>
public partial class HaulResourceTask : Task
{
    /// <summary>
    /// Type of resource to haul.
    /// </summary>
    public ResourceType ResourceType { get; private set; }

    /// <summary>
    /// Amount of resource to haul.
    /// </summary>
    public int Amount { get; private set; }

    /// <summary>
    /// Source position (where to pick up the resource).
    /// </summary>
    public Vector2 SourcePosition { get; private set; }

    /// <summary>
    /// Destination position (where to deliver the resource).
    /// </summary>
    public Vector2 DestinationPosition { get; private set; }

    /// <summary>
    /// The construction site to deliver to (if applicable).
    /// </summary>
    public ConstructionSite TargetSite { get; private set; }

    /// <summary>
    /// Builders can perform haul tasks.
    /// </summary>
    public override JobType[] ValidJobTypes => new[] { JobType.Builder };

    /// <summary>
    /// Estimated duration is based on distance.
    /// This is a rough estimate; actual time depends on worker speed.
    /// </summary>
    public override float EstimatedDuration
    {
        get
        {
            float distance = SourcePosition.DistanceTo(DestinationPosition);
            float estimatedTime = distance / GameConfig.DEFAULT_WORKER_SPEED;
            return estimatedTime;
        }
    }

    /// <summary>
    /// Creates a new haul resource task.
    /// </summary>
    /// <param name="resourceType">Type of resource to haul</param>
    /// <param name="amount">Amount to haul</param>
    /// <param name="sourcePosition">Where to pick up (usually stockpile)</param>
    /// <param name="destinationPosition">Where to deliver (usually construction site)</param>
    /// <param name="targetSite">The construction site receiving the resource</param>
    public HaulResourceTask(ResourceType resourceType, int amount, Vector2 sourcePosition, Vector2 destinationPosition, ConstructionSite targetSite)
    {
        Type = TaskType.Haul;
        ResourceType = resourceType;
        Amount = amount;
        SourcePosition = sourcePosition;
        DestinationPosition = destinationPosition;
        TargetSite = targetSite;

        // Position is set to source for pathfinding
        Position = sourcePosition;

        // Priority based on construction site needs
        Priority = 1; // Higher priority than gathering
    }

    /// <summary>
    /// Checks if this task is still valid (construction site exists and needs the resource).
    /// </summary>
    public bool IsValid()
    {
        if (TargetSite == null || !IsInstanceValid(TargetSite))
            return false;

        // Check if the construction site still needs this resource
        return TargetSite.NeedsResource(ResourceType);
    }

    public override void OnStart()
    {
        if (!IsValid())
        {
            LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[HaulResourceTask] Cannot start - construction site is invalid or doesn't need {ResourceType}");
            Cancel();
            return;
        }

        LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[HaulResourceTask] Started hauling {Amount} {ResourceType} to construction site");
    }

    public override void OnUpdate(double delta)
    {
        // Hauling is handled by worker state machine
        // This is called during the task but doesn't need to do anything here
    }

    public override void OnComplete()
    {
        LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[HaulResourceTask] Completed hauling {Amount} {ResourceType}");
    }

    /// <summary>
    /// Cancels the task.
    /// </summary>
    public override void Cancel()
    {
        LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[HaulResourceTask] Cancelled hauling {Amount} {ResourceType}");
        base.Cancel();
    }

    /// <summary>
    /// Attempts to withdraw the resource from the stockpile.
    /// Should be called by the worker when picking up the resource.
    /// </summary>
    /// <param name="stockpile">The stockpile to withdraw from</param>
    /// <returns>True if successful</returns>
    public bool TryWithdrawResource(Stockpile stockpile)
    {
        if (stockpile == null)
        {
            LogManager.Warning("[HaulResourceTask] No stockpile provided");
            return false;
        }

        bool success = stockpile.WithdrawResource(ResourceType, Amount);
        if (!success)
        {
            LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[HaulResourceTask] Failed to withdraw {Amount} {ResourceType} from stockpile");
        }
        return success;
    }

    /// <summary>
    /// Delivers the resource to the construction site.
    /// Should be called by the worker when delivering the resource.
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TryDeliverResource()
    {
        if (!IsValid())
        {
            LogManager.Warning("[HaulResourceTask] Cannot deliver - construction site is invalid");
            return false;
        }

        bool success = TargetSite.DeliverResource(ResourceType, Amount);
        return success;
    }
}
