using Godot;
using System.Collections.Generic;
using System.Linq;
using autotown.Data;
using autotown.Entities;

namespace autotown.Systems;

/// <summary>
/// Manages all workers in the game.
/// Handles spawning, tracking, and selection.
/// </summary>
public partial class WorkerManager : Node
{
    private List<Worker> _workers = new List<Worker>();
    private Worker _selectedWorker;
    private PackedScene _workerScene;

    [Signal]
    public delegate void WorkerSpawnedEventHandler(Worker worker);

    [Signal]
    public delegate void WorkerRemovedEventHandler(Worker worker);

    [Signal]
    public delegate void WorkerSelectedEventHandler(Worker worker);

    [Signal]
    public delegate void WorkerJobChangedEventHandler(Worker worker, JobType newJob);

    public Worker SelectedWorker => _selectedWorker;
    public int WorkerCount => _workers.Count;

    public override void _Ready()
    {
        // Load worker scene
        _workerScene = GD.Load<PackedScene>("res://Scenes/Entities/Worker.tscn");
        if (_workerScene == null)
        {
            GD.PrintErr("WorkerManager: Failed to load Worker.tscn scene!");
        }

        GD.Print("WorkerManager: Initialized");
    }

    /// <summary>
    /// Spawns a new worker at the specified position with the given job.
    /// </summary>
    public Worker SpawnWorker(Vector2 position, JobType jobType, Node parentNode)
    {
        if (_workerScene == null)
        {
            GD.PrintErr("WorkerManager: Cannot spawn worker - scene not loaded");
            return null;
        }

        var worker = _workerScene.Instantiate<Worker>();
        if (worker == null)
        {
            GD.PrintErr("WorkerManager: Failed to instantiate worker");
            return null;
        }

        worker.GlobalPosition = position;
        parentNode.AddChild(worker);

        // Set job after adding to scene tree so signals work
        worker.SetJob(jobType);

        // Track worker
        _workers.Add(worker);

        // Subscribe to worker signals
        worker.WorkerJobChanged += OnWorkerJobChanged;

        EmitSignal(SignalName.WorkerSpawned, worker);
        GD.Print($"WorkerManager: Spawned {jobType} worker at {position} (Total: {_workers.Count})");

        return worker;
    }

    /// <summary>
    /// Removes a worker from tracking and the scene.
    /// </summary>
    public void RemoveWorker(Worker worker)
    {
        if (worker == null || !_workers.Contains(worker))
            return;

        // Unsubscribe from signals
        worker.WorkerJobChanged -= OnWorkerJobChanged;

        _workers.Remove(worker);

        if (_selectedWorker == worker)
        {
            _selectedWorker = null;
        }

        EmitSignal(SignalName.WorkerRemoved, worker);
        worker.QueueFree();

        GD.Print($"WorkerManager: Removed worker (Total: {_workers.Count})");
    }

    /// <summary>
    /// Selects a worker for UI interaction.
    /// </summary>
    public void SelectWorker(Worker worker)
    {
        if (worker == null)
            return;

        // Deselect previous worker
        if (_selectedWorker != null)
        {
            _selectedWorker.Deselect();
        }

        _selectedWorker = worker;
        _selectedWorker.Select();

        EmitSignal(SignalName.WorkerSelected, worker);
        GD.Print($"WorkerManager: Selected worker with job {worker.Data.Job}");
    }

    /// <summary>
    /// Deselects the currently selected worker.
    /// </summary>
    public void DeselectWorker()
    {
        if (_selectedWorker != null)
        {
            _selectedWorker.Deselect();
            _selectedWorker = null;

            // Emit signal with null to notify UI
            EmitSignal(SignalName.WorkerSelected, (Worker)null);
            GD.Print($"WorkerManager: Deselected worker");
        }
    }

    /// <summary>
    /// Gets all workers.
    /// </summary>
    public List<Worker> GetAllWorkers()
    {
        return new List<Worker>(_workers);
    }

    /// <summary>
    /// Gets all idle workers (not currently working).
    /// </summary>
    public List<Worker> GetIdleWorkers()
    {
        return _workers.Where(w => w.State == WorkerState.Idle).ToList();
    }

    /// <summary>
    /// Gets all workers with a specific job type.
    /// </summary>
    public List<Worker> GetWorkersByJob(JobType jobType)
    {
        return _workers.Where(w => w.Data.Job == jobType).ToList();
    }

    /// <summary>
    /// Gets count of workers by job type.
    /// </summary>
    public Dictionary<JobType, int> GetWorkerCountsByJob()
    {
        var counts = new Dictionary<JobType, int>();

        foreach (JobType job in System.Enum.GetValues(typeof(JobType)))
        {
            counts[job] = 0;
        }

        foreach (var worker in _workers)
        {
            counts[worker.Data.Job]++;
        }

        return counts;
    }

    /// <summary>
    /// Gets count of workers by state.
    /// </summary>
    public Dictionary<WorkerState, int> GetWorkerCountsByState()
    {
        var counts = new Dictionary<WorkerState, int>();

        foreach (WorkerState state in System.Enum.GetValues(typeof(WorkerState)))
        {
            counts[state] = 0;
        }

        foreach (var worker in _workers)
        {
            counts[worker.State]++;
        }

        return counts;
    }

    /// <summary>
    /// Changes the job of the currently selected worker.
    /// </summary>
    public void ChangeSelectedWorkerJob(JobType newJob)
    {
        if (_selectedWorker != null)
        {
            _selectedWorker.SetJob(newJob);
        }
    }

    private void OnWorkerJobChanged(Worker worker, JobType newJob)
    {
        EmitSignal(SignalName.WorkerJobChanged, worker, (int)newJob);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (_selectedWorker == null)
                return;

            // Job assignment hotkeys for selected worker
            JobType? newJob = keyEvent.Keycode switch
            {
                Key.Key1 => JobType.Lumberjack,
                Key.Key2 => JobType.Miner,
                Key.Key3 => JobType.Forager,
                Key.Key4 => JobType.Builder,
                Key.Key0 => JobType.None,
                _ => null
            };

            if (newJob.HasValue)
            {
                ChangeSelectedWorkerJob(newJob.Value);
                GetViewport().SetInputAsHandled();
            }
        }
    }
}
