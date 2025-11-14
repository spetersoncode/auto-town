using Godot;
using System.Collections.Generic;
using System.Linq;
using autotown.Entities;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// UI component that displays active construction sites in a dropdown menu.
/// Shows button with count, clicking reveals all construction sites with progress.
/// </summary>
public partial class ConstructionQueueUI : MenuButton
{
    private BuildingManager _buildingManager;
    private Timer _updateTimer;
    private PopupMenu _popup;

    private const float UPDATE_INTERVAL = 0.5f; // Update twice per second

    public override void _Ready()
    {
        // Get BuildingManager reference
        _buildingManager = GetNode<BuildingManager>("/root/BuildingManager");

        // Get the popup menu
        _popup = GetPopup();

        // Release focus after popup is shown to prevent outline
        _popup.PopupHide += () => ReleaseFocus();
        Pressed += () => CallDeferred(MethodName.ReleaseFocus);

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

        // Update button text to show count
        Text = $"🏗️ Construction ({sites.Count})";

        // Clear existing popup items
        _popup.Clear();

        // Add all construction sites to the dropdown
        for (int i = 0; i < sites.Count; i++)
        {
            var site = sites[i];
            int progressPercent = Mathf.RoundToInt(site.GetResourceProgress() * 100);
            string itemText = $"{site.Data.Name}: {progressPercent}%";

            // Add item (disabled so it's display-only)
            _popup.AddItem(itemText, i);
            _popup.SetItemDisabled(i, true);
        }
    }
}
