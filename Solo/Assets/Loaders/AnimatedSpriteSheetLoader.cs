using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;

namespace Solo.Assets.Loaders;

public class AnimatedSpriteSheetLoader
{
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    private readonly static Dictionary<string, AnimatedSpriteSheet> _cache = new();

    public static AnimatedSpriteSheet Load(string assetPath, Game game)
    {
        if(_cache.TryGetValue(assetPath, out var cached))
            return cached;

        var json = File.ReadAllText(assetPath);
        var dto = JsonSerializer.Deserialize<animDto>(json, _jsonOptions);

        var texture = game.Content.Load<Texture2D>(dto!.spriteSheetName);
        var result = new AnimatedSpriteSheet(
            dto.animationName, 
            texture, 
            dto.fps, 
            dto.frames.Select(f => new AnimatedSpriteSheet.Frame(new Rectangle(f.x, f.y, f.width, f.height))).ToArray());
        _cache.Add(assetPath, result);
        return result;
    }

    internal class animDto
    {
        public string animationName { get; set; }
        public string spriteSheetName { get; set; }
        public int fps { get; set; }
        public frameDto[] frames { get; set; }

        internal class frameDto { 
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }
    }
}