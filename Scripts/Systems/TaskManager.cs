using Godot;
using System.Collections.Generic;
using System.Linq;
using autotown.Core;
using autotown.Data;

namespace autotown.Systems;

/// <summary>
/// Manages the global task queue for all workers.
/// Tasks are automatically assigned to idle workers based on job type and proximity.
/// </summary>
public partial class TaskManager : Node
{
    private List<Task> _tasks = new List<Task>();

    [Signal]
    public delegate void TaskAddedEventHandler(Task task);

    [Signal]
    public delegate void TaskRemovedEventHandler(Task task);

    [Signal]
    public delegate void TaskCompletedEventHandler(Task task);

    [Signal]
    public delegate void TaskCancelledEventHandler(Task task);

    public override void _Ready()
    {
        GD.Print("TaskManager: Initialized");
    }

    /// <summary>
    /// Adds a task to the queue.
    /// </summary>
    public void AddTask(Task task)
    {
        if (task == null)
        {
            GD.PrintErr("TaskManager: Cannot add null task");
            return;
        }

        if (_tasks.Count >= GameConfig.MAX_TASK_QUEUE_SIZE)
        {
            GD.PrintErr($"TaskManager: Task queue is full ({GameConfig.MAX_TASK_QUEUE_SIZE} tasks). Ignoring new task.");
            return;
        }

        _tasks.Add(task);

        // Subscribe to task state changes
        task.TaskCompleted += OnTaskCompleted;
        task.TaskCancelled += OnTaskCancelled;

        EmitSignal(SignalName.TaskAdded, task);
        GD.Print($"TaskManager: Added {task.Type} task at {task.Position} (Total tasks: {_tasks.Count})");
    }

    /// <summary>
    /// Removes a task from the queue.
    /// </summary>
    public void RemoveTask(Task task)
    {
        if (task == null || !_tasks.Contains(task))
            return;

        // Unsubscribe from task events
        task.TaskCompleted -= OnTaskCompleted;
        task.TaskCancelled -= OnTaskCancelled;

        _tasks.Remove(task);
        EmitSignal(SignalName.TaskRemoved, task);

        GD.Print($"TaskManager: Removed {task.Type} task (Total tasks: {_tasks.Count})");
    }

    /// <summary>
    /// Gets all pending tasks (not assigned to anyone).
    /// </summary>
    public List<Task> GetPendingTasks()
    {
        return _tasks.Where(t => t.State == TaskState.Pending).ToList();
    }

    /// <summary>
    /// Gets all in-progress tasks.
    /// </summary>
    public List<Task> GetInProgressTasks()
    {
        return _tasks.Where(t => t.State == TaskState.InProgress).ToList();
    }

    /// <summary>
    /// Finds the best available task for a worker with the given job type and position.
    /// Returns null if no suitable task is found.
    /// </summary>
    public Task FindBestTaskFor(JobType jobType, Vector2 workerPosition)
    {
        var pendingTasks = GetPendingTasks();

        // Filter by job type
        var validTasks = pendingTasks.Where(t => t.CanBePerformedBy(jobType)).ToList();

        if (validTasks.Count == 0)
            return null;

        // Filter by distance if max distance is set
        if (GameConfig.MAX_TASK_DISTANCE > 0)
        {
            validTasks = validTasks
                .Where(t => workerPosition.DistanceTo(t.Position) <= GameConfig.MAX_TASK_DISTANCE)
                .ToList();
        }

        if (validTasks.Count == 0)
            return null;

        // Additional validation for GatherTasks
        var validGatherTasks = validTasks
            .Where(t =>
            {
                if (t is GatherTask gatherTask)
                {
                    return gatherTask.IsValid();
                }
                return true;
            })
            .ToList();

        if (validGatherTasks.Count == 0)
            return null;

        // Sort by priority (higher first), then by distance (closer first)
        var bestTask = validGatherTasks
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => workerPosition.DistanceTo(t.Position))
            .FirstOrDefault();

        return bestTask;
    }

    /// <summary>
    /// Gets all tasks of a specific type.
    /// </summary>
    public List<Task> GetTasksByType(TaskType type)
    {
        return _tasks.Where(t => t.Type == type).ToList();
    }

    /// <summary>
    /// Gets all tasks for a specific job type.
    /// </summary>
    public List<Task> GetTasksForJob(JobType jobType)
    {
        return _tasks.Where(t => t.CanBePerformedBy(jobType)).ToList();
    }

    /// <summary>
    /// Cancels a task by ID.
    /// </summary>
    public void CancelTask(string taskId)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            task.Cancel();
        }
    }

    /// <summary>
    /// Cancels all tasks related to a specific resource node.
    /// Useful when a resource is depleted.
    /// </summary>
    public void CancelTasksForResource(GodotObject resourceNode)
    {
        var tasksToCancel = _tasks
            .OfType<GatherTask>()
            .Where(t => t.ResourceNode == resourceNode)
            .ToList();

        foreach (var task in tasksToCancel)
        {
            task.Cancel();
        }
    }

    /// <summary>
    /// Clears all completed and cancelled tasks.
    /// Called periodically to prevent memory bloat.
    /// </summary>
    public void CleanupFinishedTasks()
    {
        var finishedTasks = _tasks
            .Where(t => t.State == TaskState.Completed || t.State == TaskState.Cancelled)
            .ToList();

        foreach (var task in finishedTasks)
        {
            RemoveTask(task);
        }

        if (finishedTasks.Count > 0)
        {
            GD.Print($"TaskManager: Cleaned up {finishedTasks.Count} finished tasks");
        }
    }

    /// <summary>
    /// Gets the total number of tasks in the queue.
    /// </summary>
    public int GetTaskCount()
    {
        return _tasks.Count;
    }

    /// <summary>
    /// Gets counts by state for debugging/UI.
    /// </summary>
    public (int pending, int inProgress, int completed, int cancelled) GetTaskCounts()
    {
        var pending = _tasks.Count(t => t.State == TaskState.Pending);
        var inProgress = _tasks.Count(t => t.State == TaskState.InProgress);
        var completed = _tasks.Count(t => t.State == TaskState.Completed);
        var cancelled = _tasks.Count(t => t.State == TaskState.Cancelled);

        return (pending, inProgress, completed, cancelled);
    }

    private void OnTaskCompleted(Task task)
    {
        EmitSignal(SignalName.TaskCompleted, task);
        // Don't remove immediately - let cleanup handle it
    }

    private void OnTaskCancelled(Task task)
    {
        EmitSignal(SignalName.TaskCancelled, task);
        // Don't remove immediately - let cleanup handle it
    }

    public override void _Process(double delta)
    {
        // Periodically clean up finished tasks (every 5 seconds)
        if (Engine.GetProcessFrames() % 300 == 0)
        {
            CleanupFinishedTasks();
        }
    }
}
