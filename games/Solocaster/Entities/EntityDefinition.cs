using System.Collections.Generic;

namespace Solocaster.Entities;

public class EntityDefinition
{
    public required string Type { get; init; }
    public int TileX { get; init; }
    public int TileY { get; init; }
    public IReadOnlyDictionary<string, object> Properties { get; init; } = new Dictionary<string, object>();
}
