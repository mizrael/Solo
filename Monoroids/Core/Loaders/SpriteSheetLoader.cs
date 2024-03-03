using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core.Assets;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Monoroids.Core.Loaders
{
    public class SpriteSheetLoader
    {
        public SpriteSheet Load(string assetPath, Game game)
        {
            var json = File.ReadAllText(assetPath);
            var dto = JsonSerializer.Deserialize<SpriteSheetDTO>(json, new JsonSerializerOptions()
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            });

            var texture = game.Content.Load<Texture2D>(dto.spriteSheetName);

            var sprites = dto.sprites
                .Select(s => new Sprite(s.name, new Rectangle(s.x, s.y, s.width, s.height), texture))
                .ToArray();

            return new SpriteSheet(assetPath, dto.spriteSheetName, sprites);
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
}
