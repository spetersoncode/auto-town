using Godot;
using autotown.Core;

namespace autotown.UI;

/// <summary>
/// Main UI controller that manages all UI elements and responds to game state changes.
/// </summary>
public partial class GameUI : CanvasLayer
{
    private Label _gameSpeedLabel;
    private GameManager _gameManager;

    public override void _Ready()
    {
        // Get references to UI elements
        _gameSpeedLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBar/GameSpeedLabel");

        // Get GameManager singleton
        _gameManager = GetNode<GameManager>("/root/GameManager");

        // Subscribe to signals
        _gameManager.GameSpeedChanged += OnGameSpeedChanged;

        // Initialize display
        UpdateGameSpeedDisplay(_gameManager.GameSpeed);

        GD.Print("GameUI: Initialized");
    }

    public override void _ExitTree()
    {
        // Unsubscribe from signals
        if (_gameManager != null)
        {
            _gameManager.GameSpeedChanged -= OnGameSpeedChanged;
        }
    }

    /// <summary>
    /// Called when the game speed changes.
    /// </summary>
    private void OnGameSpeedChanged(float newSpeed)
    {
        UpdateGameSpeedDisplay(newSpeed);
    }

    /// <summary>
    /// Updates the game speed label display.
    /// </summary>
    private void UpdateGameSpeedDisplay(float speed)
    {
        if (_gameSpeedLabel != null)
        {
            _gameSpeedLabel.Text = $"Speed: {speed:F1}x";
        }
    }
}
