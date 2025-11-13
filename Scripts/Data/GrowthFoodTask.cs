using Godot;
using autotown.Core;
using autotown.Entities;

namespace autotown.Data;

/// <summary>
/// Task for hauling food from stockpile to town hall for population growth.
/// Builders deliver food in increments until town hall has enough for a new worker.
/// </summary>
public partial class GrowthFoodTask : Task
{
    /// <summary>
    /// Amount of food to haul (from GameConfig.FOOD_PER_HAUL_TRIP).
    /// </summary>
    public int Amount { get; private set; }

    /// <summary>
    /// Source position (stockpile location).
    /// </summary>
    public Vector2 SourcePosition { get; private set; }

    /// <summary>
    /// Destination position (town hall location).
    /// </summary>
    public Vector2 DestinationPosition { get; private set; }

    /// <summary>
    /// The town hall receiving the food.
    /// </summary>
    public TownHall TargetTownHall { get; private set; }

    /// <summary>
    /// Only builders can perform growth food haul tasks.
    /// </summary>
    public override JobType[] ValidJobTypes => new[] { JobType.Builder };

    /// <summary>
    /// Estimated duration based on distance.
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
    /// Creates a new growth food haul task.
    /// </summary>
    /// <param name="amount">Amount of food to haul</param>
    /// <param name="sourcePosition">Stockpile position</param>
    /// <param name="destinationPosition">Town hall position</param>
    /// <param name="targetTownHall">The town hall receiving the food</param>
    public GrowthFoodTask(int amount, Vector2 sourcePosition, Vector2 destinationPosition, TownHall targetTownHall)
    {
        Type = TaskType.Haul;
        Amount = amount;
        SourcePosition = sourcePosition;
        DestinationPosition = destinationPosition;
        TargetTownHall = targetTownHall;

        // Position is set to source for pathfinding
        Position = sourcePosition;

        // High priority - population growth is important
        Priority = 2;
    }

    /// <summary>
    /// Checks if this task is still valid (town hall exists and is active).
    /// </summary>
    public bool IsValid()
    {
        if (TargetTownHall == null || !IsInstanceValid(TargetTownHall))
            return false;

        // Town hall must be active to receive food
        return TargetTownHall.State == BuildingState.Active;
    }

    public override void OnStart()
    {
        if (!IsValid())
        {
            LogManager.Log(LogManager.DEBUG_HAUL_TASK, "[GrowthFoodTask] Cannot start - town hall is invalid or inactive");
            Cancel();
            return;
        }

        LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[GrowthFoodTask] Started hauling {Amount} food to town hall for population growth");
    }

    public override void OnUpdate(double delta)
    {
        // Hauling is handled by worker state machine
        // This is called during the task but doesn't need to do anything here
    }

    public override void OnComplete()
    {
        LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[GrowthFoodTask] Completed hauling {Amount} food to town hall");
    }

    /// <summary>
    /// Cancels the task.
    /// </summary>
    public override void Cancel()
    {
        LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[GrowthFoodTask] Cancelled hauling {Amount} food");
        base.Cancel();
    }

    /// <summary>
    /// Attempts to withdraw food from the stockpile.
    /// Should be called by the worker when picking up the resource.
    /// </summary>
    /// <param name="stockpile">The stockpile to withdraw from</param>
    /// <returns>True if successful</returns>
    public bool TryWithdrawResource(Stockpile stockpile)
    {
        if (stockpile == null)
        {
            LogManager.Warning("[GrowthFoodTask] No stockpile provided");
            return false;
        }

        bool success = stockpile.WithdrawResource(ResourceType.Food, Amount);
        if (!success)
        {
            LogManager.Log(LogManager.DEBUG_HAUL_TASK, $"[GrowthFoodTask] Failed to withdraw {Amount} food from stockpile");
        }
        return success;
    }

    /// <summary>
    /// Delivers the food to the town hall.
    /// Should be called by the worker when delivering the resource.
    /// </summary>
    /// <returns>True if successful</returns>
    public bool TryDeliverFood()
    {
        if (!IsValid())
        {
            LogManager.Warning("[GrowthFoodTask] Cannot deliver - town hall is invalid");
            return false;
        }

        TargetTownHall.DeliverGrowthFood(Amount);
        return true;
    }
}
