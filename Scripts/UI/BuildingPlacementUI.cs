using Godot;
using autotown.Core;
using autotown.Data;
using autotown.Systems;

namespace autotown.UI;

/// <summary>
/// Handles building placement via keyboard shortcuts.
/// Press function keys (F1-F4) to place buildings at mouse cursor:
/// F1 = House, F2 = Sawmill, F3 = Mine, F4 = Farm
/// </summary>
public partial class BuildingPlacementUI : Node
{
    private BuildingManager _buildingManager;
    private Camera2D _camera;
    private Node _buildingsContainer;

    // Scene paths for buildings
    private const string HOUSE_SCENE_PATH = "res://Scenes/Entities/House.tscn";
    private const string SAWMILL_SCENE_PATH = "res://Scenes/Entities/Sawmill.tscn";
    private const string MINE_SCENE_PATH = "res://Scenes/Entities/Mine.tscn";
    private const string FARM_SCENE_PATH = "res://Scenes/Entities/Farm.tscn";

    public override void _Ready()
    {
        _buildingManager = GetNode<BuildingManager>("/root/BuildingManager");
        LogManager.Log(LogManager.DEBUG_PLACEMENT_UI, "[BuildingPlacementUI] Initialized - Press F1-F4 to place buildings at mouse cursor");
        LogManager.Log(LogManager.DEBUG_PLACEMENT_UI, "[BuildingPlacementUI] F1=House, F2=Sawmill, F3=Mine, F4=Farm");
    }

    /// <summary>
    /// Sets the camera reference for converting screen to world coordinates.
    /// </summary>
    public void SetCamera(Camera2D camera)
    {
        _camera = camera;
    }

    /// <summary>
    /// Sets the buildings container where construction sites will be added.
    /// </summary>
    public void SetBuildingsContainer(Node container)
    {
        _buildingsContainer = container;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            BuildingType? buildingType = null;
            string scenePath = null;

            // Map keys to building types
            switch (keyEvent.Keycode)
            {
                case Key.F1:
                    buildingType = BuildingType.House;
                    scenePath = HOUSE_SCENE_PATH;
                    break;
                case Key.F2:
                    buildingType = BuildingType.Sawmill;
                    scenePath = SAWMILL_SCENE_PATH;
                    break;
                case Key.F3:
                    buildingType = BuildingType.Mine;
                    scenePath = MINE_SCENE_PATH;
                    break;
                case Key.F4:
                    buildingType = BuildingType.Farm;
                    scenePath = FARM_SCENE_PATH;
                    break;
            }

            if (buildingType.HasValue && scenePath != null)
            {
                PlaceBuilding(buildingType.Value, scenePath);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    /// <summary>
    /// Places a building at the mouse cursor position.
    /// </summary>
    private void PlaceBuilding(BuildingType buildingType, string scenePath)
    {
        if (_buildingsContainer == null)
        {
            LogManager.Error("[BuildingPlacementUI] Buildings container not set");
            return;
        }

        if (_camera == null)
        {
            LogManager.Error("[BuildingPlacementUI] Camera not set");
            return;
        }

        // Get mouse position in world coordinates
        var mousePos = GetViewport().GetMousePosition();
        var worldPos = _camera.GetScreenCenterPosition() + (mousePos - GetViewport().GetVisibleRect().Size / 2) / _camera.Zoom;

        // Get building data to check affordability
        var buildingData = BuildingDefinitions.GetBuildingData(buildingType);

        // Check if we can afford it
        if (!_buildingManager.CanAfford(buildingType))
        {
            int woodCost = buildingData.Cost.ContainsKey(ResourceType.Wood) ? buildingData.Cost[ResourceType.Wood] : 0;
            int stoneCost = buildingData.Cost.ContainsKey(ResourceType.Stone) ? buildingData.Cost[ResourceType.Stone] : 0;
            GD.Print($"[BuildingPlacementUI] Cannot afford {buildingData.Name}");
            GD.Print($"[BuildingPlacementUI] Costs: Wood={woodCost}, Stone={stoneCost}");
            return;
        }

        // Place the building
        var site = _buildingManager.PlaceBuilding(buildingType, worldPos, _buildingsContainer, scenePath);

        if (site != null)
        {
            GD.Print($"[BuildingPlacementUI] Placed {buildingData.Name} at {worldPos}");
        }
        else
        {
            GD.Print($"[BuildingPlacementUI] Failed to place {buildingData.Name}");
        }
    }
}
