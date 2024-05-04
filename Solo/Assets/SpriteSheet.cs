namespace Solo.Assets;

public record SpriteSheet
{
    private readonly Dictionary<string, Sprite> _sprites = new();

    public SpriteSheet(string name, string spriteSheetName, IEnumerable<Sprite> sprites)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(spriteSheetName))
            throw new ArgumentNullException(nameof(spriteSheetName));

        if (sprites is null || !sprites.Any())
            throw new ArgumentNullException(nameof(sprites));

        this.Name = name;
        this.ImagePath = spriteSheetName;

        foreach (var sprite in sprites)
            _sprites.Add(sprite.Name, sprite);
    }

    public Sprite Get(string name)
    {
        if (!_sprites.TryGetValue(name, out var sprite) || sprite is null)
            throw new ArgumentException($"invalid sprite name: '{name}'");
        return sprite;
    }

    public string Name { get; }
    public string ImagePath { get; }
}
