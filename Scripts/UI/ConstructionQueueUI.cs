using Godot;
using System.Collections.Generic;
using System.Linq;
using autotown.Entities;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI component that displays active construction sites with progress percentages.
/// Shows format: 🏗️ House: 75%, Sawmill: 20%
/// </summary>
public partial class ConstructionQueueUI : Label
{
    private BuildingManager _buildingManager;
    private Timer _updateTimer;

    private const int MAX_DISPLAYED_SITES = 3;
    private const float UPDATE_INTERVAL = 0.5f; // Update twice per second

    public override void _Ready()
    {
        // Get BuildingManager reference
        _buildingManager = GetNode<BuildingManager>("/root/BuildingManager");

        // Subscribe to construction signals
        _buildingManager.ConstructionStarted += OnConstructionStarted;
        _buildingManager.ConstructionCompleted += OnConstructionCompleted;

        // Create update timer for progress
        _updateTimer = new Timer();
        _updateTimer.WaitTime = UPDATE_INTERVAL;
        _updateTimer.Autostart = true;
        _updateTimer.Timeout += UpdateDisplay;
        AddChild(_updateTimer);

        // Initial update
        UpdateDisplay();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (_buildingManager != null)
        {
            _buildingManager.ConstructionStarted -= OnConstructionStarted;
            _buildingManager.ConstructionCompleted -= OnConstructionCompleted;
        }
    }

    private void OnConstructionStarted(ConstructionSite site)
    {
        UpdateDisplay();
    }

    private void OnConstructionCompleted(Building building)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        var sites = _buildingManager.GetConstructionSites();

        if (sites.Count == 0)
        {
            Text = "";
            Visible = false;
            return;
        }

        Visible = true;

        // Build display string for up to MAX_DISPLAYED_SITES
        var displayParts = new List<string>();

        int displayCount = Mathf.Min(sites.Count, MAX_DISPLAYED_SITES);
        for (int i = 0; i < displayCount; i++)
        {
            var site = sites[i];
            int progressPercent = Mathf.RoundToInt(site.GetResourceProgress() * 100);
            displayParts.Add($"{site.Data.Name}: {progressPercent}%");
        }

        // If there are more sites than we can display, add indicator
        if (sites.Count > MAX_DISPLAYED_SITES)
        {
            int remaining = sites.Count - MAX_DISPLAYED_SITES;
            displayParts.Add($"+{remaining} more");
        }

        Text = $"🏗️ {string.Join(", ", displayParts)}";
    }
}
