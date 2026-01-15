using Microsoft.Xna.Framework.Graphics;

namespace Solo.Assets;

public record SpriteSheet
{
    private readonly Dictionary<string, Sprite> _sprites = new();

    public SpriteSheet(string name, string spriteSheetName, Texture2D texture, IEnumerable<Sprite> sprites)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentException.ThrowIfNullOrEmpty(spriteSheetName, nameof(spriteSheetName));

        if (sprites is null || !sprites.Any())
            throw new ArgumentNullException(nameof(sprites));

        this.Name = name;
        this.ImagePath = spriteSheetName;
        this.Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        foreach (var sprite in sprites)
        {
            if(_sprites.ContainsKey(sprite.Name))
                throw new ArgumentException($"sprite with name '{sprite.Name}' already exists in the sprite sheet '{name}'");
            _sprites.Add(sprite.Name, sprite);
        }
    }

    public Sprite Get(string name)
    {
        if (!_sprites.TryGetValue(name, out var sprite) || sprite is null)
            throw new ArgumentException($"invalid sprite name: '{name}'");
        return sprite;
    }

    public string Name { get; }
    public string ImagePath { get; }
    public Texture2D Texture { get; }
}
