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
        GD.Print("[ConstructionQueueUI] _Ready called - starting initialization");

        try
        {
            // Get BuildingManager reference
            _buildingManager = GetNode<BuildingManager>("/root/BuildingManager");
            if (_buildingManager == null)
            {
                GD.PushError("[ConstructionQueueUI] BuildingManager not found!");
                return;
            }
            GD.Print("[ConstructionQueueUI] BuildingManager found");

            // Get the popup menu
            _popup = GetPopup();
            if (_popup == null)
            {
                GD.PushError("[ConstructionQueueUI] GetPopup() returned null - MenuButton may not be properly initialized");
                return;
            }
            GD.Print("[ConstructionQueueUI] Popup menu obtained");

            // Subscribe to construction signals
            _buildingManager.ConstructionStarted += OnConstructionStarted;
            _buildingManager.ConstructionCompleted += OnConstructionCompleted;
            GD.Print("[ConstructionQueueUI] Subscribed to construction signals");

            // Create update timer for progress
            _updateTimer = new Timer();
            _updateTimer.WaitTime = UPDATE_INTERVAL;
            _updateTimer.Autostart = true;
            _updateTimer.Timeout += UpdateDisplay;
            AddChild(_updateTimer);
            GD.Print("[ConstructionQueueUI] Update timer created");

            // Initial update
            UpdateDisplay();

            GD.Print("[ConstructionQueueUI] Initialized successfully!");
        }
        catch (System.Exception ex)
        {
            GD.PushError($"[ConstructionQueueUI] Exception during initialization: {ex.Message}");
            GD.PushError($"[ConstructionQueueUI] Stack trace: {ex.StackTrace}");
        }
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

        // Always visible
        Visible = true;

        // Update button text to show count
        Text = $"🏗️ Construction ({sites.Count})";

        // Clear existing popup items
        _popup.Clear();

        if (sites.Count == 0)
        {
            // Show empty state in dropdown
            _popup.AddItem("No buildings under construction", 0);
            _popup.SetItemDisabled(0, true);
            GD.Print("[ConstructionQueueUI] No construction sites");
        }
        else
        {
            GD.Print($"[ConstructionQueueUI] Showing {sites.Count} construction sites");

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
}
