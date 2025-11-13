using Godot;
using autotown.Core;

namespace autotown.Core;

/// <summary>
/// Controls camera movement including panning, zooming, and edge scrolling.
/// Attach this script to a Camera2D node in the world.
/// </summary>
public partial class CameraController : Camera2D
{
    /// <summary>
    /// Camera movement speed in pixels per second.
    /// </summary>
    [Export]
    public float MoveSpeed { get; set; } = 300f;

    /// <summary>
    /// Minimum zoom level (zoomed out).
    /// </summary>
    [Export]
    public float ZoomMin { get; set; } = 0.5f;

    /// <summary>
    /// Maximum zoom level (zoomed in).
    /// </summary>
    [Export]
    public float ZoomMax { get; set; } = 2.0f;

    /// <summary>
    /// Zoom step when using mouse wheel.
    /// </summary>
    [Export]
    public float ZoomStep { get; set; } = 0.1f;

    /// <summary>
    /// Enable or disable edge scrolling.
    /// </summary>
    [Export]
    public bool EdgeScrollingEnabled { get; set; } = true;

    /// <summary>
    /// Distance from screen edge in pixels to trigger edge scrolling.
    /// </summary>
    [Export]
    public float EdgeScrollThreshold { get; set; } = 20f;

    /// <summary>
    /// Edge scrolling speed multiplier.
    /// </summary>
    [Export]
    public float EdgeScrollSpeed { get; set; } = 1.0f;

    private Vector2 _mapMinBounds;
    private Vector2 _mapMaxBounds;
    private bool _boundsSet = false;

    public override void _Ready()
    {
        // Set default zoom
        Zoom = new Vector2(1.0f, 1.0f);

        GD.Print("CameraController: Camera initialized");
    }

    public override void _Process(double delta)
    {
        HandleKeyboardPanning((float)delta);
        HandleEdgeScrolling((float)delta);
        ClampToBounds();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            HandleMouseZoom(mouseButton);
        }
    }

    /// <summary>
    /// Handles keyboard-based camera panning (WASD and arrow keys).
    /// </summary>
    private void HandleKeyboardPanning(float delta)
    {
        Vector2 moveDirection = Vector2.Zero;

        // WASD keys
        if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up))
            moveDirection.Y -= 1;
        if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down))
            moveDirection.Y += 1;
        if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left))
            moveDirection.X -= 1;
        if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right))
            moveDirection.X += 1;

        if (moveDirection.Length() > 0)
        {
            moveDirection = moveDirection.Normalized();
            Position += moveDirection * MoveSpeed * delta;
        }
    }

    /// <summary>
    /// Handles edge scrolling when mouse is near screen edges.
    /// </summary>
    private void HandleEdgeScrolling(float delta)
    {
        if (!EdgeScrollingEnabled)
            return;

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        Vector2 scrollDirection = Vector2.Zero;

        // Check edges
        if (mousePos.X < EdgeScrollThreshold)
            scrollDirection.X -= 1;
        else if (mousePos.X > viewportSize.X - EdgeScrollThreshold)
            scrollDirection.X += 1;

        if (mousePos.Y < EdgeScrollThreshold)
            scrollDirection.Y -= 1;
        else if (mousePos.Y > viewportSize.Y - EdgeScrollThreshold)
            scrollDirection.Y += 1;

        if (scrollDirection.Length() > 0)
        {
            scrollDirection = scrollDirection.Normalized();
            Position += scrollDirection * MoveSpeed * EdgeScrollSpeed * delta;
        }
    }

    /// <summary>
    /// Handles mouse wheel zooming.
    /// </summary>
    private void HandleMouseZoom(InputEventMouseButton mouseButton)
    {
        if (mouseButton.ButtonIndex == MouseButton.WheelUp)
        {
            // Zoom in
            float newZoom = Mathf.Clamp(Zoom.X + ZoomStep, ZoomMin, ZoomMax);
            Zoom = new Vector2(newZoom, newZoom);
        }
        else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
        {
            // Zoom out
            float newZoom = Mathf.Clamp(Zoom.X - ZoomStep, ZoomMin, ZoomMax);
            Zoom = new Vector2(newZoom, newZoom);
        }
    }

    /// <summary>
    /// Sets the camera boundaries based on map dimensions.
    /// </summary>
    /// <param name="mapWidth">Map width in tiles</param>
    /// <param name="mapHeight">Map height in tiles</param>
    public void SetBounds(int mapWidth, int mapHeight)
    {
        const int boundaryPaddingTiles = 2;
        int padding = boundaryPaddingTiles * GameConfig.TILE_SIZE;

        _mapMinBounds = new Vector2(padding, padding);
        _mapMaxBounds = new Vector2(
            mapWidth * GameConfig.TILE_SIZE - padding,
            mapHeight * GameConfig.TILE_SIZE - padding
        );

        _boundsSet = true;
        GD.Print($"CameraController: Bounds set to {_mapMinBounds} - {_mapMaxBounds}");
    }

    /// <summary>
    /// Clamps camera position to map boundaries.
    /// </summary>
    private void ClampToBounds()
    {
        if (!_boundsSet)
            return;

        Position = new Vector2(
            Mathf.Clamp(Position.X, _mapMinBounds.X, _mapMaxBounds.X),
            Mathf.Clamp(Position.Y, _mapMinBounds.Y, _mapMaxBounds.Y)
        );
    }

    /// <summary>
    /// Centers the camera on a specific world position.
    /// </summary>
    /// <param name="worldPosition">World position to center on</param>
    public void CenterOn(Vector2 worldPosition)
    {
        Position = worldPosition;
        ClampToBounds();
        GD.Print($"CameraController: Centered on {worldPosition}");
    }

    /// <summary>
    /// Smoothly moves the camera to a target position.
    /// </summary>
    /// <param name="targetPosition">Target world position</param>
    /// <param name="duration">Duration of the transition in seconds</param>
    public async void SmoothMoveTo(Vector2 targetPosition, float duration = 0.5f)
    {
        Vector2 startPosition = Position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += (float)GetProcessDeltaTime();
            float t = Mathf.Clamp(elapsed / duration, 0f, 1f);

            // Ease-in-out interpolation
            t = t * t * (3f - 2f * t);

            Position = startPosition.Lerp(targetPosition, t);
            ClampToBounds();

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        Position = targetPosition;
        ClampToBounds();
    }
}
