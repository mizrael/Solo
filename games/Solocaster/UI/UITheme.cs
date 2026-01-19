using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace Solocaster.UI;

public static class UITheme
{
    private static UIThemeData _theme = new();

    public static PanelStyle Panel => _theme.Panel;
    public static PanelStyle Button => _theme.Button;
    public static PanelStyle ItemSlot => _theme.ItemSlot;
    public static PanelStyle Tooltip => _theme.Tooltip;

    public static void Load(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"UITheme: Config file not found at {path}, using defaults");
            return;
        }

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<UIThemeJson>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data != null)
        {
            _theme = new UIThemeData
            {
                Panel = ParsePanelStyle(data.Panel),
                Button = ParsePanelStyle(data.Button),
                ItemSlot = ParsePanelStyle(data.ItemSlot),
                Tooltip = ParsePanelStyle(data.Tooltip)
            };
        }
    }

    private static PanelStyle ParsePanelStyle(PanelStyleJson? json)
    {
        if (json == null)
            return new PanelStyle();

        return new PanelStyle
        {
            BackgroundColor = ParseColor(json.BackgroundColor) ?? new Color(40, 40, 40, 220),
            BorderColor = ParseColor(json.BorderColor) ?? new Color(80, 80, 80),
            BorderWidth = json.BorderWidth ?? 2,
            ContentPadding = json.ContentPadding ?? 16
        };
    }

    private static Color? ParseColor(int[]? rgba)
    {
        if (rgba == null || rgba.Length < 3)
            return null;

        int r = rgba[0];
        int g = rgba[1];
        int b = rgba[2];
        int a = rgba.Length >= 4 ? rgba[3] : 255;

        return new Color(r, g, b, a);
    }

    // JSON deserialization classes
    private class UIThemeJson
    {
        public PanelStyleJson? Panel { get; set; }
        public PanelStyleJson? Button { get; set; }
        public PanelStyleJson? ItemSlot { get; set; }
        public PanelStyleJson? Tooltip { get; set; }
    }

    private class PanelStyleJson
    {
        public int[]? BackgroundColor { get; set; }
        public int[]? BorderColor { get; set; }
        public int? BorderWidth { get; set; }
        public int? ContentPadding { get; set; }
    }
}

public class UIThemeData
{
    public PanelStyle Panel { get; set; } = new();
    public PanelStyle Button { get; set; } = new();
    public PanelStyle ItemSlot { get; set; } = new();
    public PanelStyle Tooltip { get; set; } = new();
}

public class PanelStyle
{
    public Color BackgroundColor { get; set; } = new Color(40, 40, 40, 220);
    public Color BorderColor { get; set; } = new Color(80, 80, 80);
    public int BorderWidth { get; set; } = 2;
    public int ContentPadding { get; set; } = 16;
}
