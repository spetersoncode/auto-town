using Godot;
using System.Collections.Generic;
using System.Linq;
using autotown.Core;
using autotown.Data;
using autotown.Entities;

namespace autotown.Systems;

/// <summary>
/// Manages population growth in the settlement.
/// Coordinates housing capacity, food delivery, and worker spawning.
/// </summary>
public partial class PopulationManager : Node
{
    // === Signals ===

    /// <summary>
    /// Emitted when the population count changes.
    /// Parameters: int newPopulation
    /// </summary>
    [Signal]
    public delegate void PopulationChangedEventHandler(int newPopulation);

    /// <summary>
    /// Emitted when total housing capacity changes.
    /// Parameters: int newCapacity, int occupied, int available
    /// </summary>
    [Signal]
    public delegate void HousingCapacityChangedEventHandler(int newCapacity, int occupied, int available);

    /// <summary>
    /// Emitted when a new worker is spawned due to population growth.
    /// Parameters: Worker worker
    /// </summary>
    [Signal]
    public delegate void WorkerSpawnedFromGrowthEventHandler(Worker worker);

    // === References ===

    private WorkerManager _workerManager;
    private BuildingManager _buildingManager;
    private TaskManager _taskManager;
    private ResourceManager _resourceManager;
    private TownHall _townHall;
    private Stockpile _stockpile;
    private Node _workersContainer;

    // === State ===

    private List<House> _houses = new List<House>();
    private Timer _growthCheckTimer;
    private bool _isInitialized = false;

    // === Properties ===

    /// <summary>
    /// Total housing capacity from all active houses.
    /// </summary>
    public int TotalHousingCapacity { get; private set; } = 0;

    /// <summary>
    /// Number of workers currently occupying housing.
    /// </summary>
    public int OccupiedHousing { get; private set; } = 0;

    /// <summary>
    /// Available housing slots for new workers.
    /// </summary>
    public int AvailableHousing => TotalHousingCapacity - OccupiedHousing;

    /// <summary>
    /// Current population (from WorkerManager).
    /// </summary>
    public int CurrentPopulation => _workerManager?.WorkerCount ?? 0;

    // === Lifecycle ===

    public override void _Ready()
    {
        GD.Print("[PopulationManager] Initializing...");

        // Create growth check timer
        _growthCheckTimer = new Timer();
        _growthCheckTimer.WaitTime = GameConfig.GROWTH_CHECK_INTERVAL;
        _growthCheckTimer.Autostart = false;
        _growthCheckTimer.OneShot = false;
        _growthCheckTimer.Timeout += OnGrowthCheckTimerTimeout;
        AddChild(_growthCheckTimer);

        GD.Print("[PopulationManager] Ready - waiting for initialization");
    }

    /// <summary>
    /// Initializes the PopulationManager with required dependencies.
    /// Called by WorldController after all systems are set up.
    /// </summary>
    public void Initialize(WorkerManager workerManager, BuildingManager buildingManager,
                          TaskManager taskManager, TownHall townHall, Stockpile stockpile,
                          Node workersContainer)
    {
        if (_isInitialized)
        {
            GD.PushWarning("[PopulationManager] Already initialized");
            return;
        }

        _workerManager = workerManager;
        _buildingManager = buildingManager;
        _taskManager = taskManager;
        _resourceManager = GetNode<ResourceManager>("/root/ResourceManager");
        _townHall = townHall;
        _stockpile = stockpile;
        _workersContainer = workersContainer;

        // Subscribe to signals
        _buildingManager.ConstructionCompleted += OnBuildingConstructionCompleted;
        _townHall.GrowthFoodReady += OnGrowthFoodReady;

        // Calculate initial housing capacity from starter workers
        OccupiedHousing = CurrentPopulation;

        _isInitialized = true;

        // Start the growth check timer
        _growthCheckTimer.Start();

        GD.Print($"[PopulationManager] ========================================");
        GD.Print($"[PopulationManager] INITIALIZED SUCCESSFULLY!");
        GD.Print($"[PopulationManager] Population: {CurrentPopulation}");
        GD.Print($"[PopulationManager] Housing Capacity: {TotalHousingCapacity}");
        GD.Print($"[PopulationManager] Available Housing: {AvailableHousing}");
        GD.Print($"[PopulationManager] Occupied Housing: {OccupiedHousing}");
        GD.Print($"[PopulationManager] Timer interval: {GameConfig.GROWTH_CHECK_INTERVAL}s");
        GD.Print($"[PopulationManager] Subscribed to BuildingManager.ConstructionCompleted");
        GD.Print($"[PopulationManager] ========================================");
    }

    // === Growth System ===

    /// <summary>
    /// Periodically checks if conditions are met for population growth.
    /// </summary>
    private void OnGrowthCheckTimerTimeout()
    {
        if (!_isInitialized)
        {
            GD.Print("[PopulationManager] Timer fired but not initialized yet");
            return;
        }

        GD.Print($"[PopulationManager] Growth check - Population: {CurrentPopulation}, Housing: {TotalHousingCapacity}, Available: {AvailableHousing}, Food at town hall: {_townHall?.GrowthFoodStorage ?? -1}");
        CheckGrowthConditions();
    }

    /// <summary>
    /// Checks if conditions are met for population growth and takes appropriate action.
    /// </summary>
    private void CheckGrowthConditions()
    {
        GD.Print($"[PopulationManager] CheckGrowthConditions called");

        // Check if there's available housing
        if (AvailableHousing <= 0)
        {
            GD.Print($"[PopulationManager] No available housing for growth (Current: {CurrentPopulation}, Capacity: {TotalHousingCapacity})");
            return;
        }

        GD.Print($"[PopulationManager] Available housing: {AvailableHousing} slots");

        // Check if town hall has enough food for growth
        if (_townHall.HasEnoughFoodForGrowth)
        {
            GD.Print($"[PopulationManager] Town hall has enough food ({_townHall.GrowthFoodStorage}/{GameConfig.FOOD_PER_WORKER})");
            // Conditions met - spawn a new worker
            SpawnNewWorker();
        }
        else
        {
            GD.Print($"[PopulationManager] Town hall needs more food ({_townHall.GrowthFoodStorage}/{GameConfig.FOOD_PER_WORKER})");
            // Need more food - ensure we have a growth food task
            EnsureGrowthFoodTask();
        }
    }

    /// <summary>
    /// Ensures there's at least one growth food task in the queue.
    /// </summary>
    private void EnsureGrowthFoodTask()
    {
        // Check if there's already a pending growth food task
        var existingTask = _taskManager.GetPendingTasks()
            .OfType<GrowthFoodTask>()
            .FirstOrDefault(t => t.State == TaskState.Pending);

        if (existingTask != null)
        {
            LogManager.Log(LogManager.DEBUG_TASK_MANAGER, "[PopulationManager] Growth food task already exists");
            return;
        }

        // Check if we have enough food in stockpile
        if (!_resourceManager.HasEnough(ResourceType.Food, GameConfig.FOOD_PER_HAUL_TRIP))
        {
            LogManager.Log(LogManager.DEBUG_TASK_MANAGER, $"[PopulationManager] Not enough food in stockpile for haul (need {GameConfig.FOOD_PER_HAUL_TRIP})");
            return;
        }

        // Create a new growth food task
        var growthFoodTask = new GrowthFoodTask(
            GameConfig.FOOD_PER_HAUL_TRIP,
            _stockpile.GlobalPosition,
            _townHall.GlobalPosition,
            _townHall
        );

        _taskManager.AddTask(growthFoodTask);
        GD.Print($"[PopulationManager] Created growth food task - hauling {GameConfig.FOOD_PER_HAUL_TRIP} food to town hall");
    }

    /// <summary>
    /// Called when the town hall has accumulated enough food for growth.
    /// </summary>
    private void OnGrowthFoodReady(int foodAmount)
    {
        GD.Print($"[PopulationManager] Town hall has {foodAmount} food ready for population growth");

        // Check conditions immediately
        CheckGrowthConditions();
    }

    /// <summary>
    /// Spawns a new worker at the town hall.
    /// </summary>
    private void SpawnNewWorker()
    {
        if (AvailableHousing <= 0)
        {
            GD.PushWarning("[PopulationManager] Cannot spawn worker - no available housing");
            return;
        }

        if (!_townHall.HasEnoughFoodForGrowth)
        {
            GD.PushWarning("[PopulationManager] Cannot spawn worker - insufficient growth food");
            return;
        }

        // Consume the growth food
        if (!_townHall.ConsumeGrowthFood())
        {
            GD.PrintErr("[PopulationManager] Failed to consume growth food");
            return;
        }

        // Spawn the worker next to town hall (not inside it) with no job (JobType.None)
        // Offset by 48 pixels (3 tiles) to the left so workers appear adjacent
        Vector2 spawnOffset = new Vector2(-48, 0);
        Vector2 spawnPosition = _townHall.GlobalPosition + spawnOffset;

        var newWorker = _workerManager.SpawnWorker(spawnPosition, JobType.None, _workersContainer);
        if (newWorker == null)
        {
            GD.PrintErr("[PopulationManager] Failed to spawn new worker");
            return;
        }

        // Assign worker to an available house
        AssignWorkerToHouse(newWorker);

        // Update occupied housing count
        OccupiedHousing++;

        // Emit signals
        EmitSignal(SignalName.PopulationChanged, CurrentPopulation);
        EmitSignal(SignalName.WorkerSpawnedFromGrowth, newWorker);
        EmitSignal(SignalName.HousingCapacityChanged, TotalHousingCapacity, OccupiedHousing, AvailableHousing);

        GD.Print($"[PopulationManager] New worker spawned! Population: {CurrentPopulation}/{TotalHousingCapacity}");
    }

    /// <summary>
    /// Assigns a worker to an available house.
    /// </summary>
    private void AssignWorkerToHouse(Worker worker)
    {
        var availableHouse = _houses.FirstOrDefault(h => h.State == BuildingState.Active && !h.IsFull);

        if (availableHouse == null)
        {
            GD.PushWarning("[PopulationManager] No available house found for worker assignment");
            return;
        }

        bool success = availableHouse.AssignWorker();
        if (success)
        {
            GD.Print($"[PopulationManager] Worker assigned to house at {availableHouse.GlobalPosition}");
        }
    }

    // === Housing Management ===

    /// <summary>
    /// Called when a building construction is completed.
    /// </summary>
    private void OnBuildingConstructionCompleted(Building building)
    {
        GD.Print($"[PopulationManager] Building construction completed: {building.Data.Name} (Type: {building.GetType().Name})");

        // Check if it's a house
        if (building is House house)
        {
            GD.Print($"[PopulationManager] Detected house completion!");
            RegisterHouse(house);
        }
        else
        {
            GD.Print($"[PopulationManager] Building is not a House, it's a {building.GetType().Name}");
        }
    }

    /// <summary>
    /// Registers a house and updates housing capacity.
    /// </summary>
    private void RegisterHouse(House house)
    {
        if (_houses.Contains(house))
        {
            GD.PushWarning("[PopulationManager] House already registered");
            return;
        }

        _houses.Add(house);
        TotalHousingCapacity += house.HousingCapacity;

        EmitSignal(SignalName.HousingCapacityChanged, TotalHousingCapacity, OccupiedHousing, AvailableHousing);

        GD.Print($"[PopulationManager] House registered - Total capacity: {TotalHousingCapacity}, Available: {AvailableHousing}");

        // Check if we can grow now that housing is available
        CheckGrowthConditions();
    }

    /// <summary>
    /// Unregisters a house (e.g., if destroyed).
    /// </summary>
    public void UnregisterHouse(House house)
    {
        if (!_houses.Contains(house))
            return;

        _houses.Remove(house);
        TotalHousingCapacity -= house.HousingCapacity;
        OccupiedHousing -= house.CurrentOccupancy;

        EmitSignal(SignalName.HousingCapacityChanged, TotalHousingCapacity, OccupiedHousing, AvailableHousing);

        GD.Print($"[PopulationManager] House unregistered - Total capacity: {TotalHousingCapacity}");
    }
}
