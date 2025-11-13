namespace autotown.Core;

/// <summary>
/// Centralized game configuration constants.
/// Eliminates magic numbers and provides a single source of truth for game parameters.
/// </summary>
public static class GameConfig
{
    // === Worker Configuration ===

    /// <summary>
    /// Default movement speed for workers in pixels per second.
    /// </summary>
    public const float DEFAULT_WORKER_SPEED = 100.0f;

    /// <summary>
    /// Default work efficiency multiplier (1.0 = 100% efficiency).
    /// </summary>
    public const float DEFAULT_WORKER_EFFICIENCY = 1.0f;

    // === Building Configuration ===

    /// <summary>
    /// Default construction time for buildings in seconds.
    /// </summary>
    public const float DEFAULT_BUILD_TIME = 5.0f;

    /// <summary>
    /// Default building size in tiles (1x1 for single-tile buildings).
    /// </summary>
    public const int DEFAULT_BUILDING_TILE_SIZE = 1;

    // === Game Speed Configuration ===

    /// <summary>
    /// Minimum allowed game speed (paused).
    /// </summary>
    public const float MIN_GAME_SPEED = 0.0f;

    /// <summary>
    /// Maximum allowed game speed.
    /// </summary>
    public const float MAX_GAME_SPEED = 3.0f;

    /// <summary>
    /// Normal game speed (1x, real-time).
    /// </summary>
    public const float GAME_SPEED_NORMAL = 1.0f;

    /// <summary>
    /// Fast game speed (2x).
    /// </summary>
    public const float GAME_SPEED_FAST = 2.0f;

    /// <summary>
    /// Faster game speed (3x).
    /// </summary>
    public const float GAME_SPEED_FASTER = 3.0f;

    // === Placeholder Sprite Dimensions ===

    /// <summary>
    /// Default size for resource placeholder sprites in pixels.
    /// </summary>
    public const float RESOURCE_SPRITE_SIZE = 16f;

    /// <summary>
    /// Default size for worker placeholder sprites in pixels.
    /// </summary>
    public const float WORKER_SPRITE_SIZE = 16f;

    /// <summary>
    /// Default width for building placeholder sprites in pixels.
    /// </summary>
    public const float BUILDING_SPRITE_WIDTH = 32f;

    /// <summary>
    /// Default height for building placeholder sprites in pixels.
    /// </summary>
    public const float BUILDING_SPRITE_HEIGHT = 32f;
}
