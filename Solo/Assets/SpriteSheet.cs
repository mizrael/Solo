using Microsoft.Xna.Framework.Graphics;

namespace Solo.Assets;

public record SpriteSheet
{
    private readonly Dictionary<string, Sprite> _spritesByName = new();
    private readonly Sprite[] _sprites;

    public SpriteSheet(string name, Texture2D texture, IEnumerable<Sprite> sprites)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        if (sprites is null || !sprites.Any())
            throw new ArgumentNullException(nameof(sprites));

        this.Name = name;
        this.Texture = texture ?? throw new ArgumentNullException(nameof(texture));

        _sprites = sprites.ToArray();
        foreach (var sprite in _sprites)
        {
            if(_spritesByName.ContainsKey(sprite.Name))
                throw new ArgumentException($"sprite with name '{sprite.Name}' already exists in the sprite sheet '{name}'");
            _spritesByName.Add(sprite.Name, sprite);
        }
    }

    public Sprite Get(string name)
    {
        if (!_spritesByName.TryGetValue(name, out var sprite) || sprite is null)
            throw new ArgumentException($"invalid sprite name: '{name}'");
        return sprite;
    }

    public IReadOnlyList<Sprite> Sprites => _sprites;

    public string Name { get; }
    public Texture2D Texture { get; }
}
