using Godot;
using autotown.Core;

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
    /// Random seed for world generation. If 0, uses random seed.
    /// </summary>
    [Export]
    public int Seed { get; set; } = 0;

    public override void _Ready()
    {
        GD.Print("WorldController: Initializing world...");

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

        // Configure camera
        _camera.SetBounds(WorldData.Width, WorldData.Height);
        _camera.CenterOn(WorldData.GetMapCenter());

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
}
