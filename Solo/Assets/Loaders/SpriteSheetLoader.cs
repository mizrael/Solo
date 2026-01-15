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

    public static SpriteSheet Load(string assetPath, Game game)
    {
        var json = File.ReadAllText(assetPath);
        var dto = JsonSerializer.Deserialize<SpriteSheetDTO>(json, options);

        var texture = game.Content.Load<Texture2D>(dto!.spriteSheetName);

        var sprites = dto.sprites
            .Select(s => new Sprite(s.name, texture, new Rectangle(s.x, s.y, s.width, s.height)))
            .ToArray();

        return new SpriteSheet(assetPath, dto.spriteSheetName, texture, sprites);
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
}
