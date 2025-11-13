using Godot;
using autotown.Core;
using autotown.Data;
using autotown.Systems;

namespace autotown.Entities;

/// <summary>
/// Sawmill building that processes wood resources.
/// Periodically generates ProcessTasks for Lumberjacks to complete.
/// </summary>
public partial class Sawmill : Building
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
        Type = BuildingType.Sawmill;
        _taskManager = GetNode<TaskManager>("/root/TaskManager");

        GD.Print($"[Sawmill] Initialized - will produce {GameConfig.SAWMILL_WOOD_OUTPUT} Wood every {GameConfig.PROCESSING_TASK_INTERVAL}s");
    }

    protected override void OnActivated()
    {
        GD.Print($"[Sawmill] Activated - now generating wood processing tasks");
        _taskGenerationTimer = 0f; // Reset timer when activated
    }

    protected override void OnConstructionCompleted()
    {
        GD.Print($"[Sawmill] Construction completed - ready to process wood");
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
    /// Generates a new wood processing task.
    /// </summary>
    private void GenerateProcessingTask()
    {
        if (_taskManager == null)
        {
            GD.PushError("[Sawmill] TaskManager not found");
            return;
        }

        var processTask = new ProcessTask(
            this,
            ResourceType.Wood,
            GameConfig.SAWMILL_WOOD_OUTPUT,
            GameConfig.SAWMILL_PROCESS_TIME,
            new[] { JobType.Lumberjack }
        );

        // Subscribe to task completion to track active tasks
        processTask.TaskCompleted += OnProcessTaskCompleted;
        processTask.TaskCancelled += OnProcessTaskCancelled;

        _hasActiveTask = true;
        _taskManager.AddTask(processTask);

        GD.Print($"[Sawmill] Generated processing task for {GameConfig.SAWMILL_WOOD_OUTPUT} Wood");
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
