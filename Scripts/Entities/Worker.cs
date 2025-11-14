using Godot;
using System;
using autotown.Core;
using autotown.Data;
using autotown.Systems;

namespace autotown.Entities;

/// <summary>
/// Worker entity that autonomously finds and completes tasks.
/// </summary>
public partial class Worker : CharacterBody2D
{
    private WorkerData _data;
    private NavigationAgent2D _navAgent;
    private Polygon2D _sprite;
    private Polygon2D _selectionIndicator;

    private WorkerState _state = WorkerState.Idle;
    private Task _currentTask;
    private float _taskScanTimer = 0f;
    private bool _isSelected = false;
    private bool _navigationReady = false;

    // Cached references to autoloads
    private TaskManager _taskManager;
    private ResourceManager _resourceManager;
    private Stockpile _stockpile;

    [Export]
    public float MovementSpeed { get; set; } = GameConfig.DEFAULT_WORKER_SPEED;

    [Export]
    public float InteractionRange { get; set; } = GameConfig.WORKER_INTERACTION_RANGE;

    public WorkerData Data => _data;
    public WorkerState State => _state;
    public bool IsSelected => _isSelected;

    [Signal]
    public delegate void WorkerStateChangedEventHandler(Worker worker, WorkerState newState);

    [Signal]
    public delegate void WorkerJobChangedEventHandler(Worker worker, JobType newJob);

    [Signal]
    public delegate void WorkerSelectedEventHandler(Worker worker);

    [Signal]
    public delegate void WorkerDeselectedEventHandler(Worker worker);

    public override void _Ready()
    {
        // Initialize worker data
        _data = new WorkerData
        {
            Job = JobType.None,
            State = WorkerState.Idle,
            MovementSpeed = MovementSpeed,
            Efficiency = GameConfig.DEFAULT_WORKER_EFFICIENCY
        };

        // Get navigation agent (will be added as child in scene)
        _navAgent = GetNodeOrNull<NavigationAgent2D>("NavigationAgent2D");
        if (_navAgent == null)
        {
            GD.PrintErr("Worker: NavigationAgent2D not found! Worker will not be able to move.");
        }
        else
        {
            // Configure navigation agent with avoidance for dynamic obstacle avoidance
            _navAgent.MaxSpeed = GameConfig.NAV_AGENT_MAX_SPEED;
            _navAgent.PathDesiredDistance = GameConfig.NAV_AGENT_PATH_DESIRED_DISTANCE;
            _navAgent.TargetDesiredDistance = GameConfig.NAV_AGENT_TARGET_DESIRED_DISTANCE;
            _navAgent.Radius = GameConfig.NAV_AGENT_RADIUS;

            // Enable avoidance for dynamic worker-to-worker avoidance
            // This prevents workers from bunching up and getting stuck
            _navAgent.AvoidanceEnabled = true;
            _navAgent.AvoidanceLayers = 1; // Workers avoid layer 1
            _navAgent.AvoidanceMask = 1;   // Workers are on layer 1 for avoidance

            // Wait for navigation map to be ready
            CallDeferred(MethodName.CheckNavigationReady);
        }

        // Get sprite (will be added in scene)
        _sprite = GetNodeOrNull<Polygon2D>("Sprite");
        if (_sprite == null)
        {
            // Create placeholder sprite if not in scene (circular)
            _sprite = PlaceholderSprite.CreateWorkerSprite(GameConfig.WORKER_SPRITE_SIZE);
            _sprite.Name = "Sprite";
            AddChild(_sprite);
        }

        // Create selection indicator (circular outline, slightly larger)
        _selectionIndicator = PlaceholderSprite.CreateCircle(
            GameConfig.WORKER_SPRITE_SIZE / 2 + 2,
            new Color(1, 1, 0, 0.5f), // Yellow semi-transparent
            32
        );
        _selectionIndicator.Name = "SelectionIndicator";
        _selectionIndicator.Visible = false;
        AddChild(_selectionIndicator);

        // Cache autoload references
        _taskManager = GetNode<TaskManager>("/root/TaskManager");
        _resourceManager = GetNode<ResourceManager>("/root/ResourceManager");

        // Collision setup - minimal collision for clicking only
        var collisionShape = new CollisionShape2D();
        var circle = new CircleShape2D { Radius = GameConfig.NAV_AGENT_RADIUS };
        collisionShape.Shape = circle;
        AddChild(collisionShape);

        // Configure collision layers to prevent worker-to-worker collision
        // Workers are on layer 2, but don't collide with anything (mask = 0)
        // This prevents getting stuck while still allowing mouse clicks
        CollisionLayer = 2; // Workers are on layer 2
        CollisionMask = 0;  // Workers don't collide with anything

        SetState(WorkerState.Idle);
    }

    public override void _PhysicsProcess(double delta)
    {
        switch (_state)
        {
            case WorkerState.Idle:
                UpdateIdle(delta);
                break;

            case WorkerState.Moving:
                UpdateMoving(delta);
                break;

            case WorkerState.Working:
                UpdateWorking(delta);
                break;

            case WorkerState.Hauling:
                UpdateHauling(delta);
                break;
        }
    }

    private void UpdateIdle(double delta)
    {
        // Scan for tasks periodically
        _taskScanTimer += (float)delta;
        if (_taskScanTimer >= GameConfig.TASK_SCAN_INTERVAL)
        {
            _taskScanTimer = 0f;
            ScanForTask();
        }
    }

    private void UpdateMoving(double delta)
    {
        if (_navAgent == null)
        {
            GD.PrintErr("Worker: Cannot move - NavigationAgent2D is null");
            SetState(WorkerState.Idle);
            return;
        }

        if (!_navigationReady)
        {
            // Still waiting for navigation to be ready
            return;
        }

        // Check if we've reached the destination
        if (_navAgent.IsNavigationFinished())
        {
            OnReachedDestination();
            return;
        }

        // Get the next position on the path
        var nextPosition = _navAgent.GetNextPathPosition();
        var direction = (nextPosition - GlobalPosition).Normalized();

        // Set velocity and move
        Velocity = direction * MovementSpeed;
        MoveAndSlide();
    }

    private void UpdateWorking(double delta)
    {
        // Worker is waiting for harvest to complete
        // HarvestableResource handles its own progress

        if (_currentTask is GatherTask gatherTask)
        {
            // Check if resource is still valid
            if (!gatherTask.IsValid())
            {
                // Resource was depleted or became invalid
                GD.Print($"Worker: Resource invalid, canceling task");
                _currentTask.Cancel();
                _currentTask = null;
                SetState(WorkerState.Idle);
                return;
            }

            var resourceState = gatherTask.ResourceNode.State;

            // Check if depleted
            if (resourceState == HarvestableResource.HarvestState.Depleted)
            {
                GD.Print($"Worker: Resource depleted, canceling task");
                _currentTask.Cancel();
                _currentTask = null;
                SetState(WorkerState.Idle);
                return;
            }

            // Check if we've finished harvesting (state went back to Available after being harvested)
            if (resourceState == HarvestableResource.HarvestState.Available)
            {
                // Harvest completed! Add resources to inventory
                int amountAdded = _data.AddToInventory(gatherTask.ResourceType, gatherTask.ExpectedYield);

                if (amountAdded > 0)
                {
                    GD.Print($"Worker: Harvest completed! Added {amountAdded} {gatherTask.ResourceType}, inventory: {_data.CarriedAmount}/{WorkerData.MAX_INVENTORY_CAPACITY}");
                }

                // Check if worker's inventory is full or resource is depleted
                if (_data.IsInventoryFull())
                {
                    GD.Print($"Worker: Inventory full ({_data.CarriedAmount}/{WorkerData.MAX_INVENTORY_CAPACITY}), hauling to stockpile");
                    SetState(WorkerState.Hauling);
                    SetDestination(gatherTask.StockpilePosition);
                }
                else if (gatherTask.ResourceNode.State == HarvestableResource.HarvestState.Depleted)
                {
                    GD.Print($"Worker: Resource depleted with {_data.CarriedAmount} resources, hauling to stockpile");
                    SetState(WorkerState.Hauling);
                    SetDestination(gatherTask.StockpilePosition);
                }
                else if (_data.CanCarryMore())
                {
                    // Inventory not full and resource still available - harvest again!
                    GD.Print($"Worker: Inventory has space ({_data.CarriedAmount}/{WorkerData.MAX_INVENTORY_CAPACITY}), harvesting again");

                    // Reserve and start another harvest cycle
                    if (gatherTask.TryReserveResource(this))
                    {
                        bool started = gatherTask.ResourceNode.StartHarvest(this);
                        if (!started)
                        {
                            GD.Print($"Worker: Failed to start next harvest, hauling current resources");
                            SetState(WorkerState.Hauling);
                            SetDestination(gatherTask.StockpilePosition);
                        }
                    }
                    else
                    {
                        GD.Print($"Worker: Failed to reserve for next harvest, hauling current resources");
                        SetState(WorkerState.Hauling);
                        SetDestination(gatherTask.StockpilePosition);
                    }
                }
            }

            // Otherwise, still BeingHarvested - just wait
        }
        else if (_currentTask is BuildTask buildTask)
        {
            // Check if build task is still valid
            if (!buildTask.IsValid())
            {
                GD.Print($"Worker: Build task invalid, canceling");
                _currentTask.Cancel();
                _currentTask = null;
                SetState(WorkerState.Idle);
                return;
            }

            // Update the build task progress
            buildTask.OnUpdate(delta);

            // Task will complete itself when finished
            if (buildTask.State == TaskState.Completed)
            {
                _currentTask = null;
                SetState(WorkerState.Idle);
            }
        }
        else if (_currentTask is ProcessTask processTask)
        {
            // Check if process task is still valid
            if (!processTask.IsValid())
            {
                GD.Print($"Worker: Process task invalid, canceling");
                _currentTask.Cancel();
                _currentTask = null;
                SetState(WorkerState.Idle);
                return;
            }

            // Update the process task progress
            processTask.OnUpdate(delta);

            // Task will complete itself when finished
            if (processTask.State == TaskState.Completed)
            {
                _currentTask = null;

                // Worker now has resources in inventory, transition to hauling
                if (_data.IsCarryingResources())
                {
                    GD.Print($"Worker: Production complete, hauling {_data.CarriedAmount} {_data.CarriedResource} to stockpile");
                    SetState(WorkerState.Hauling);
                    SetDestination(_stockpile.GlobalPosition);
                }
                else
                {
                    // No resources produced (edge case), go idle
                    SetState(WorkerState.Idle);
                }
            }
        }
    }

    private void UpdateHauling(double delta)
    {
        // Hauling is just moving while carrying resources
        // Reuse the movement logic
        if (_navAgent == null)
        {
            GD.PrintErr("Worker: Cannot haul - NavigationAgent2D is null");
            SetState(WorkerState.Idle);
            return;
        }

        if (!_navigationReady)
        {
            // Still waiting for navigation to be ready
            return;
        }

        // Check if we've reached the stockpile
        if (_navAgent.IsNavigationFinished())
        {
            OnReachedDestination();
            return;
        }

        // Move towards stockpile
        var nextPosition = _navAgent.GetNextPathPosition();
        var direction = (nextPosition - GlobalPosition).Normalized();

        Velocity = direction * MovementSpeed;
        MoveAndSlide();
    }

    private void ScanForTask()
    {
        if (_data.Job == JobType.None)
            return;

        if (!_navigationReady)
        {
            // Wait for navigation to be ready before assigning tasks
            return;
        }

        // Don't scan for new tasks if already assigned to a task
        if (_currentTask != null)
        {
            return;
        }

        // First, check if there are any existing pending tasks
        var task = _taskManager.FindBestTaskFor(_data.Job, GlobalPosition);
        if (task != null)
        {
            AssignTask(task);
            return;
        }

        // If no tasks available, find a resource directly
        var resource = FindNearestAvailableResource();
        if (resource != null)
        {
            // Create a task on-demand for this resource
            var gatherTask = new GatherTask(resource, _stockpile.GlobalPosition);
            _taskManager.AddTask(gatherTask);
            AssignTask(gatherTask);
        }
    }

    private HarvestableResource FindNearestAvailableResource()
    {
        // Determine which resource type we're looking for based on job
        ResourceType? targetResourceType = _data.Job switch
        {
            JobType.Lumberjack => ResourceType.Wood,
            JobType.Miner => ResourceType.Stone,
            JobType.Forager => ResourceType.Food,
            _ => null
        };

        if (!targetResourceType.HasValue)
            return null;

        // Get the ResourceNodes container from the world
        var world = GetTree().Root.GetNode<Node2D>("Main/World");
        if (world == null)
            return null;

        var resourceNodesContainer = world.GetNodeOrNull<Node2D>("ResourceNodes");
        if (resourceNodesContainer == null)
            return null;

        // Find the closest available resource of our type
        HarvestableResource closestResource = null;
        float closestDistance = float.MaxValue;

        foreach (var child in resourceNodesContainer.GetChildren())
        {
            if (child is HarvestableResource resource)
            {
                // Check if it's the right type and available
                if (resource.ResourceType == targetResourceType.Value && resource.CanBeHarvested())
                {
                    float distance = GlobalPosition.DistanceTo(resource.GlobalPosition);

                    // Only consider resources within range if configured
                    if (GameConfig.MAX_TASK_DISTANCE > 0 && distance > GameConfig.MAX_TASK_DISTANCE)
                        continue;

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestResource = resource;
                    }
                }
            }
        }

        return closestResource;
    }

    private void AssignTask(Task task)
    {
        if (task is GatherTask gatherTask)
        {
            // Try to reserve the resource
            if (!gatherTask.TryReserveResource(this))
            {
                GD.Print($"Worker: Failed to reserve resource for task");
                return;
            }

            // Try to assign the task
            if (!task.TryAssign(this))
            {
                gatherTask.ReleaseResourceReservation(this);
                GD.Print($"Worker: Failed to assign task");
                return;
            }

            _currentTask = task;

            // Set the worker reference on the GatherTask so it can manage inventory
            gatherTask.SetAssignedWorker(_data);

            GD.Print($"Worker: Assigned {task.Type} task at {task.Position}");

            // Start moving to task location
            SetState(WorkerState.Moving);
            SetDestination(task.Position);

            task.OnStart();
        }
        else if (task is HaulResourceTask haulTask)
        {
            // Validate haul task
            if (!haulTask.IsValid())
            {
                GD.Print($"Worker: Haul task is invalid");
                return;
            }

            // Try to assign the task
            if (!task.TryAssign(this))
            {
                GD.Print($"Worker: Failed to assign haul task");
                return;
            }

            _currentTask = task;
            GD.Print($"Worker: Assigned Haul task - {haulTask.Amount} {haulTask.ResourceType}");

            // Start moving to source (stockpile) to pick up resources
            SetState(WorkerState.Moving);
            SetDestination(haulTask.SourcePosition);

            task.OnStart();
        }
        else if (task is GrowthFoodTask growthFoodTask)
        {
            // Validate growth food task
            if (!growthFoodTask.IsValid())
            {
                GD.Print($"Worker: Growth food task is invalid");
                return;
            }

            // Try to assign the task
            if (!task.TryAssign(this))
            {
                GD.Print($"Worker: Failed to assign growth food task");
                return;
            }

            _currentTask = task;
            GD.Print($"Worker: Assigned Growth Food task - hauling {growthFoodTask.Amount} food to town hall");

            // Start moving to source (stockpile) to pick up food
            SetState(WorkerState.Moving);
            SetDestination(growthFoodTask.SourcePosition);

            task.OnStart();
        }
        else if (task is BuildTask buildTask)
        {
            // Validate build task
            if (!buildTask.IsValid())
            {
                GD.Print($"Worker: Build task is invalid");
                return;
            }

            // Try to assign the task
            if (!task.TryAssign(this))
            {
                GD.Print($"Worker: Failed to assign build task");
                return;
            }

            _currentTask = task;
            GD.Print($"Worker: Assigned Build task at {task.Position}");

            // Start moving to construction site
            SetState(WorkerState.Moving);
            SetDestination(task.Position);

            task.OnStart();
        }
        else if (task is ProcessTask processTask)
        {
            // Validate process task
            if (!processTask.IsValid())
            {
                GD.Print($"Worker: Process task is invalid");
                return;
            }

            // Try to assign the task
            if (!task.TryAssign(this))
            {
                GD.Print($"Worker: Failed to assign process task");
                return;
            }

            _currentTask = task;

            // Set the worker reference on the ProcessTask so it can manage inventory
            processTask.SetAssignedWorker(_data);

            GD.Print($"Worker: Assigned Process task at {task.Position}");

            // Start moving to building
            SetState(WorkerState.Moving);
            SetDestination(task.Position);

            task.OnStart();
        }
    }

    private void OnReachedDestination()
    {
        if (_state == WorkerState.Moving && _currentTask != null)
        {
            // Reached the resource node
            if (_currentTask is GatherTask gatherTask)
            {
                // Check if we're within interaction range
                var distance = GlobalPosition.DistanceTo(gatherTask.Position);
                if (distance <= InteractionRange)
                {
                    // Start harvesting
                    GD.Print($"Worker: Reached resource, starting harvest. Current resource state: {gatherTask.ResourceNode.State}");
                    bool harvestStarted = gatherTask.ResourceNode.StartHarvest(this);
                    if (harvestStarted)
                    {
                        SetState(WorkerState.Working);
                        GD.Print($"Worker: Started harvesting at {gatherTask.Position}. Resource state after start: {gatherTask.ResourceNode.State}");
                    }
                    else
                    {
                        GD.PrintErr($"Worker: Failed to start harvest! Resource state: {gatherTask.ResourceNode.State}");
                        _currentTask.Cancel();
                        _currentTask = null;
                        SetState(WorkerState.Idle);
                    }
                }
                else
                {
                    GD.Print($"Worker: Not within interaction range ({distance} > {InteractionRange}), retrying navigation");
                    SetDestination(gatherTask.Position);
                }
            }
            else if (_currentTask is HaulResourceTask haulTask)
            {
                // Reached source (stockpile) to pick up resources
                GD.Print($"Worker: Reached stockpile to pick up {haulTask.Amount} {haulTask.ResourceType}");

                // Try to withdraw resources from stockpile
                if (haulTask.TryWithdrawResource(_stockpile))
                {
                    // Pick up resources
                    _data.PickupResource(haulTask.ResourceType, haulTask.Amount);
                    GD.Print($"Worker: Picked up {haulTask.Amount} {haulTask.ResourceType}");

                    // Now haul to construction site
                    SetState(WorkerState.Hauling);
                    SetDestination(haulTask.DestinationPosition);
                }
                else
                {
                    // Failed to withdraw resources - cancel task
                    GD.Print($"Worker: Failed to withdraw resources, canceling haul task");
                    _currentTask.Cancel();
                    _currentTask = null;
                    SetState(WorkerState.Idle);
                }
            }
            else if (_currentTask is GrowthFoodTask growthFoodTask)
            {
                // Reached source (stockpile) to pick up food for population growth
                GD.Print($"Worker: Reached stockpile to pick up {growthFoodTask.Amount} food for population growth");

                // Try to withdraw food from stockpile
                if (growthFoodTask.TryWithdrawResource(_stockpile))
                {
                    // Pick up food
                    _data.PickupResource(ResourceType.Food, growthFoodTask.Amount);
                    GD.Print($"Worker: Picked up {growthFoodTask.Amount} food for town hall");

                    // Now haul to town hall
                    SetState(WorkerState.Hauling);
                    SetDestination(growthFoodTask.DestinationPosition);
                }
                else
                {
                    // Failed to withdraw food - cancel task
                    GD.Print($"Worker: Failed to withdraw food, canceling growth food task");
                    _currentTask.Cancel();
                    _currentTask = null;
                    SetState(WorkerState.Idle);
                }
            }
            else if (_currentTask is BuildTask buildTask)
            {
                // Reached construction site
                GD.Print($"Worker: Reached construction site, starting build work");
                SetState(WorkerState.Working);
            }
            else if (_currentTask is ProcessTask processTask)
            {
                // Reached building for processing
                GD.Print($"Worker: Reached building, starting processing work");
                SetState(WorkerState.Working);
            }
        }
        else if (_state == WorkerState.Hauling && _currentTask != null)
        {
            if (_currentTask is GatherTask)
            {
                // Reached the stockpile after gathering
                DepositResources();

                // Complete the task
                _currentTask.Complete();
                _currentTask = null;

                SetState(WorkerState.Idle);
            }
            else if (_currentTask is HaulResourceTask haulTask)
            {
                // Reached construction site with resources
                GD.Print($"Worker: Reached construction site, delivering {_data.CarriedAmount} {_data.CarriedResource}");

                // Deliver resources to construction site
                if (haulTask.TryDeliverResource())
                {
                    // Clear carried resources
                    _data.DropResource();
                    GD.Print($"Worker: Delivered resources to construction site");

                    // Complete the haul task
                    _currentTask.Complete();
                    _currentTask = null;

                    SetState(WorkerState.Idle);
                }
                else
                {
                    GD.Print($"Worker: Failed to deliver resources, canceling task");
                    _data.DropResource();
                    _currentTask.Cancel();
                    _currentTask = null;
                    SetState(WorkerState.Idle);
                }
            }
            else if (_currentTask is GrowthFoodTask growthFoodTask)
            {
                // Reached town hall with food for population growth
                GD.Print($"Worker: Reached town hall, delivering {_data.CarriedAmount} food for population growth");

                // Deliver food to town hall
                if (growthFoodTask.TryDeliverFood())
                {
                    // Clear carried resources
                    _data.DropResource();
                    GD.Print($"Worker: Delivered {growthFoodTask.Amount} food to town hall");

                    // Complete the growth food task
                    _currentTask.Complete();
                    _currentTask = null;

                    SetState(WorkerState.Idle);
                }
                else
                {
                    GD.Print($"Worker: Failed to deliver food to town hall, canceling task");
                    _data.DropResource();
                    _currentTask.Cancel();
                    _currentTask = null;
                    SetState(WorkerState.Idle);
                }
            }
        }
        else if (_state == WorkerState.Hauling && _currentTask == null)
        {
            // Worker is hauling without a task (e.g., after completing ProcessTask)
            // Deposit resources at stockpile
            GD.Print($"Worker: Reached stockpile with {_data.CarriedAmount} {_data.CarriedResource}, depositing");
            DepositResources();
            SetState(WorkerState.Idle);
        }
    }

    private void DepositResources()
    {
        if (!_data.IsCarryingResources())
            return;

        // Deposit the carried resource
        if (_data.CarriedResource.HasValue)
        {
            _resourceManager.AddResource(_data.CarriedResource.Value, _data.CarriedAmount);
            GD.Print($"Worker: Deposited {_data.CarriedAmount} {_data.CarriedResource.Value} to stockpile");
        }

        _data.DropResource();
    }

    private void CheckNavigationReady()
    {
        if (_navAgent != null && _navAgent.IsInsideTree())
        {
            _navigationReady = true;
            GD.Print("Worker: Navigation ready");
        }
        else
        {
            // Try again next frame
            CallDeferred(MethodName.CheckNavigationReady);
        }
    }

    private void SetDestination(Vector2 targetPosition)
    {
        if (_navAgent != null && _navigationReady)
        {
            _navAgent.TargetPosition = targetPosition;
            GD.Print($"Worker: Set destination to {targetPosition}");
        }
        else
        {
            GD.PrintErr($"Worker: Cannot set destination - navigation not ready (navAgent: {_navAgent != null}, ready: {_navigationReady})");
        }
    }

    private void SetState(WorkerState newState)
    {
        if (_state == newState)
            return;

        _state = newState;
        _data.State = newState;

        EmitSignal(SignalName.WorkerStateChanged, this, (int)newState);
        GD.Print($"Worker: State changed to {newState}");
    }

    public void SetJob(JobType newJob)
    {
        var oldJob = _data.Job;
        _data.Job = newJob;

        // Cancel current task if it's not compatible with new job
        if (_currentTask != null && !_currentTask.CanBePerformedBy(newJob))
        {
            if (_currentTask is GatherTask gatherTask)
            {
                gatherTask.ReleaseResourceReservation(this);
            }
            _currentTask.Cancel();
            _currentTask = null;
            SetState(WorkerState.Idle);
        }

        // Update sprite color based on job
        UpdateSpriteColor();

        EmitSignal(SignalName.WorkerJobChanged, this, (int)newJob);
        GD.Print($"Worker: Job changed from {oldJob} to {newJob}");
    }

    private void UpdateSpriteColor()
    {
        if (_sprite == null)
            return;

        // Color-code workers by job
        _sprite.Color = _data.Job switch
        {
            JobType.Lumberjack => new Color(0.13f, 0.55f, 0.13f), // Green (wood)
            JobType.Miner => new Color(0.6f, 0.4f, 0.2f), // Brown/bronze (distinct from gray stone)
            JobType.Forager => new Color(1.0f, 0.84f, 0.0f), // Yellow (food)
            JobType.Builder => new Color(1.0f, 0.5f, 0.0f), // Orange (distinct from red buildings)
            _ => new Color(0.3f, 0.6f, 1.0f) // Blue (default/idle)
        };
    }

    public void SetStockpile(Stockpile stockpile)
    {
        _stockpile = stockpile;
    }

    public void Select()
    {
        _isSelected = true;
        if (_selectionIndicator != null)
        {
            _selectionIndicator.Visible = true;
        }
        EmitSignal(SignalName.WorkerSelected, this);
    }

    public void Deselect()
    {
        _isSelected = false;
        if (_selectionIndicator != null)
        {
            _selectionIndicator.Visible = false;
        }
        EmitSignal(SignalName.WorkerDeselected, this);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            // Check if click is on this worker
            var mousePos = GetGlobalMousePosition();
            var distance = GlobalPosition.DistanceTo(mousePos);

            if (distance <= GameConfig.WORKER_SPRITE_SIZE)
            {
                var workerManager = GetNodeOrNull<WorkerManager>("/root/WorkerManager");
                if (workerManager != null)
                {
                    // Left click: select worker
                    if (mouseEvent.ButtonIndex == MouseButton.Left)
                    {
                        GD.Print($"Worker: Left-click detected, selecting worker");
                        workerManager.SelectWorker(this);
                        GetViewport().SetInputAsHandled();
                    }
                    // Right click: deselect worker if this worker is selected
                    else if (mouseEvent.ButtonIndex == MouseButton.Right)
                    {
                        GD.Print($"Worker: Right-click detected, checking if selected");
                        if (workerManager.SelectedWorker == this)
                        {
                            GD.Print($"Worker: Deselecting worker");
                            workerManager.DeselectWorker();
                            GetViewport().SetInputAsHandled();
                        }
                    }
                }
            }
        }
    }
}
