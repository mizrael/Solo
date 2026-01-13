using System.Collections.Generic;

namespace Solocaster.Entities;

public record EntityDefinition(
    string Type,
    int TileX,
    int TileY,
    IReadOnlyDictionary<string, object> Properties
);
