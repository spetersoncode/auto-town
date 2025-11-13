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

    // === Map Generation Configuration ===

    /// <summary>
    /// Width of the generated map in tiles.
    /// </summary>
    public const int MAP_WIDTH = 100;

    /// <summary>
    /// Height of the generated map in tiles.
    /// </summary>
    public const int MAP_HEIGHT = 100;

    /// <summary>
    /// Size of each tile in pixels.
    /// </summary>
    public const int TILE_SIZE = 16;

    /// <summary>
    /// Noise frequency for terrain generation (higher = more varied).
    /// </summary>
    public const float NOISE_FREQUENCY = 0.05f;

    /// <summary>
    /// Number of noise octaves for terrain detail.
    /// </summary>
    public const int NOISE_OCTAVES = 3;

    /// <summary>
    /// Threshold for water generation (-1 to 1, values below this become water).
    /// </summary>
    public const float WATER_THRESHOLD = -0.3f;

    /// <summary>
    /// Threshold for mountain generation (-1 to 1, values above this become mountains).
    /// </summary>
    public const float MOUNTAIN_THRESHOLD = 0.5f;

    // === Resource Spawning Configuration ===

    /// <summary>
    /// Number of tree clusters to spawn across the map.
    /// </summary>
    public const int TREE_CLUSTER_COUNT = 15;

    /// <summary>
    /// Number of trees per cluster (approximate).
    /// </summary>
    public const int TREES_PER_CLUSTER = 8;

    /// <summary>
    /// Radius of tree cluster spawn area in tiles.
    /// </summary>
    public const int TREE_CLUSTER_RADIUS = 4;

    /// <summary>
    /// Number of stone deposit clusters to spawn.
    /// </summary>
    public const int STONE_CLUSTER_COUNT = 10;

    /// <summary>
    /// Number of stone nodes per cluster (approximate).
    /// </summary>
    public const int STONES_PER_CLUSTER = 5;

    /// <summary>
    /// Radius of stone cluster spawn area in tiles.
    /// </summary>
    public const int STONE_CLUSTER_RADIUS = 3;

    /// <summary>
    /// Number of foraging areas to spawn.
    /// </summary>
    public const int FORAGE_AREA_COUNT = 12;

    /// <summary>
    /// Number of forage nodes per area (approximate).
    /// </summary>
    public const int FORAGES_PER_AREA = 6;

    /// <summary>
    /// Radius of foraging area spawn in tiles.
    /// </summary>
    public const int FORAGE_AREA_RADIUS = 3;

    /// <summary>
    /// Minimum spacing between resource clusters in tiles.
    /// </summary>
    public const int RESOURCE_MIN_SPACING = 8;

    // === Resource Harvesting Configuration ===

    /// <summary>
    /// Maximum number of times a tree can be harvested before depletion.
    /// </summary>
    public const int TREE_MAX_HARVESTS = 5;

    /// <summary>
    /// Amount of wood gathered per tree harvest.
    /// </summary>
    public const int TREE_YIELD_PER_HARVEST = 10;

    /// <summary>
    /// Time required to harvest a tree in seconds.
    /// </summary>
    public const float TREE_HARVEST_DURATION = 2.0f;

    /// <summary>
    /// Maximum number of times a stone deposit can be harvested before depletion.
    /// </summary>
    public const int STONE_MAX_HARVESTS = 3;

    /// <summary>
    /// Amount of stone gathered per stone harvest.
    /// </summary>
    public const int STONE_YIELD_PER_HARVEST = 15;

    /// <summary>
    /// Time required to harvest stone in seconds.
    /// </summary>
    public const float STONE_HARVEST_DURATION = 3.0f;

    /// <summary>
    /// Maximum number of times a forage area can be harvested before depletion.
    /// </summary>
    public const int FORAGE_MAX_HARVESTS = 4;

    /// <summary>
    /// Amount of food gathered per forage harvest.
    /// </summary>
    public const int FORAGE_YIELD_PER_HARVEST = 8;

    /// <summary>
    /// Time required to forage for food in seconds.
    /// </summary>
    public const float FORAGE_HARVEST_DURATION = 1.5f;

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

    // === Camera Configuration ===

    /// <summary>
    /// Camera panning speed in pixels per second.
    /// </summary>
    public const float CAMERA_MOVE_SPEED = 300f;

    /// <summary>
    /// Minimum camera zoom level (zoomed out).
    /// </summary>
    public const float CAMERA_ZOOM_MIN = 0.5f;

    /// <summary>
    /// Maximum camera zoom level (zoomed in).
    /// </summary>
    public const float CAMERA_ZOOM_MAX = 2.0f;

    /// <summary>
    /// Default camera zoom level.
    /// </summary>
    public const float CAMERA_ZOOM_DEFAULT = 1.0f;

    /// <summary>
    /// Zoom step when using mouse wheel.
    /// </summary>
    public const float CAMERA_ZOOM_STEP = 0.1f;

    /// <summary>
    /// Edge scrolling activation threshold in pixels from screen edge.
    /// </summary>
    public const float CAMERA_EDGE_SCROLL_THRESHOLD = 20f;

    /// <summary>
    /// Edge scrolling speed multiplier (multiplies CAMERA_MOVE_SPEED).
    /// </summary>
    public const float CAMERA_EDGE_SCROLL_SPEED = 1.0f;

    /// <summary>
    /// Camera boundary padding in tiles (prevents camera from going past map edge).
    /// </summary>
    public const int CAMERA_BOUNDARY_PADDING = 2;
}
