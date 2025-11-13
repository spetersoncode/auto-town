using Godot;
using System;

namespace autotown.Data;

/// <summary>
/// Task state enum.
/// </summary>
public enum TaskState
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

/// <summary>
/// Base class for all tasks that workers can perform.
/// </summary>
public abstract partial class Task : GodotObject
{
    /// <summary>
    /// Unique identifier for this task.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// The type of task (Gather, Build, Process, Haul).
    /// </summary>
    public TaskType Type { get; protected set; }

    /// <summary>
    /// Current state of the task.
    /// </summary>
    public TaskState State { get; private set; } = TaskState.Pending;

    /// <summary>
    /// Task priority (higher = more important). Used for task assignment.
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// World position where the task takes place.
    /// </summary>
    public Vector2 Position { get; protected set; }

    /// <summary>
    /// Which job types can perform this task.
    /// </summary>
    public abstract JobType[] ValidJobTypes { get; }

    /// <summary>
    /// Estimated duration of the task in seconds.
    /// </summary>
    public abstract float EstimatedDuration { get; }

    /// <summary>
    /// Worker currently assigned to this task (null if unassigned).
    /// </summary>
    public GodotObject AssignedWorker { get; private set; }

    [Signal]
    public delegate void TaskStateChangedEventHandler(Task task, TaskState newState);

    [Signal]
    public delegate void TaskCompletedEventHandler(Task task);

    [Signal]
    public delegate void TaskCancelledEventHandler(Task task);

    public Task()
    {
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Checks if this task can be assigned to the given job type.
    /// </summary>
    public bool CanBePerformedBy(JobType jobType)
    {
        foreach (var validJob in ValidJobTypes)
        {
            if (validJob == jobType)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Assigns a worker to this task.
    /// </summary>
    public virtual bool TryAssign(GodotObject worker)
    {
        if (State != TaskState.Pending)
            return false;

        AssignedWorker = worker;
        SetState(TaskState.InProgress);
        return true;
    }

    /// <summary>
    /// Unassigns the worker from this task.
    /// </summary>
    public virtual void Unassign()
    {
        AssignedWorker = null;
        if (State == TaskState.InProgress)
        {
            SetState(TaskState.Pending);
        }
    }

    /// <summary>
    /// Marks the task as completed.
    /// </summary>
    public virtual void Complete()
    {
        SetState(TaskState.Completed);
        EmitSignal(SignalName.TaskCompleted, this);
    }

    /// <summary>
    /// Cancels the task.
    /// </summary>
    public virtual void Cancel()
    {
        SetState(TaskState.Cancelled);
        EmitSignal(SignalName.TaskCancelled, this);
    }

    /// <summary>
    /// Called when the task is started by a worker.
    /// </summary>
    public abstract void OnStart();

    /// <summary>
    /// Called when the task is being executed (per frame or update).
    /// </summary>
    public abstract void OnUpdate(double delta);

    /// <summary>
    /// Called when the task completes successfully.
    /// </summary>
    public abstract void OnComplete();

    /// <summary>
    /// Sets the task state and emits signals.
    /// </summary>
    private void SetState(TaskState newState)
    {
        if (State == newState)
            return;

        State = newState;
        EmitSignal(SignalName.TaskStateChanged, this, (int)newState);
    }
}
