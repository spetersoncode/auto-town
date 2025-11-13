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

    /// <summary>
    /// Range in pixels within which a worker can interact with resources/buildings.
    /// </summary>
    public const float WORKER_INTERACTION_RANGE = 24.0f;

    // === Navigation Configuration ===

    /// <summary>
    /// Maximum speed for NavigationAgent2D in pixels per second.
    /// </summary>
    public const float NAV_AGENT_MAX_SPEED = 100.0f;

    /// <summary>
    /// Path desired distance - how close the agent gets to target before considering it reached.
    /// </summary>
    public const float NAV_AGENT_PATH_DESIRED_DISTANCE = 8.0f;

    /// <summary>
    /// Target desired distance - how close to final target before stopping.
    /// Must be smaller than WORKER_INTERACTION_RANGE so workers can actually interact.
    /// </summary>
    public const float NAV_AGENT_TARGET_DESIRED_DISTANCE = 10.0f;

    /// <summary>
    /// Radius of the navigation agent for collision avoidance.
    /// </summary>
    public const float NAV_AGENT_RADIUS = 8.0f;

    /// <summary>
    /// Navigation mesh cell size in pixels (affects pathfinding precision).
    /// </summary>
    public const float NAV_MESH_CELL_SIZE = 8.0f;

    /// <summary>
    /// Border size around navigation mesh obstacles.
    /// </summary>
    public const float NAV_MESH_BORDER_SIZE = 4.0f;

    // === Task System Configuration ===

    /// <summary>
    /// How often workers scan for new tasks in seconds (avoids checking every frame).
    /// </summary>
    public const float TASK_SCAN_INTERVAL = 0.5f;

    /// <summary>
    /// Maximum distance a worker will consider for tasks in pixels (0 = unlimited).
    /// </summary>
    public const float MAX_TASK_DISTANCE = 800.0f;

    /// <summary>
    /// Maximum number of pending tasks in the queue (prevents unbounded growth).
    /// </summary>
    public const int MAX_TASK_QUEUE_SIZE = 200;

    // === Building Configuration ===

    /// <summary>
    /// Default construction time for buildings in seconds.
    /// </summary>
    public const float DEFAULT_BUILD_TIME = 5.0f;

    /// <summary>
    /// Default building size in tiles (1x1 for single-tile buildings).
    /// </summary>
    public const int DEFAULT_BUILDING_TILE_SIZE = 1;


    // === Building Functionality ===

    /// <summary>
    /// Amount of resources hauled per trip to construction site.
    /// </summary>
    public const int CONSTRUCTION_HAUL_AMOUNT = 20;

    /// <summary>
    /// Interval between processing task generation (in seconds).
    /// </summary>
    public const float PROCESSING_TASK_INTERVAL = 10.0f;

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

    // === Grid Configuration ===

    /// <summary>
    /// Size of each tile in pixels. Fundamental grid size used throughout the game.
    /// </summary>
    public const int TILE_SIZE = 16;

    // === Starting Resources Configuration ===

    /// <summary>
    /// Starting wood when a new game begins.
    /// </summary>
    public const int STARTING_WOOD = 50;

    /// <summary>
    /// Starting stone when a new game begins.
    /// </summary>
    public const int STARTING_STONE = 30;

    /// <summary>
    /// Starting food when a new game begins.
    /// </summary>
    public const int STARTING_FOOD = 100;

    /// <summary>
    /// Maximum capacity per resource type in stockpile (0 = unlimited).
    /// </summary>
    public const int STOCKPILE_CAPACITY_PER_RESOURCE = 0; // Unlimited for tech demo

    // === Population Growth Configuration ===

    /// <summary>
    /// Amount of food required at town hall to spawn a new worker.
    /// </summary>
    public const int FOOD_PER_WORKER = 200;

    /// <summary>
    /// How often the PopulationManager checks growth conditions in seconds.
    /// </summary>
    public const float GROWTH_CHECK_INTERVAL = 10.0f;

    /// <summary>
    /// Amount of food delivered per GrowthFoodTask haul trip.
    /// </summary>
    public const int FOOD_PER_HAUL_TRIP = 20;
}
