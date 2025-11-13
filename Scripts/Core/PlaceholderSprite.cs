using Godot;
using autotown.Data;

namespace autotown.Core;

/// <summary>
/// Utility class for creating placeholder sprites with color-coding.
/// Used during development before final art assets are available.
/// </summary>
public static class PlaceholderSprite
{
    // Color coding scheme
    private static readonly Color WoodColor = new(0.13f, 0.55f, 0.13f); // Forest Green
    private static readonly Color StoneColor = new(0.5f, 0.5f, 0.5f);   // Gray
    private static readonly Color FoodColor = new(1.0f, 0.84f, 0.0f);   // Gold/Yellow
    private static readonly Color WorkerColor = new(0.3f, 0.6f, 1.0f);  // Light Blue
    private static readonly Color BuildingColor = new(0.8f, 0.2f, 0.2f); // Red/Brown
    private static readonly Color DefaultColor = new(1.0f, 1.0f, 1.0f); // White

    /// <summary>
    /// Creates a ColorRect node as a placeholder sprite.
    /// </summary>
    /// <param name="size">Size of the rectangle</param>
    /// <param name="color">Color of the rectangle</param>
    /// <returns>Configured ColorRect node</returns>
    public static ColorRect CreateRect(Vector2 size, Color color)
    {
        var rect = new ColorRect
        {
            Size = size,
            Color = color,
            Position = -size / 2 // Center the rect
        };
        return rect;
    }

    /// <summary>
    /// Creates a placeholder sprite for a resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <param name="size">Size of the sprite (default: 16x16)</param>
    /// <returns>ColorRect representing the resource</returns>
    public static ColorRect CreateResourceSprite(ResourceType resourceType, float size = GameConfig.RESOURCE_SPRITE_SIZE)
    {
        Color color = resourceType switch
        {
            ResourceType.Wood => WoodColor,
            ResourceType.Stone => StoneColor,
            ResourceType.Food => FoodColor,
            _ => DefaultColor
        };

        return CreateRect(new Vector2(size, size), color);
    }

    /// <summary>
    /// Creates a placeholder sprite for a worker.
    /// </summary>
    /// <param name="size">Size of the sprite (default: 16x16)</param>
    /// <returns>ColorRect representing the worker</returns>
    public static ColorRect CreateWorkerSprite(float size = GameConfig.WORKER_SPRITE_SIZE)
    {
        return CreateRect(new Vector2(size, size), WorkerColor);
    }

    /// <summary>
    /// Creates a placeholder sprite for a building.
    /// </summary>
    /// <param name="width">Width of the building sprite</param>
    /// <param name="height">Height of the building sprite</param>
    /// <returns>ColorRect representing the building</returns>
    public static ColorRect CreateBuildingSprite(float width = GameConfig.BUILDING_SPRITE_WIDTH, float height = GameConfig.BUILDING_SPRITE_HEIGHT)
    {
        return CreateRect(new Vector2(width, height), BuildingColor);
    }

    /// <summary>
    /// Gets the color associated with a resource type.
    /// </summary>
    /// <param name="resourceType">Type of resource</param>
    /// <returns>Color for the resource type</returns>
    public static Color GetResourceColor(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Wood => WoodColor,
            ResourceType.Stone => StoneColor,
            ResourceType.Food => FoodColor,
            _ => DefaultColor
        };
    }

    /// <summary>
    /// Gets the color for workers.
    /// </summary>
    /// <returns>Worker color</returns>
    public static Color GetWorkerColor() => WorkerColor;

    /// <summary>
    /// Gets the color for buildings.
    /// </summary>
    /// <returns>Building color</returns>
    public static Color GetBuildingColor() => BuildingColor;
}
