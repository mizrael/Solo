using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Character;
using Solocaster.Inventory;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class SummaryStepPanel : Widget
{
    public SummaryStepPanel(SpriteFont font, Game game, Vector2 size)
    {
        Size = size;

        var character = GameState.CurrentCharacter!;

        RaceTemplate? race = null;
        ClassTemplate? cls = null;
        CharacterTemplateLoader.TryGetRace(character.RaceId, out race);
        CharacterTemplateLoader.TryGetClass(character.ClassId, out cls);

        int leftX = 20;
        int rightX = 200;
        int y = 20;
        int lineHeight = 28;

        // Avatar
        try
        {
            var spriteSheet = SpriteSheetLoader.Get("avatars", game);
            var sprite = spriteSheet.Get(character.AvatarSpriteName);

            var avatarWidget = new ImageWidget
            {
                Texture = sprite.Texture,
                SourceRectangle = sprite.Bounds,
                ScaleToFit = true,
                Position = new Vector2(leftX, y),
                Size = new Vector2(120, 120)
            };
            AddChild(avatarWidget);
        }
        catch { }

        // Character info on right
        int infoY = y;

        var nameLabel = new LabelWidget
        {
            Text = character.Name,
            Font = font,
            TextColor = new Color(220, 200, 160),
            Position = new Vector2(rightX, infoY),
            Size = new Vector2(size.X - rightX - 20, lineHeight)
        };
        AddChild(nameLabel);
        infoY += lineHeight;

        var raceClassLabel = new LabelWidget
        {
            Text = $"{race?.Name ?? character.RaceId} {cls?.Name ?? character.ClassId}",
            Font = font,
            TextColor = Color.LightGray,
            Position = new Vector2(rightX, infoY),
            Size = new Vector2(size.X - rightX - 20, lineHeight)
        };
        AddChild(raceClassLabel);
        infoY += lineHeight;

        var sexLabel = new LabelWidget
        {
            Text = character.Sex.ToString(),
            Font = font,
            TextColor = Color.LightGray,
            Position = new Vector2(rightX, infoY),
            Size = new Vector2(size.X - rightX - 20, lineHeight)
        };
        AddChild(sexLabel);

        // Stats section
        y = 160;

        var statsHeader = new LabelWidget
        {
            Text = "Starting Stats:",
            Font = font,
            TextColor = new Color(200, 180, 140),
            Position = new Vector2(leftX, y),
            Size = new Vector2(size.X - 40, lineHeight)
        };
        AddChild(statsHeader);
        y += lineHeight + 5;

        // Calculate combined stats
        var stats = new[] { StatType.Strength, StatType.Agility, StatType.Vitality, StatType.Intelligence, StatType.Wisdom };

        foreach (var stat in stats)
        {
            int baseValue = 10;
            int raceBonus = (int)(race?.StatBonuses.GetValueOrDefault(stat) ?? 0);
            int classBonus = (int)(cls?.StatBonuses.GetValueOrDefault(stat) ?? 0);
            int total = baseValue + raceBonus + classBonus;

            string bonusText = "";
            if (raceBonus != 0 || classBonus != 0)
            {
                var parts = new List<string>();
                if (raceBonus != 0) parts.Add($"{(raceBonus >= 0 ? "+" : "")}{raceBonus} race");
                if (classBonus != 0) parts.Add($"{(classBonus >= 0 ? "+" : "")}{classBonus} class");
                bonusText = $" ({string.Join(", ", parts)})";
            }

            var statLabel = new LabelWidget
            {
                Text = $"{FormatStatName(stat)}: {total}{bonusText}",
                Font = font,
                TextColor = Color.White,
                Position = new Vector2(leftX + 20, y),
                Size = new Vector2(size.X - 60, lineHeight)
            };
            AddChild(statLabel);
            y += lineHeight;
        }
    }

    private static string FormatStatName(StatType stat)
    {
        return stat switch
        {
            StatType.Strength => "STR",
            StatType.Agility => "AGI",
            StatType.Vitality => "VIT",
            StatType.Intelligence => "INT",
            StatType.Wisdom => "WIS",
            _ => stat.ToString()
        };
    }
}
