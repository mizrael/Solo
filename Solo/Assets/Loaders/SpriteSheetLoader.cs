using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;

namespace Solo.Assets.Loaders;

public class SpriteSheetLoader
{
    private static JsonSerializerOptions options = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    private static Dictionary<string, SpriteSheet> _cache = new();

    public static SpriteSheet Get(string name, Game game)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (!_cache.TryGetValue(name, out var spriteSheet))
        {
            spriteSheet = LoadInternal(name, game);
            _cache[name] = spriteSheet;
        }

        return _cache[name];
    }

    private static SpriteSheet LoadInternal(string name, Game game)
    {
        var assetPath = Path.Combine(BasePath, name + ".json");

        var json = File.ReadAllText(assetPath);
        var dto = JsonSerializer.Deserialize<SpriteSheetDTO>(json, options);

        var texture = game.Content.Load<Texture2D>(dto!.spriteSheetName);

        var sprites = dto.sprites
            .Select(s => new Sprite(s.name, texture, new Rectangle(s.x, s.y, s.width, s.height)))
            .ToArray();

        return new SpriteSheet(name: dto.spriteSheetName, texture, sprites);
    }

    internal class SpriteSheetDTO
    {
        public string spriteSheetName { get; set; }

        public SpriteDTO[] sprites { get; set; }

        internal class SpriteDTO
        {
            public string name { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }
    }

    public static string BasePath = "./data/spritesheets/";
}
