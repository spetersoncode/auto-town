using Godot;
using autotown.Core;
using autotown.Data;
using autotown.Systems;

namespace autotown.Entities;

/// <summary>
/// Farm building that produces food resources.
/// Periodically generates ProcessTasks for Farmers to complete.
/// </summary>
public partial class Farm : Building
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

    protected override void OnReady()
    {
        Type = BuildingType.Farm;
        _taskManager = GetNode<TaskManager>("/root/TaskManager");

        GD.Print($"[Farm] Initialized - will produce {GameConfig.FARM_FOOD_OUTPUT} Food every {GameConfig.PROCESSING_TASK_INTERVAL}s");
    }

    protected override void OnActivated()
    {
        GD.Print($"[Farm] Activated - now generating food production tasks");
        _taskGenerationTimer = 0f; // Reset timer when activated
    }

    protected override void OnConstructionCompleted()
    {
        GD.Print($"[Farm] Construction completed - ready to produce food");
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
    /// Generates a new food production task.
    /// </summary>
    private void GenerateProcessingTask()
    {
        if (_taskManager == null)
        {
            GD.PushError("[Farm] TaskManager not found");
            return;
        }

        var processTask = new ProcessTask(
            this,
            ResourceType.Food,
            GameConfig.FARM_FOOD_OUTPUT,
            GameConfig.FARM_PROCESS_TIME,
            new[] { JobType.Farmer }
        );

        // Subscribe to task completion to track active tasks
        processTask.TaskCompleted += OnProcessTaskCompleted;
        processTask.TaskCancelled += OnProcessTaskCancelled;

        _hasActiveTask = true;
        _taskManager.AddTask(processTask);

        GD.Print($"[Farm] Generated processing task for {GameConfig.FARM_FOOD_OUTPUT} Food");
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
