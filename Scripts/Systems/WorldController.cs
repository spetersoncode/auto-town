using Godot;
using System.Linq;
using autotown.Core;
using autotown.Data;
using autotown.Entities;

namespace autotown.Systems;

/// <summary>
/// Controls world initialization and manages world state.
/// Attach this script to the World node.
/// </summary>
public partial class WorldController : Node2D
{
    /// <summary>
    /// Reference to the generated world data.
    /// </summary>
    public WorldData WorldData { get; private set; }

    /// <summary>
    /// Reference to the world generator.
    /// </summary>
    private WorldGenerator _worldGenerator;

    /// <summary>
    /// Reference to the camera controller.
    /// </summary>
    private CameraController _camera;

    /// <summary>
    /// Reference to the navigation region.
    /// </summary>
    private NavigationRegion2D _navigationRegion;

    /// <summary>
    /// Reference to the stockpile building.
    /// </summary>
    private Stockpile _stockpile;

    /// <summary>
    /// Cached reference to TaskManager.
    /// </summary>
    private TaskManager _taskManager;

    /// <summary>
    /// Cached reference to WorkerManager.
    /// </summary>
    private WorkerManager _workerManager;

    /// <summary>
    /// Random seed for world generation. If 0, uses random seed.
    /// </summary>
    [Export]
    public int Seed { get; set; } = 0;

    public override void _Ready()
    {
        GD.Print("WorldController: Initializing world...");

        // Cache autoload references
        _taskManager = GetNode<TaskManager>("/root/TaskManager");
        _workerManager = GetNode<WorkerManager>("/root/WorkerManager");

        // Get references to child nodes
        TileMapLayer tileMap = GetNode<TileMapLayer>("Terrain/TileMapLayer");
        _camera = GetNode<CameraController>("Camera");

        // Create or configure TileSet
        SetupTileSet(tileMap);

        // Create world generator
        int? seed = Seed == 0 ? null : (int?)Seed;
        _worldGenerator = new WorldGenerator(seed);

        // Generate world
        WorldData = _worldGenerator.Generate(tileMap, this);

        // Setup navigation mesh (must be after world generation)
        SetupNavigationMesh();

        // Configure camera
        _camera.SetBounds(WorldData.Width, WorldData.Height);
        _camera.CenterOn(WorldData.GetMapCenter());

        // Find the stockpile (spawned during world generation)
        FindStockpile();

        // Generate gather tasks for all resource nodes
        GenerateGatherTasks();

        // Spawn starter workers
        SpawnStarterWorkers();

        GD.Print("WorldController: World initialization complete!");
    }

    /// <summary>
    /// Sets up a simple TileSet for terrain rendering using placeholder colors.
    /// Creates a TileSet with 4 tiles: Grass, Dirt, Water, Mountain.
    /// </summary>
    private void SetupTileSet(TileMapLayer tileMap)
    {
        // Create a new TileSet
        TileSet tileSet = new TileSet();
        tileSet.TileSize = new Vector2I(GameConfig.TILE_SIZE, GameConfig.TILE_SIZE);

        // Create a single source for our tiles
        TileSetAtlasSource atlasSource = new TileSetAtlasSource();
        atlasSource.TextureRegionSize = new Vector2I(GameConfig.TILE_SIZE, GameConfig.TILE_SIZE);

        // We'll create a simple 1x1 pixel texture per tile and use modulate color
        // Create 4 tiles (one for each TileType)
        Color[] tileColors = new Color[]
        {
            new Color(0.15f, 0.4f, 0.15f),  // Grass - Darker green for better tree contrast
            new Color(0.55f, 0.4f, 0.25f),  // Dirt - Brown
            new Color(0.2f, 0.4f, 0.8f),    // Water - Blue
            new Color(0.4f, 0.4f, 0.4f)     // Mountain - Gray
        };

        // Create a simple white 16x16 image that we'll modulate with colors
        Image whiteImage = Image.CreateEmpty(GameConfig.TILE_SIZE, GameConfig.TILE_SIZE, false, Image.Format.Rgba8);
        whiteImage.Fill(Colors.White);
        ImageTexture baseTexture = ImageTexture.CreateFromImage(whiteImage);
        atlasSource.Texture = baseTexture;

        // Create atlas coordinates for each tile type
        for (int i = 0; i < 4; i++)
        {
            Vector2I atlasCoord = new Vector2I(i, 0);
            atlasSource.CreateTile(atlasCoord);

            // Set tile color modulation
            TileData tileData = atlasSource.GetTileData(atlasCoord, 0);
            if (tileData != null)
            {
                tileData.Modulate = tileColors[i];
            }
        }

        // Add atlas source to tileset
        tileSet.AddSource(atlasSource, 0);

        // Assign tileset to TileMapLayer
        tileMap.TileSet = tileSet;

        GD.Print("WorldController: TileSet configured with 4 terrain types");
    }

    /// <summary>
    /// Gets the world position of the map center.
    /// </summary>
    /// <returns>Center position in world coordinates</returns>
    public Vector2 GetMapCenter()
    {
        return WorldData?.GetMapCenter() ?? Vector2.Zero;
    }

    /// <summary>
    /// Sets up a simple navigation mesh for worker pathfinding.
    /// Just a flat mesh covering the entire map - physics handles obstacle avoidance.
    /// </summary>
    private void SetupNavigationMesh()
    {
        GD.Print("WorldController: Setting up navigation mesh...");

        // Create navigation region
        _navigationRegion = new NavigationRegion2D();
        _navigationRegion.Name = "NavigationRegion";
        AddChild(_navigationRegion);

        // Create simple navigation polygon covering entire map
        var navPoly = new NavigationPolygon();

        // Use larger cell size for simpler pathfinding
        navPoly.CellSize = 8.0f;

        // Create walkable area covering the entire map
        float mapWidthPixels = WorldData.Width * GameConfig.TILE_SIZE;
        float mapHeightPixels = WorldData.Height * GameConfig.TILE_SIZE;

        // Simple rectangular outline
        var walkableOutline = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(mapWidthPixels, 0),
            new Vector2(mapWidthPixels, mapHeightPixels),
            new Vector2(0, mapHeightPixels)
        };
        navPoly.AddOutline(walkableOutline);

        // Bake the navigation mesh
        #pragma warning disable CS0618
        navPoly.MakePolygonsFromOutlines();
        #pragma warning restore CS0618

        _navigationRegion.NavigationPolygon = navPoly;
        _navigationRegion.Enabled = true;

        GD.Print($"WorldController: Simple navigation mesh created (flat, no obstacles)");
    }

    /// <summary>
    /// Finds the stockpile building in the Buildings container.
    /// </summary>
    private void FindStockpile()
    {
        var buildingsContainer = GetNode<Node2D>("Buildings");
        foreach (var child in buildingsContainer.GetChildren())
        {
            if (child is Stockpile stockpile)
            {
                _stockpile = stockpile;
                GD.Print($"WorldController: Found stockpile at {_stockpile.GlobalPosition}");
                return;
            }
        }

        GD.PrintErr("WorldController: Stockpile not found!");
    }

    /// <summary>
    /// Generates gather tasks for all resource nodes on the map.
    /// Note: We DON'T pre-generate all tasks anymore. Workers will find resources dynamically.
    /// This just sets up resource depletion monitoring.
    /// </summary>
    private void GenerateGatherTasks()
    {
        if (_stockpile == null)
        {
            GD.PrintErr("WorldController: Cannot generate tasks - stockpile not found");
            return;
        }

        var resourceNodesContainer = GetNode<Node2D>("ResourceNodes");
        int resourceCount = 0;

        foreach (var child in resourceNodesContainer.GetChildren())
        {
            if (child is HarvestableResource resource)
            {
                // Subscribe to resource depletion to cancel related tasks
                resource.ResourceDepleted += (res) => OnResourceDepleted(res);
                resourceCount++;
            }
        }

        GD.Print($"WorldController: Monitoring {resourceCount} resource nodes (tasks created on-demand)");
    }

    /// <summary>
    /// Spawns starter workers at the town hall position.
    /// </summary>
    private void SpawnStarterWorkers()
    {
        // Find town hall position (center of map)
        var townHallPos = WorldData.GetMapCenter();

        var workersContainer = GetNode<Node2D>("Workers");

        // Spawn 5 workers with different jobs
        _workerManager.SpawnWorker(townHallPos + new Vector2(-32, -32), JobType.Lumberjack, workersContainer);
        _workerManager.SpawnWorker(townHallPos + new Vector2(32, -32), JobType.Miner, workersContainer);
        _workerManager.SpawnWorker(townHallPos + new Vector2(-32, 32), JobType.Forager, workersContainer);
        _workerManager.SpawnWorker(townHallPos + new Vector2(32, 32), JobType.Builder, workersContainer);
        _workerManager.SpawnWorker(townHallPos, JobType.Builder, workersContainer);

        // Set stockpile reference for all workers
        foreach (var worker in _workerManager.GetAllWorkers())
        {
            worker.SetStockpile(_stockpile);
        }

        GD.Print($"WorldController: Spawned {_workerManager.WorkerCount} starter workers");
    }

    /// <summary>
    /// Called when a resource is depleted. Cancels all tasks for that resource.
    /// </summary>
    private void OnResourceDepleted(HarvestableResource resource)
    {
        _taskManager.CancelTasksForResource(resource);
        GD.Print($"WorldController: Resource depleted, cancelled related tasks");
    }
}
