using Godot;
using System.Collections.Generic;
using autotown.Data;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI component that displays worker counts by job type with color-coded icons.
/// Shows format: 🌲3 ⛏️2 🍓1 🔨1 💤2
/// </summary>
public partial class WorkerStatsUI : HBoxContainer
{
    private WorkerManager _workerManager;
    private Dictionary<JobType, Label> _jobLabels = new Dictionary<JobType, Label>();

    // Colors matching worker colors
    private readonly Dictionary<JobType, Color> _jobColors = new Dictionary<JobType, Color>
    {
        { JobType.Lumberjack, new Color(0.05f, 0.55f, 0.05f) }, // Green (darker for better readability on white bg)
        { JobType.Miner, new Color(0.5f, 0.5f, 0.5f) },          // Gray
        { JobType.Forager, new Color(1f, 0.84f, 0f) },           // Gold/Yellow
        { JobType.Builder, new Color(1f, 0.5f, 0f) },            // Orange
        { JobType.None, new Color(0.3f, 0.6f, 1f) },             // Blue (idle/no job)
        { JobType.Farmer, new Color(0.6f, 0.8f, 0.2f) }          // Light Green
    };

    // Icons for each job type
    private readonly Dictionary<JobType, string> _jobIcons = new Dictionary<JobType, string>
    {
        { JobType.Lumberjack, "🌲" },
        { JobType.Miner, "⛏️" },
        { JobType.Forager, "🍓" },
        { JobType.Builder, "🔨" },
        { JobType.None, "💤" },
        { JobType.Farmer, "🌾" }
    };

    public override void _Ready()
    {
        // Get WorkerManager reference
        _workerManager = GetNode<WorkerManager>("/root/WorkerManager");

        // Create labels for each job type
        CreateJobLabels();

        // Subscribe to worker signals
        _workerManager.WorkerJobChanged += OnWorkerJobChanged;
        _workerManager.WorkerSpawned += OnWorkerSpawned;
        _workerManager.WorkerRemoved += OnWorkerRemoved;

        // Initial update
        UpdateDisplay();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (_workerManager != null)
        {
            _workerManager.WorkerJobChanged -= OnWorkerJobChanged;
            _workerManager.WorkerSpawned -= OnWorkerSpawned;
            _workerManager.WorkerRemoved -= OnWorkerRemoved;
        }
    }

    private void CreateJobLabels()
    {
        // Add spacing control at start
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(10, 0);
        AddChild(spacer);

        // Create labels for each job type in display order
        JobType[] displayOrder = { JobType.Lumberjack, JobType.Miner, JobType.Forager, JobType.Builder, JobType.None };

        foreach (var jobType in displayOrder)
        {
            var label = new Label();
            label.Text = $"{_jobIcons[jobType]}0";
            label.Modulate = _jobColors[jobType];

            // Add some margin between labels
            if (_jobLabels.Count > 0)
            {
                var margin = new Control();
                margin.CustomMinimumSize = new Vector2(8, 0);
                AddChild(margin);
            }

            AddChild(label);
            _jobLabels[jobType] = label;
        }
    }

    private void OnWorkerJobChanged(Entities.Worker worker, JobType newJob)
    {
        UpdateDisplay();
    }

    private void OnWorkerSpawned(Entities.Worker worker)
    {
        UpdateDisplay();
    }

    private void OnWorkerRemoved(Entities.Worker worker)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var counts = _workerManager.GetWorkerCountsByJob();

        foreach (var kvp in _jobLabels)
        {
            JobType jobType = kvp.Key;
            Label label = kvp.Value;
            int count = counts.ContainsKey(jobType) ? counts[jobType] : 0;

            label.Text = $"{_jobIcons[jobType]}{count}";
        }
    }
}
