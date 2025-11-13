using Godot;
using autotown.Core;

namespace autotown.UI;

/// <summary>
/// Main UI controller that manages all UI elements and responds to game state changes.
/// </summary>
public partial class GameUI : CanvasLayer
{
    private Label _gameSpeedLabel;
    private Button _helpButton;
    private HelpOverlayUI _helpOverlay;
    private GameManager _gameManager;

    public override void _Ready()
    {
        // Get references to UI elements
        _gameSpeedLabel = GetNode<Label>("MarginContainer/VBoxContainer/TopBarPanel/TopBarMargin/TopBar/GameSpeedLabel");
        _helpButton = GetNode<Button>("MarginContainer/VBoxContainer/TopBarPanel/TopBarMargin/TopBar/HelpButton");
        _helpOverlay = GetNode<HelpOverlayUI>("HelpOverlay");

        // Get GameManager singleton
        _gameManager = GetNode<GameManager>("/root/GameManager");

        // Subscribe to signals
        _gameManager.GameSpeedChanged += OnGameSpeedChanged;
        _helpButton.Pressed += OnHelpButtonPressed;

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

        if (_helpButton != null)
        {
            _helpButton.Pressed -= OnHelpButtonPressed;
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

    /// <summary>
    /// Called when the help button is pressed.
    /// </summary>
    private void OnHelpButtonPressed()
    {
        _helpOverlay.Toggle();
    }
}
