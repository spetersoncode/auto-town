using Godot;
using autotown.Core;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Town Hall building - the central hub of the settlement.
/// Accumulates food for population growth and spawns new workers.
/// </summary>
public partial class TownHall : Building
{
    // === Signals ===

    /// <summary>
    /// Emitted when the town hall has accumulated enough food for population growth.
    /// Parameters: int foodAmount
    /// </summary>
    [Signal]
    public delegate void GrowthFoodReadyEventHandler(int foodAmount);

    /// <summary>
    /// Emitted when growth food is delivered to the town hall.
    /// Parameters: int amount, int newTotal
    /// </summary>
    [Signal]
    public delegate void GrowthFoodDeliveredEventHandler(int amount, int newTotal);

    // === Properties ===

    /// <summary>
    /// Amount of food stored at town hall specifically for population growth.
    /// This is separate from the global resource pool.
    /// </summary>
    public int GrowthFoodStorage { get; private set; } = 0;

    /// <summary>
    /// Whether this town hall has enough food for population growth.
    /// </summary>
    public bool HasEnoughFoodForGrowth => GrowthFoodStorage >= GameConfig.FOOD_PER_WORKER;

    // === Lifecycle ===

    protected override void OnReady()
    {
        Type = BuildingType.TownHall;
        GD.Print($"[TownHall] Initialized at {GlobalPosition}");
    }

    protected override void OnActivated()
    {
        GD.Print($"[TownHall] Activated - ready to accept growth food deliveries");
    }

    protected override void OnConstructionCompleted()
    {
        GD.Print($"[TownHall] Construction completed - town hall is operational");
    }

    // === Growth Food Management ===

    /// <summary>
    /// Delivers food to the town hall for population growth.
    /// Called by workers completing GrowthFoodTask.
    /// </summary>
    /// <param name="amount">Amount of food to deliver</param>
    public void DeliverGrowthFood(int amount)
    {
        if (amount <= 0)
        {
            GD.PushWarning($"[TownHall] Cannot deliver non-positive food amount: {amount}");
            return;
        }

        GrowthFoodStorage += amount;
        GD.Print($"[TownHall] Growth food delivered: +{amount} (Total: {GrowthFoodStorage}/{GameConfig.FOOD_PER_WORKER})");

        EmitSignal(SignalName.GrowthFoodDelivered, amount, GrowthFoodStorage);

        // Check if we've reached the threshold for population growth
        if (HasEnoughFoodForGrowth)
        {
            GD.Print($"[TownHall] Growth food threshold reached! ({GrowthFoodStorage}/{GameConfig.FOOD_PER_WORKER})");
            EmitSignal(SignalName.GrowthFoodReady, GrowthFoodStorage);
        }
    }

    /// <summary>
    /// Consumes the growth food to spawn a new worker.
    /// Called by PopulationManager after spawning a new worker.
    /// </summary>
    /// <returns>True if food was consumed successfully</returns>
    public bool ConsumeGrowthFood()
    {
        if (!HasEnoughFoodForGrowth)
        {
            GD.PushWarning($"[TownHall] Cannot consume growth food - insufficient amount ({GrowthFoodStorage}/{GameConfig.FOOD_PER_WORKER})");
            return false;
        }

        GrowthFoodStorage -= GameConfig.FOOD_PER_WORKER;
        GD.Print($"[TownHall] Growth food consumed for new worker. Remaining: {GrowthFoodStorage}");
        return true;
    }

    /// <summary>
    /// Gets the progress toward the next population growth (0-1).
    /// </summary>
    public float GetGrowthProgress()
    {
        return Mathf.Min(1.0f, (float)GrowthFoodStorage / GameConfig.FOOD_PER_WORKER);
    }
}
