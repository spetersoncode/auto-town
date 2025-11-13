using Godot;
using System;
using System.Collections.Generic;
using autotown.Core;
using autotown.Data;
using autotown.Entities;

namespace autotown.Systems;

/// <summary>
/// Handles procedural world generation including terrain, resource nodes, and initial buildings.
/// Uses noise-based algorithms to create varied and interesting maps.
/// </summary>
public class WorldGenerator
{
    private Random _random;
    private FastNoiseLite _noise;
    private MapGenerationConfig _config;

    /// <summary>
    /// Initializes the world generator with configuration and optional seed.
    /// </summary>
    /// <param name="config">Map generation configuration</param>
    /// <param name="seed">Random seed for generation. If null, uses random seed.</param>
    public WorldGenerator(MapGenerationConfig config, int? seed = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        int actualSeed = seed ?? new Random().Next();
        _random = new Random(actualSeed);

        // Configure noise generator
        _noise = new FastNoiseLite();
        _noise.Seed = actualSeed;
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _noise.Frequency = _config.NoiseFrequency;
        _noise.FractalOctaves = _config.NoiseOctaves;
        _noise.FractalType = FastNoiseLite.FractalTypeEnum.Fbm;

        GD.Print($"WorldGenerator: Initialized with seed {actualSeed}");
    }

    /// <summary>
    /// Generates the complete world including terrain and resource nodes.
    /// </summary>
    /// <param name="tileMap">TileMapLayer to populate with terrain</param>
    /// <param name="worldNode">World node containing ResourceNodes and Buildings containers</param>
    /// <returns>WorldData containing the generated world state</returns>
    public WorldData Generate(TileMapLayer tileMap, Node2D worldNode)
    {
        GD.Print("WorldGenerator: Starting world generation...");

        // Create world data
        WorldData worldData = new WorldData(_config.MapWidth, _config.MapHeight);

        // Generate terrain
        GenerateTerrain(tileMap, worldData);

        // Spawn resource nodes
        Node2D resourceContainer = worldNode.GetNode<Node2D>("ResourceNodes");
        SpawnResourceNodes(resourceContainer, worldData);

        // Spawn starter buildings (TownHall, Stockpile, and House at center)
        Node2D buildingContainer = worldNode.GetNode<Node2D>("Buildings");
        SpawnStarterTownHall(buildingContainer, worldData);
        SpawnStarterStockpile(buildingContainer, worldData);
        SpawnStarterHouse(buildingContainer, worldData);

        GD.Print("WorldGenerator: World generation complete!");
        return worldData;
    }

    /// <summary>
    /// Generates terrain using noise-based algorithm and populates the TileMapLayer.
    /// </summary>
    private void GenerateTerrain(TileMapLayer tileMap, WorldData worldData)
    {
        GD.Print("WorldGenerator: Generating terrain...");

        int waterTiles = 0;
        int mountainTiles = 0;
        int grassTiles = 0;
        int dirtTiles = 0;

        for (int x = 0; x < _config.MapWidth; x++)
        {
            for (int y = 0; y < _config.MapHeight; y++)
            {
                // Get noise value (-1 to 1)
                float noiseValue = _noise.GetNoise2D(x, y);

                // Determine tile type based on noise thresholds
                TileType tileType;
                if (noiseValue < _config.WaterThreshold)
                {
                    tileType = TileType.Water;
                    waterTiles++;
                }
                else if (noiseValue > _config.MountainThreshold)
                {
                    tileType = TileType.Mountain;
                    mountainTiles++;
                }
                else
                {
                    // Mix grass and dirt for variation
                    if (_random.NextDouble() < 0.7)
                    {
                        tileType = TileType.Grass;
                        grassTiles++;
                    }
                    else
                    {
                        tileType = TileType.Dirt;
                        dirtTiles++;
                    }
                }

                // Store in world data
                worldData.SetTile(x, y, tileType);

                // Set tile in TileMapLayer (using TileType enum value as atlas coords)
                Vector2I tileCoord = new Vector2I(x, y);
                Vector2I atlasCoord = new Vector2I((int)tileType, 0);
                tileMap.SetCell(tileCoord, 0, atlasCoord);
            }
        }

        GD.Print($"WorldGenerator: Terrain generated - Grass: {grassTiles}, Dirt: {dirtTiles}, Water: {waterTiles}, Mountain: {mountainTiles}");
    }

    /// <summary>
    /// Spawns all resource node clusters across the map.
    /// </summary>
    private void SpawnResourceNodes(Node2D container, WorldData worldData)
    {
        GD.Print("WorldGenerator: Spawning resource nodes...");

        // Spawn tree clusters
        SpawnResourceClusters(
            container,
            worldData,
            ResourceType.Wood,
            _config.TreeClusterCount,
            _config.TreesPerCluster,
            _config.TreeClusterRadius,
            "res://Scenes/Entities/TreeNode.tscn"
        );

        // Spawn stone deposits
        SpawnResourceClusters(
            container,
            worldData,
            ResourceType.Stone,
            _config.StoneClusterCount,
            _config.StonesPerCluster,
            _config.StoneClusterRadius,
            "res://Scenes/Entities/StoneNode.tscn"
        );

        // Spawn foraging areas
        SpawnResourceClusters(
            container,
            worldData,
            ResourceType.Food,
            _config.ForageAreaCount,
            _config.ForagesPerArea,
            _config.ForageAreaRadius,
            "res://Scenes/Entities/ForageNode.tscn"
        );

        int treeCount = worldData.GetResourceNodeCount(ResourceType.Wood);
        int stoneCount = worldData.GetResourceNodeCount(ResourceType.Stone);
        int forageCount = worldData.GetResourceNodeCount(ResourceType.Food);
        GD.Print($"WorldGenerator: Resource nodes spawned - Trees: {treeCount}, Stone: {stoneCount}, Forage: {forageCount} (Total: {treeCount + stoneCount + forageCount})");
    }

    /// <summary>
    /// Spawns a specific type of resource in clusters across the map.
    /// </summary>
    private void SpawnResourceClusters(Node2D container, WorldData worldData, ResourceType resourceType,
                                       int clusterCount, int nodesPerCluster, int clusterRadius, string scenePath)
    {
        PackedScene nodeScene = GD.Load<PackedScene>(scenePath);
        if (nodeScene == null)
        {
            GD.PrintErr($"WorldGenerator: Failed to load scene: {scenePath}");
            return;
        }

        List<Vector2> clusterCenters = new List<Vector2>();

        for (int i = 0; i < clusterCount; i++)
        {
            // Find valid cluster center (walkable tile, away from other clusters)
            Vector2? centerPos = FindValidClusterCenter(worldData, clusterCenters);
            if (centerPos == null)
            {
                GD.Print($"WorldGenerator: Could not find valid position for {resourceType} cluster {i + 1}");
                continue;
            }

            clusterCenters.Add(centerPos.Value);

            // Spawn nodes around cluster center
            for (int j = 0; j < nodesPerCluster; j++)
            {
                Vector2? nodePos = FindValidNodePosition(worldData, centerPos.Value, clusterRadius);
                if (nodePos == null)
                    continue;

                // Instantiate node
                Node2D resourceNode = nodeScene.Instantiate<Node2D>();
                resourceNode.Position = nodePos.Value;
                container.AddChild(resourceNode);

                // Register in world data
                worldData.AddResourceNode(resourceType, nodePos.Value);
            }
        }
    }

    /// <summary>
    /// Finds a valid position for a cluster center.
    /// </summary>
    private Vector2? FindValidClusterCenter(WorldData worldData, List<Vector2> existingCenters)
    {
        const int maxAttempts = 100;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int tileX = _random.Next(0, worldData.Width);
            int tileY = _random.Next(0, worldData.Height);

            // Check if walkable
            if (!worldData.IsWalkable(tileX, tileY))
                continue;

            Vector2 worldPos = worldData.TileToWorld(tileX, tileY);

            // Check if too close to spawn point (map center)
            Vector2 spawnPoint = worldData.GetMapCenter();
            const float SPAWN_EXCLUSION_RADIUS = 160f; // 10 tiles * 16 pixels per tile
            float distanceFromSpawn = worldPos.DistanceTo(spawnPoint);
            if (distanceFromSpawn < SPAWN_EXCLUSION_RADIUS)
                continue;

            // Check spacing from other clusters
            bool tooClose = false;
            foreach (Vector2 existingCenter in existingCenters)
            {
                float distance = worldPos.DistanceTo(existingCenter);
                if (distance < _config.ResourceMinSpacing * _config.TileSize)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return worldPos;
        }

        return null; // Failed to find valid position
    }

    /// <summary>
    /// Finds a valid position for a resource node within a cluster.
    /// </summary>
    private Vector2? FindValidNodePosition(WorldData worldData, Vector2 clusterCenter, int radius)
    {
        const int maxAttempts = 20;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random offset within cluster radius
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float distance = (float)(_random.NextDouble() * radius * _config.TileSize);

            Vector2 offset = new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );

            Vector2 worldPos = clusterCenter + offset;
            Vector2I tileCoord = worldData.WorldToTile(worldPos);

            // Check if valid
            if (worldData.IsWalkable(tileCoord.X, tileCoord.Y))
                return worldPos;
        }

        return null;
    }

    /// <summary>
    /// Spawns a starter TownHall building at the map center.
    /// </summary>
    private void SpawnStarterTownHall(Node2D container, WorldData worldData)
    {
        GD.Print("WorldGenerator: Spawning starter TownHall...");

        Vector2 centerPos = worldData.GetMapCenter();

        // Create TownHall instance with script
        var townHall = new TownHall();
        townHall.Name = "StarterTownHall";
        townHall.Position = centerPos;

        // Add visual placeholder
        ColorRect visual = PlaceholderSprite.CreateBuildingSprite(
            GameConfig.BUILDING_SPRITE_WIDTH,
            GameConfig.BUILDING_SPRITE_HEIGHT
        );
        townHall.AddChild(visual);

        // No collision for now - just visual

        container.AddChild(townHall);
        worldData.AddBuildingPosition(centerPos);

        // Note: TownHall will be registered by WorldController after generation
        // Don't activate it here - let WorldController handle registration and activation

        GD.Print($"WorldGenerator: TownHall spawned at {centerPos} (no collision)");
    }

    /// <summary>
    /// Spawns a starter Stockpile building near the TownHall.
    /// </summary>
    private void SpawnStarterStockpile(Node2D container, WorldData worldData)
    {
        GD.Print("WorldGenerator: Spawning starter Stockpile...");

        Vector2 centerPos = worldData.GetMapCenter();

        // Offset from TownHall (8 tiles to the right for better spacing)
        Vector2 stockpilePos = centerPos + new Vector2(_config.TileSize * 8, 0);

        // Load stockpile scene
        PackedScene stockpileScene = GD.Load<PackedScene>("res://Scenes/Entities/Stockpile.tscn");
        if (stockpileScene == null)
        {
            GD.PrintErr("WorldGenerator: Failed to load Stockpile.tscn");
            return;
        }

        // Instantiate and position
        Node2D stockpile = stockpileScene.Instantiate<Node2D>();
        stockpile.Name = "StarterStockpile";
        stockpile.Position = stockpilePos;

        container.AddChild(stockpile);
        worldData.AddBuildingPosition(stockpilePos);

        GD.Print($"WorldGenerator: Stockpile spawned at {stockpilePos}");
    }

    /// <summary>
    /// Spawns a starter House to provide initial housing for the 5 starting workers.
    /// </summary>
    private void SpawnStarterHouse(Node2D container, WorldData worldData)
    {
        GD.Print("WorldGenerator: Spawning starter House...");

        Vector2 centerPos = worldData.GetMapCenter();

        // Offset from TownHall (6 tiles down for good spacing)
        Vector2 housePos = centerPos + new Vector2(0, _config.TileSize * 6);

        // Load house scene
        PackedScene houseScene = GD.Load<PackedScene>("res://Scenes/Entities/House.tscn");
        if (houseScene == null)
        {
            GD.PrintErr("WorldGenerator: Failed to load House.tscn");
            return;
        }

        // Instantiate and position
        var house = houseScene.Instantiate<Building>();
        house.Name = "StarterHouse";
        house.Position = housePos;

        container.AddChild(house);
        worldData.AddBuildingPosition(housePos);

        // Note: House will be registered by WorldController after generation
        // Don't activate it here - let WorldController handle registration and activation

        GD.Print($"WorldGenerator: Starter House spawned at {housePos}");
    }
}
