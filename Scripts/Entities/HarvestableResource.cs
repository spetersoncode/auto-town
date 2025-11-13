using Godot;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Base class for all harvestable resource nodes in the game world.
/// Handles harvest state, depletion, worker reservation, and visual feedback.
/// </summary>
public partial class HarvestableResource : Node2D
{
    // === Enums ===

    /// <summary>
    /// Current state of the resource node.
    /// </summary>
    public enum HarvestState
    {
        Available,      // Ready to be harvested
        BeingHarvested, // Currently being harvested by a worker
        Depleted        // No resources remaining
    }

    // === Signals ===

    /// <summary>
    /// Emitted when a harvest operation starts.
    /// Parameters: HarvestableResource resource
    /// </summary>
    [Signal]
    public delegate void HarvestStartedEventHandler(HarvestableResource resource);

    /// <summary>
    /// Emitted when a harvest operation completes.
    /// Parameters: ResourceType type, int amountHarvested
    /// </summary>
    [Signal]
    public delegate void HarvestCompletedEventHandler(ResourceType type, int amountHarvested);

    /// <summary>
    /// Emitted when the resource node is fully depleted.
    /// Parameters: HarvestableResource resource
    /// </summary>
    [Signal]
    public delegate void ResourceDepletedEventHandler(HarvestableResource resource);

    // === Exported Properties ===

    /// <summary>
    /// The type of resource this node provides.
    /// </summary>
    [Export]
    public ResourceType ResourceType { get; set; } = ResourceType.Wood;

    /// <summary>
    /// Maximum number of times this resource can be harvested.
    /// </summary>
    [Export]
    public int MaxHarvests { get; set; } = 5;

    /// <summary>
    /// Amount of resource yielded per harvest.
    /// </summary>
    [Export]
    public int YieldPerHarvest { get; set; } = 10;

    /// <summary>
    /// Time required to complete a harvest in seconds.
    /// </summary>
    [Export]
    public float HarvestDuration { get; set; } = 2.0f;

    // === Private Fields ===

    /// <summary>
    /// Current number of harvests completed.
    /// </summary>
    private int _currentHarvests = 0;

    /// <summary>
    /// Current state of the resource.
    /// </summary>
    private HarvestState _state = HarvestState.Available;

    /// <summary>
    /// Reference to the worker currently harvesting this resource (if any).
    /// </summary>
    private Node _reservedByWorker = null;

    /// <summary>
    /// Current harvest progress (0.0 to 1.0).
    /// </summary>
    private float _harvestProgress = 0.0f;

    /// <summary>
    /// Reference to the visual node (ColorRect) for visual feedback.
    /// </summary>
    private ColorRect _visual;

    // === Public Properties ===

    /// <summary>
    /// Gets the current state of the resource.
    /// </summary>
    public HarvestState State => _state;

    /// <summary>
    /// Gets the current harvest count.
    /// </summary>
    public int CurrentHarvests => _currentHarvests;

    /// <summary>
    /// Gets the remaining harvest count.
    /// </summary>
    public int RemainingHarvests => MaxHarvests - _currentHarvests;

    /// <summary>
    /// Gets the current harvest progress (0.0 to 1.0).
    /// </summary>
    public float HarvestProgress => _harvestProgress;

    /// <summary>
    /// Checks if this resource is currently reserved by a worker.
    /// </summary>
    public bool IsReserved => _reservedByWorker != null;

    // === Lifecycle ===

    public override void _Ready()
    {
        // Get the visual node (currently not used, but available for future features)
        _visual = GetNodeOrNull<ColorRect>("Visual");

        // Reduced logging - only log once per game instead of per node
        // GD.Print($"[HarvestableResource] {ResourceType} node initialized at {GlobalPosition}. " +
        //          $"Max Harvests: {MaxHarvests}, Yield: {YieldPerHarvest}");
    }

    public override void _Process(double delta)
    {
        // Update harvest progress if being harvested
        if (_state == HarvestState.BeingHarvested && HarvestDuration > 0)
        {
            float oldProgress = _harvestProgress;
            _harvestProgress += (float)delta / HarvestDuration;

            // Only log at 25% intervals to reduce spam
            int oldQuarter = (int)(oldProgress * 4);
            int newQuarter = (int)(_harvestProgress * 4);
            if (newQuarter > oldQuarter && newQuarter <= 4)
            {
                GD.Print($"[HarvestableResource] {ResourceType} harvesting... progress: {_harvestProgress * 100:F0}%");
            }

            if (_harvestProgress >= 1.0f)
            {
                CompleteHarvest();
            }
        }
    }

    // === Public Methods ===

    /// <summary>
    /// Checks if this resource can be harvested.
    /// </summary>
    public bool CanBeHarvested()
    {
        return _state == HarvestState.Available && !IsReserved;
    }

    /// <summary>
    /// Reserves this resource for a worker to prevent multiple workers from claiming it.
    /// </summary>
    /// <param name="worker">The worker node reserving this resource</param>
    /// <returns>True if reservation successful, false if already reserved or unavailable</returns>
    public bool ReserveForHarvest(Node worker)
    {
        if (!CanBeHarvested())
        {
            return false;
        }

        _reservedByWorker = worker;
        GD.Print($"[HarvestableResource] {ResourceType} reserved by worker at {GlobalPosition}");
        return true;
    }

    /// <summary>
    /// Releases the reservation on this resource.
    /// </summary>
    /// <param name="worker">The worker releasing the reservation</param>
    public void ReleaseReservation(Node worker)
    {
        if (_reservedByWorker == worker)
        {
            _reservedByWorker = null;
            GD.Print($"[HarvestableResource] {ResourceType} reservation released at {GlobalPosition}");
        }
    }

    /// <summary>
    /// Starts the harvest process.
    /// Must be called by a worker after successful reservation.
    /// </summary>
    /// <param name="worker">The worker starting the harvest</param>
    /// <returns>True if harvest started successfully</returns>
    public bool StartHarvest(Node worker)
    {
        if (_reservedByWorker != worker)
        {
            GD.PushWarning($"[HarvestableResource] Worker attempted to harvest without reservation");
            return false;
        }

        if (_state != HarvestState.Available)
        {
            GD.PushWarning($"[HarvestableResource] Cannot harvest - state is {_state}");
            return false;
        }

        _state = HarvestState.BeingHarvested;
        _harvestProgress = 0.0f;

        GD.Print($"[HarvestableResource] Harvest started on {ResourceType} at {GlobalPosition}. Duration: {HarvestDuration}s, IsProcessing: {IsProcessing()}");
        EmitSignal(SignalName.HarvestStarted, this);

        return true;
    }

    /// <summary>
    /// Completes the harvest and yields resources.
    /// Called automatically when harvest progress reaches 100%.
    /// </summary>
    public void CompleteHarvest()
    {
        if (_state != HarvestState.BeingHarvested)
        {
            GD.PushWarning($"[HarvestableResource] CompleteHarvest called but state is {_state}");
            return;
        }

        // Increment harvest count
        _currentHarvests++;
        _harvestProgress = 0.0f;

        GD.Print($"[HarvestableResource] Harvest completed! Yielded {YieldPerHarvest} {ResourceType}. " +
                 $"Harvests: {_currentHarvests}/{MaxHarvests}");

        // Emit harvest completed signal
        EmitSignal(SignalName.HarvestCompleted, (int)ResourceType, YieldPerHarvest);

        // Check if depleted
        if (_currentHarvests >= MaxHarvests)
        {
            Deplete();
        }
        else
        {
            // Return to available state
            _state = HarvestState.Available;
            _reservedByWorker = null;
        }
    }

    /// <summary>
    /// Manually triggers depletion (for testing or special cases).
    /// </summary>
    public void Deplete()
    {
        if (_state == HarvestState.Depleted)
        {
            return;
        }

        _state = HarvestState.Depleted;
        _reservedByWorker = null;
        _harvestProgress = 0.0f;

        GD.Print($"[HarvestableResource] {ResourceType} depleted at {GlobalPosition} - removing from scene");

        // Emit depleted signal before removal
        EmitSignal(SignalName.ResourceDepleted, this);

        // Remove the node from the scene entirely
        QueueFree();
    }

    /// <summary>
    /// Gets information about this resource for debugging.
    /// </summary>
    public string GetDebugInfo()
    {
        return $"{ResourceType} - State: {_state}, Harvests: {_currentHarvests}/{MaxHarvests}, " +
               $"Reserved: {IsReserved}, Progress: {_harvestProgress:F2}";
    }
}
