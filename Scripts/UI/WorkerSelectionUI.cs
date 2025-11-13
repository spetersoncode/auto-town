using Godot;
using autotown.Data;
using autotown.Entities;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI panel that displays information about the currently selected worker
/// and allows job assignment via keyboard shortcuts.
/// </summary>
public partial class WorkerSelectionUI : PanelContainer
{
    private Label _workerInfoLabel;
    private Label _instructionsLabel;
    private WorkerManager _workerManager;
    private Worker _currentWorker;

    public override void _Ready()
    {
        // Get WorkerManager reference
        _workerManager = GetNode<WorkerManager>("/root/WorkerManager");

        // Get child labels
        _workerInfoLabel = GetNode<Label>("VBoxContainer/WorkerInfoLabel");
        _instructionsLabel = GetNode<Label>("VBoxContainer/InstructionsLabel");

        // Set instructions text
        _instructionsLabel.Text = "Click worker to select\n1=Lumberjack 2=Miner 3=Forager 4=Builder 0=Idle";

        // Subscribe to WorkerManager signals
        _workerManager.WorkerSelected += OnWorkerSelected;
        _workerManager.WorkerJobChanged += OnWorkerJobChanged;

        // Initial state
        UpdateUI();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (_workerManager != null)
        {
            _workerManager.WorkerSelected -= OnWorkerSelected;
            _workerManager.WorkerJobChanged -= OnWorkerJobChanged;
        }

        if (_currentWorker != null)
        {
            _currentWorker.WorkerStateChanged -= OnWorkerStateChanged;
        }
    }

    private void OnWorkerSelected(Worker worker)
    {
        // Unsubscribe from old worker
        if (_currentWorker != null)
        {
            _currentWorker.WorkerStateChanged -= OnWorkerStateChanged;
        }

        _currentWorker = worker;

        // Subscribe to new worker
        if (_currentWorker != null)
        {
            _currentWorker.WorkerStateChanged += OnWorkerStateChanged;
        }

        UpdateUI();
    }

    private void OnWorkerJobChanged(Worker worker, JobType newJob)
    {
        if (worker == _currentWorker)
        {
            UpdateUI();
        }
    }

    private void OnWorkerStateChanged(Worker worker, WorkerState newState)
    {
        if (worker == _currentWorker)
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (_currentWorker == null || !IsInstanceValid(_currentWorker))
        {
            _workerInfoLabel.Text = "No worker selected";
            return;
        }

        var data = _currentWorker.Data;
        var jobName = GetJobDisplayName(data.Job);
        var stateName = data.State.ToString();

        _workerInfoLabel.Text = $"Selected Worker\nJob: {jobName}\nState: {stateName}";
    }

    private string GetJobDisplayName(JobType job)
    {
        return job switch
        {
            JobType.None => "Idle",
            JobType.Lumberjack => "Lumberjack",
            JobType.Miner => "Miner",
            JobType.Forager => "Forager",
            JobType.Builder => "Builder",
            JobType.Farmer => "Farmer",
            _ => "Unknown"
        };
    }

    public override void _Process(double delta)
    {
        // Update UI periodically in case state changes without signals
        if (Engine.GetProcessFrames() % 30 == 0) // Every 0.5 seconds at 60fps
        {
            UpdateUI();
        }
    }
}
