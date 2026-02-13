using System;

namespace Solo;

public readonly record struct GameObjectName
{
    public string Value { get; }

    public GameObjectName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public override string ToString() => Value;

    public static string BuildPath(GameObjectName[] segments)
    {
        if (segments.Length == 0)
            return string.Empty;

        var totalLength = segments.Length - 1;
        foreach (var segment in segments)
            totalLength += segment.Value.Length;

        return string.Create(totalLength, segments, static (span, segs) =>
        {
            var pos = 0;
            for (int i = 0; i < segs.Length; i++)
            {
                if (i > 0)
                    span[pos++] = '/';
                segs[i].Value.AsSpan().CopyTo(span[pos..]);
                pos += segs[i].Value.Length;
            }
        });
    }

    public static GameObjectName[] ParsePath(string path)
    {
        var parts = path.Split('/');
        var result = new GameObjectName[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            result[i] = new GameObjectName(parts[i]);
        return result;
    }
}
