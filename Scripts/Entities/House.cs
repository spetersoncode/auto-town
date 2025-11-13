using Godot;
using autotown.Core;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// House building that provides housing capacity for population growth.
/// Passive building with no active processing functionality.
/// </summary>
public partial class House : Building
{
    /// <summary>
    /// Housing capacity provided by this house.
    /// </summary>
    public int HousingCapacity { get; private set; } = GameConfig.HOUSE_CAPACITY;

    /// <summary>
    /// Number of workers currently living in this house.
    /// </summary>
    public int CurrentOccupancy { get; private set; } = 0;

    /// <summary>
    /// Whether this house is full.
    /// </summary>
    public bool IsFull => CurrentOccupancy >= HousingCapacity;

    /// <summary>
    /// Remaining capacity in this house.
    /// </summary>
    public int RemainingCapacity => Mathf.Max(0, HousingCapacity - CurrentOccupancy);

    protected override void OnReady()
    {
        Type = BuildingType.House;
        GD.Print($"[House] Initialized with capacity {HousingCapacity}");
    }

    protected override void OnActivated()
    {
        GD.Print($"[House] Activated - providing {HousingCapacity} housing slots");
    }

    protected override void OnConstructionCompleted()
    {
        GD.Print($"[House] Construction completed - now providing housing for population");
    }

    /// <summary>
    /// Assigns a worker to live in this house.
    /// </summary>
    /// <returns>True if successful, false if house is full</returns>
    public bool AssignWorker()
    {
        if (IsFull)
        {
            GD.PushWarning($"[House] Cannot assign worker - house is full ({CurrentOccupancy}/{HousingCapacity})");
            return false;
        }

        CurrentOccupancy++;
        GD.Print($"[House] Worker assigned ({CurrentOccupancy}/{HousingCapacity})");
        return true;
    }

    /// <summary>
    /// Removes a worker from this house.
    /// </summary>
    /// <returns>True if successful</returns>
    public bool RemoveWorker()
    {
        if (CurrentOccupancy <= 0)
        {
            GD.PushWarning($"[House] Cannot remove worker - house is empty");
            return false;
        }

        CurrentOccupancy--;
        GD.Print($"[House] Worker removed ({CurrentOccupancy}/{HousingCapacity})");
        return true;
    }

    /// <summary>
    /// Gets the current occupancy rate as a percentage (0-1).
    /// </summary>
    public float GetOccupancyRate()
    {
        return HousingCapacity > 0 ? (float)CurrentOccupancy / HousingCapacity : 0f;
    }
}
