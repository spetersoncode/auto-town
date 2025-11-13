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
            // Configure navigation agent - simple settings, rely on physics for collision
            _navAgent.MaxSpeed = GameConfig.NAV_AGENT_MAX_SPEED;
            _navAgent.PathDesiredDistance = GameConfig.NAV_AGENT_PATH_DESIRED_DISTANCE;
            _navAgent.TargetDesiredDistance = GameConfig.NAV_AGENT_TARGET_DESIRED_DISTANCE;
            _navAgent.Radius = GameConfig.NAV_AGENT_RADIUS;

            // Disable avoidance - let physics handle obstacles
            _navAgent.AvoidanceEnabled = false;

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

        // Collision setup
        var collisionShape = new CollisionShape2D();
        var circle = new CircleShape2D { Radius = GameConfig.NAV_AGENT_RADIUS };
        collisionShape.Shape = circle;
        AddChild(collisionShape);

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

            // Debug log occasionally (every 120 frames = ~2 seconds at 60 FPS)
            if (Engine.GetProcessFrames() % 120 == 0)
            {
                GD.Print($"Worker: Waiting for harvest, resource state: {resourceState}");
            }

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
                // Harvest completed! Pick up resources
                GD.Print($"Worker: Harvest completed! Resource state is now Available. Picking up resources.");
                _data.PickupResource(gatherTask.ResourceType, gatherTask.ExpectedYield);
                GD.Print($"Worker: Picked up {gatherTask.ExpectedYield} {gatherTask.ResourceType}");

                // DON'T complete task yet - wait until we deposit at stockpile
                // Now haul to stockpile
                SetState(WorkerState.Hauling);
                SetDestination(gatherTask.StockpilePosition);
            }

            // Otherwise, still BeingHarvested - just wait
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
            GD.Print($"Worker: Assigned {task.Type} task at {task.Position}");

            // Start moving to task location
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
        }
        else if (_state == WorkerState.Hauling && _currentTask != null)
        {
            // Reached the stockpile
            DepositResources();

            // Complete the task
            _currentTask.Complete();
            _currentTask = null;

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
            JobType.Miner => new Color(0.5f, 0.5f, 0.5f), // Gray (stone)
            JobType.Forager => new Color(1.0f, 0.84f, 0.0f), // Yellow (food)
            JobType.Builder => new Color(0.8f, 0.2f, 0.2f), // Red (building)
            _ => new Color(0.3f, 0.6f, 1.0f) // Blue (default)
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
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                // Check if click is on this worker
                var mousePos = GetGlobalMousePosition();
                var distance = GlobalPosition.DistanceTo(mousePos);

                if (distance <= GameConfig.WORKER_SPRITE_SIZE)
                {
                    // Notify WorkerManager of selection
                    var workerManager = GetNodeOrNull<WorkerManager>("/root/WorkerManager");
                    if (workerManager != null)
                    {
                        workerManager.SelectWorker(this);
                    }
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }
}
