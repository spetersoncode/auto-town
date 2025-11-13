using Godot;
using System.Collections.Generic;
using autotown.Core;
using autotown.Data;

namespace autotown.Systems;

/// <summary>
/// Stores the state of the generated world including terrain, resource nodes, and buildings.
/// Provides methods to query tile properties and convert between coordinate systems.
/// </summary>
public class WorldData
{
    /// <summary>
    /// Width of the map in tiles.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Height of the map in tiles.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// 2D array storing tile types for the entire map.
    /// Access via [x, y] where (0,0) is top-left.
    /// </summary>
    private TileType[,] _tiles;

    /// <summary>
    /// List of all resource node positions in the world.
    /// Key: ResourceType, Value: List of world positions
    /// </summary>
    private Dictionary<ResourceType, List<Vector2>> _resourceNodes;

    /// <summary>
    /// List of all building positions in the world.
    /// Currently stores world positions; will expand in Phase 6.
    /// </summary>
    private List<Vector2> _buildingPositions;

    /// <summary>
    /// Initializes a new WorldData instance with the specified dimensions.
    /// </summary>
    /// <param name="width">Map width in tiles</param>
    /// <param name="height">Map height in tiles</param>
    public WorldData(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new TileType[width, height];
        _resourceNodes = new Dictionary<ResourceType, List<Vector2>>();
        _buildingPositions = new List<Vector2>();

        // Initialize resource node lists for each type
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            _resourceNodes[type] = new List<Vector2>();
        }
    }

    /// <summary>
    /// Sets the tile type at the specified tile coordinates.
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <param name="type">Type of tile to set</param>
    public void SetTile(int tileX, int tileY, TileType type)
    {
        if (IsValidTileCoord(tileX, tileY))
        {
            _tiles[tileX, tileY] = type;
        }
    }

    /// <summary>
    /// Gets the tile type at the specified tile coordinates.
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <returns>TileType at the specified coordinates, or Grass if out of bounds</returns>
    public TileType GetTile(int tileX, int tileY)
    {
        if (IsValidTileCoord(tileX, tileY))
        {
            return _tiles[tileX, tileY];
        }
        return TileType.Grass; // Default to grass for out-of-bounds
    }

    /// <summary>
    /// Checks if the specified tile coordinates are within map bounds.
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <returns>True if coordinates are valid, false otherwise</returns>
    public bool IsValidTileCoord(int tileX, int tileY)
    {
        return tileX >= 0 && tileX < Width && tileY >= 0 && tileY < Height;
    }

    /// <summary>
    /// Checks if the tile at the specified coordinates is walkable.
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <returns>True if walkable (Grass or Dirt), false otherwise</returns>
    public bool IsWalkable(int tileX, int tileY)
    {
        if (!IsValidTileCoord(tileX, tileY))
            return false;

        TileType type = GetTile(tileX, tileY);
        return type == TileType.Grass || type == TileType.Dirt;
    }

    /// <summary>
    /// Checks if the tile at the specified coordinates is buildable.
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <returns>True if buildable (Grass or Dirt), false otherwise</returns>
    public bool IsBuildable(int tileX, int tileY)
    {
        if (!IsValidTileCoord(tileX, tileY))
            return false;

        TileType type = GetTile(tileX, tileY);
        return type == TileType.Grass || type == TileType.Dirt;
    }

    /// <summary>
    /// Converts world position (in pixels) to tile coordinates.
    /// </summary>
    /// <param name="worldPos">World position in pixels</param>
    /// <returns>Tile coordinates (X, Y)</returns>
    public Vector2I WorldToTile(Vector2 worldPos)
    {
        int tileX = Mathf.FloorToInt(worldPos.X / GameConfig.TILE_SIZE);
        int tileY = Mathf.FloorToInt(worldPos.Y / GameConfig.TILE_SIZE);
        return new Vector2I(tileX, tileY);
    }

    /// <summary>
    /// Converts tile coordinates to world position (in pixels, centered on tile).
    /// </summary>
    /// <param name="tileX">Tile X coordinate</param>
    /// <param name="tileY">Tile Y coordinate</param>
    /// <returns>World position at the center of the tile</returns>
    public Vector2 TileToWorld(int tileX, int tileY)
    {
        float worldX = tileX * GameConfig.TILE_SIZE + GameConfig.TILE_SIZE / 2f;
        float worldY = tileY * GameConfig.TILE_SIZE + GameConfig.TILE_SIZE / 2f;
        return new Vector2(worldX, worldY);
    }

    /// <summary>
    /// Adds a resource node position to the world data.
    /// </summary>
    /// <param name="type">Type of resource</param>
    /// <param name="worldPos">World position of the resource node</param>
    public void AddResourceNode(ResourceType type, Vector2 worldPos)
    {
        if (_resourceNodes.ContainsKey(type))
        {
            _resourceNodes[type].Add(worldPos);
        }
    }

    /// <summary>
    /// Gets all resource node positions for a specific resource type.
    /// </summary>
    /// <param name="type">Type of resource</param>
    /// <returns>List of world positions for the resource type</returns>
    public List<Vector2> GetResourceNodes(ResourceType type)
    {
        return _resourceNodes.ContainsKey(type) ? _resourceNodes[type] : new List<Vector2>();
    }

    /// <summary>
    /// Adds a building position to the world data.
    /// </summary>
    /// <param name="worldPos">World position of the building</param>
    public void AddBuildingPosition(Vector2 worldPos)
    {
        _buildingPositions.Add(worldPos);
    }

    /// <summary>
    /// Gets all building positions in the world.
    /// </summary>
    /// <returns>List of building world positions</returns>
    public List<Vector2> GetBuildingPositions()
    {
        return _buildingPositions;
    }

    /// <summary>
    /// Gets the center position of the map in world coordinates.
    /// </summary>
    /// <returns>World position at the center of the map</returns>
    public Vector2 GetMapCenter()
    {
        return TileToWorld(Width / 2, Height / 2);
    }

    /// <summary>
    /// Gets the total number of resource nodes for a specific type.
    /// </summary>
    /// <param name="type">Type of resource</param>
    /// <returns>Count of resource nodes</returns>
    public int GetResourceNodeCount(ResourceType type)
    {
        return _resourceNodes.ContainsKey(type) ? _resourceNodes[type].Count : 0;
    }
}
