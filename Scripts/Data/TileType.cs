namespace autotown.Data;

/// <summary>
/// Defines the types of terrain tiles that can exist in the world.
/// Each tile type has different properties affecting walkability and buildability.
/// </summary>
public enum TileType
{
    /// <summary>
    /// Grass terrain - Walkable and buildable. Primary terrain type.
    /// </summary>
    Grass = 0,

    /// <summary>
    /// Dirt terrain - Walkable and buildable. Secondary terrain type.
    /// </summary>
    Dirt = 1,

    /// <summary>
    /// Water terrain - Not walkable and not buildable. Natural barrier.
    /// </summary>
    Water = 2,

    /// <summary>
    /// Mountain terrain - Not walkable and not buildable. Natural obstacle.
    /// </summary>
    Mountain = 3
}
