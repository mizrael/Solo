using System.Collections.Generic;
using Solocaster.Components;
using Solocaster.Inventory;

namespace Solocaster.Monsters;

public class MonsterTemplate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public Dictionary<StatType, float> Stats { get; init; } = new();
    public MonsterBehavior Behavior { get; init; } = new();
    public string SpritesheetBasePath { get; init; } = string.Empty;
    public float Scale { get; init; } = 1.0f;
    public BillboardAnchor Anchor { get; init; } = BillboardAnchor.Bottom;
}

public class MonsterBehavior
{
    public float DetectionRange { get; init; } = 8.0f;
    public float AttackRange { get; init; } = 1.2f;
    public float MoveSpeed { get; init; } = 2.0f;
}
