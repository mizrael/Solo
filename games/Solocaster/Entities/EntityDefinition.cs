using System.Collections.Generic;

namespace Solocaster.Entities;

public record EntityDefinition(
    string Name,
    string Type,
    int TileX,
    int TileY,
    IReadOnlyDictionary<string, string> Properties
);
