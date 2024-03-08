using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Text.Json;

namespace Monoroids.Core.Assets.Loaders;

public class AnimationLoader
{
    public Animation Load(string assetPath, Game game)
    {
        var json = File.ReadAllText(assetPath);
        var dto = JsonSerializer.Deserialize<AnimationDTO>(json, new JsonSerializerOptions()
        {
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        });

        var texture = game.Content.Load<Texture2D>(dto!.asset);
        return new Animation(texture, assetPath, dto.fps, dto.framesCount, new Point(dto.frameWidth, dto.frameHeight));
    }

    internal class AnimationDTO
    {
        public string asset { get; set; }
        public int fps { get; set; }
        public int framesCount { get; set; }
        public int frameWidth { get; set; }
        public int frameHeight { get; set; }        
    }
}