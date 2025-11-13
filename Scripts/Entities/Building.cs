using Godot;
using autotown.Data;

namespace autotown.Entities;

/// <summary>
/// Defines the operational state of a building.
/// </summary>
public enum BuildingState
{
    /// <summary>Building is under construction and not yet operational</summary>
    UnderConstruction,

    /// <summary>Building is operational and functioning</summary>
    Active,

    /// <summary>Building exists but is not currently operational</summary>
    Inactive
}

/// <summary>
/// Base class for all buildings in the game.
/// Provides common functionality for building state management and lifecycle.
/// </summary>
public abstract partial class Building : Node2D
{
    // === Signals ===

    /// <summary>
    /// Emitted when the building's state changes.
    /// Parameters: BuildingState oldState, BuildingState newState
    /// </summary>
    [Signal]
    public delegate void BuildingStateChangedEventHandler(BuildingState oldState, BuildingState newState);

    /// <summary>
    /// Emitted when construction is completed and building becomes active.
    /// </summary>
    [Signal]
    public delegate void BuildingCompletedEventHandler();

    // === Properties ===

    /// <summary>
    /// The type of this building.
    /// </summary>
    public BuildingType Type { get; protected set; }

    /// <summary>
    /// Building data containing costs, build time, and properties.
    /// </summary>
    public BuildingData Data { get; protected set; }

    /// <summary>
    /// Current operational state of the building.
    /// </summary>
    public BuildingState State { get; private set; } = BuildingState.UnderConstruction;

    // === Lifecycle ===

    public override void _Ready()
    {
        // Load building data based on type
        Data = BuildingDefinitions.GetBuildingData(Type);

        GD.Print($"[Building] {Data.Name} initialized at {GlobalPosition} in state {State}");

        OnReady();
    }

    public override void _Process(double delta)
    {
        if (State == BuildingState.Active)
        {
            OnActiveProcess(delta);
        }
    }

    // === State Management ===

    /// <summary>
    /// Changes the building's state and emits the state changed signal.
    /// </summary>
    protected void ChangeState(BuildingState newState)
    {
        if (State == newState)
            return;

        var oldState = State;
        State = newState;

        GD.Print($"[Building] {Data.Name} state changed: {oldState} -> {newState}");

        EmitSignal(SignalName.BuildingStateChanged, (int)oldState, (int)newState);

        // Handle state transitions
        switch (newState)
        {
            case BuildingState.Active:
                OnActivated();
                break;
            case BuildingState.Inactive:
                OnDeactivated();
                break;
        }
    }

    /// <summary>
    /// Activates the building, making it operational.
    /// </summary>
    public void Activate()
    {
        ChangeState(BuildingState.Active);
    }

    /// <summary>
    /// Deactivates the building, making it non-operational.
    /// </summary>
    public void Deactivate()
    {
        ChangeState(BuildingState.Inactive);
    }

    /// <summary>
    /// Called when construction is completed and building becomes active.
    /// This is called by ConstructionSite when construction finishes.
    /// </summary>
    public void OnConstructionComplete()
    {
        GD.Print($"[Building] {Data.Name} construction completed");

        ChangeState(BuildingState.Active);

        EmitSignal(SignalName.BuildingCompleted);

        OnConstructionCompleted();
    }

    // === Virtual Methods (Override in derived classes) ===

    /// <summary>
    /// Called during _Ready after building data is loaded.
    /// Override to add building-specific initialization.
    /// </summary>
    protected virtual void OnReady()
    {
    }

    /// <summary>
    /// Called every frame when the building is active.
    /// Override to add building-specific active behavior.
    /// </summary>
    protected virtual void OnActiveProcess(double delta)
    {
    }

    /// <summary>
    /// Called when the building is activated.
    /// Override to add building-specific activation logic.
    /// </summary>
    protected virtual void OnActivated()
    {
    }

    /// <summary>
    /// Called when the building is deactivated.
    /// Override to add building-specific deactivation logic.
    /// </summary>
    protected virtual void OnDeactivated()
    {
    }

    /// <summary>
    /// Called when construction is completed.
    /// Override to add building-specific completion logic.
    /// </summary>
    protected virtual void OnConstructionCompleted()
    {
    }

    /// <summary>
    /// Gets a position where workers can interact with this building.
    /// Default returns the center of the building.
    /// </summary>
    public virtual Vector2 GetInteractionPosition()
    {
        return GlobalPosition;
    }
}
