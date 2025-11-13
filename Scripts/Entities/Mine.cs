using Godot;
using autotown.Core;
using autotown.Data;
using autotown.Systems;

namespace autotown.Entities;

/// <summary>
/// Mine building that extracts stone resources.
/// Periodically generates ProcessTasks for Miners to complete.
/// </summary>
public partial class Mine : Building
{
    /// <summary>
    /// Timer for generating processing tasks.
    /// </summary>
    private float _taskGenerationTimer = 0f;

    /// <summary>
    /// Whether a processing task is currently pending or in progress.
    /// Prevents generating multiple tasks at once.
    /// </summary>
    private bool _hasActiveTask = false;

    /// <summary>
    /// Reference to the task manager.
    /// </summary>
    private TaskManager _taskManager;

    /// <summary>
    /// Building resource containing production parameters.
    /// </summary>
    private BuildingResource _resource;

    protected override void OnReady()
    {
        Type = BuildingType.Mine;
        _taskManager = GetNode<TaskManager>("/root/TaskManager");
        _resource = BuildingDefinitions.GetBuildingResource(Type);

        GD.Print($"[Mine] Initialized - will produce {_resource.ProductionOutput} Stone every {GameConfig.PROCESSING_TASK_INTERVAL}s");
    }

    protected override void OnActivated()
    {
        GD.Print($"[Mine] Activated - now generating stone extraction tasks");
        _taskGenerationTimer = 0f; // Reset timer when activated
    }

    protected override void OnConstructionCompleted()
    {
        GD.Print($"[Mine] Construction completed - ready to extract stone");
    }

    protected override void OnActiveProcess(double delta)
    {
        // Increment timer
        _taskGenerationTimer += (float)delta;

        // Check if it's time to generate a new task
        if (_taskGenerationTimer >= GameConfig.PROCESSING_TASK_INTERVAL && !_hasActiveTask)
        {
            GenerateProcessingTask();
            _taskGenerationTimer = 0f;
        }
    }

    /// <summary>
    /// Generates a new stone extraction task.
    /// </summary>
    private void GenerateProcessingTask()
    {
        if (_taskManager == null)
        {
            GD.PushError("[Mine] TaskManager not found");
            return;
        }

        var processTask = new ProcessTask(
            this,
            ResourceType.Stone,
            _resource.ProductionOutput,
            _resource.ProductionTime,
            new[] { JobType.Miner }
        );

        // Subscribe to task completion to track active tasks
        processTask.TaskCompleted += OnProcessTaskCompleted;
        processTask.TaskCancelled += OnProcessTaskCancelled;

        _hasActiveTask = true;
        _taskManager.AddTask(processTask);

        GD.Print($"[Mine] Generated processing task for {_resource.ProductionOutput} Stone");
    }

    /// <summary>
    /// Called when a processing task is completed.
    /// </summary>
    private void OnProcessTaskCompleted(Task task)
    {
        _hasActiveTask = false;

        // Unsubscribe from events
        if (task != null)
        {
            task.TaskCompleted -= OnProcessTaskCompleted;
            task.TaskCancelled -= OnProcessTaskCancelled;
        }
    }

    /// <summary>
    /// Called when a processing task is cancelled.
    /// </summary>
    private void OnProcessTaskCancelled(Task task)
    {
        _hasActiveTask = false;

        // Unsubscribe from events
        if (task != null)
        {
            task.TaskCompleted -= OnProcessTaskCompleted;
            task.TaskCancelled -= OnProcessTaskCancelled;
        }
    }
}
