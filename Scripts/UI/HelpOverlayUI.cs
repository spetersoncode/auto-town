using Godot;

namespace autotown.UI;

/// <summary>
/// Toggleable help overlay that displays game controls.
/// Can be shown/hidden via help button or by clicking outside the panel.
/// </summary>
public partial class HelpOverlayUI : Control
{
    private PanelContainer _helpPanel;
    private bool _isVisible = false;

    public override void _Ready()
    {
        // Start hidden
        Visible = false;

        // Get the help panel
        _helpPanel = GetNode<PanelContainer>("HelpPanel");

        // Set up background to capture clicks
        MouseFilter = MouseFilterEnum.Stop;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isVisible)
            return;

        // Close on any click (including outside the panel)
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            Hide();
            AcceptEvent(); // Consume the event
        }

        // Close on Escape key
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
            AcceptEvent();
        }
    }

    /// <summary>
    /// Toggle the help overlay visibility.
    /// </summary>
    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    /// <summary>
    /// Show the help overlay.
    /// </summary>
    public new void Show()
    {
        _isVisible = true;
        Visible = true;
    }

    /// <summary>
    /// Hide the help overlay.
    /// </summary>
    public new void Hide()
    {
        _isVisible = false;
        Visible = false;
    }
}
