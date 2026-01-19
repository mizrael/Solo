using System;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Solocaster.Inventory;

namespace Solocaster.UI;

public static class UITheme
{
    private static UIThemeData _theme = new();

    public static PanelStyle Panel => _theme.Panel;
    public static PanelStyle Button => _theme.Button;
    public static PanelStyle ItemSlot => _theme.ItemSlot;
    public static PanelStyle Tooltip => _theme.Tooltip;
    public static TextColors Text => _theme.Text;
    public static SelectionColors Selection => _theme.Selection;
    public static StatusBarColors StatusBar => _theme.StatusBar;
    public static StatColors Stats => _theme.Stats;
    public static ScrollbarColors Scrollbar => _theme.Scrollbar;

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
                Tooltip = ParsePanelStyle(data.Tooltip),
                Text = ParseTextColors(data.Text),
                Selection = ParseSelectionColors(data.Selection),
                StatusBar = ParseStatusBarColors(data.StatusBar),
                Stats = ParseStatColors(data.Stats),
                Scrollbar = ParseScrollbarColors(data.Scrollbar)
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

    private static TextColors ParseTextColors(TextColorsJson? json)
    {
        if (json == null)
            return new TextColors();

        return new TextColors
        {
            Primary = ParseColor(json.Primary) ?? Color.White,
            Secondary = ParseColor(json.Secondary) ?? Color.LightGray,
            Muted = ParseColor(json.Muted) ?? Color.Gray,
            Error = ParseColor(json.Error) ?? Color.Red,
            Warning = ParseColor(json.Warning) ?? Color.Yellow,
            Title = ParseColor(json.Title) ?? new Color(220, 200, 160),
            Highlight = ParseColor(json.Highlight) ?? new Color(200, 180, 140),
            Subtitle = ParseColor(json.Subtitle) ?? new Color(160, 160, 160),
            SectionHeader = ParseColor(json.SectionHeader) ?? new Color(150, 150, 150),
            Placeholder = ParseColor(json.Placeholder) ?? new Color(100, 100, 100),
            Shadow = ParseColor(json.Shadow) ?? Color.Black
        };
    }

    private static SelectionColors ParseSelectionColors(SelectionColorsJson? json)
    {
        if (json == null)
            return new SelectionColors();

        return new SelectionColors
        {
            SelectedBackground = ParseColor(json.SelectedBackground) ?? new Color(80, 70, 50),
            HoverBackground = ParseColor(json.HoverBackground) ?? new Color(60, 60, 60),
            SelectionBorder = ParseColor(json.SelectionBorder) ?? new Color(200, 180, 140),
            HoverBorder = ParseColor(json.HoverBorder) ?? new Color(150, 150, 150, 180),
            InvalidDrop = ParseColor(json.InvalidDrop) ?? new Color(200, 50, 50, 180),
            ValidDrop = ParseColor(json.ValidDrop) ?? new Color(50, 200, 50, 180),
            EmptySlot = ParseColor(json.EmptySlot) ?? new Color(50, 50, 50, 150),
            SlotHover = ParseColor(json.SlotHover) ?? new Color(60, 60, 80, 220),
            CloseButton = ParseColor(json.CloseButton) ?? new Color(180, 60, 60)
        };
    }

    private static StatusBarColors ParseStatusBarColors(StatusBarColorsJson? json)
    {
        if (json == null)
            return new StatusBarColors();

        return new StatusBarColors
        {
            HealthFill = ParseColor(json.HealthFill) ?? new Color(180, 40, 40),
            HealthBackground = ParseColor(json.HealthBackground) ?? new Color(60, 20, 20),
            ManaFill = ParseColor(json.ManaFill) ?? new Color(40, 80, 180),
            ManaBackground = ParseColor(json.ManaBackground) ?? new Color(20, 30, 60),
            ProgressFill = ParseColor(json.ProgressFill) ?? new Color(80, 200, 80),
            ProgressBackground = ParseColor(json.ProgressBackground) ?? new Color(40, 40, 40)
        };
    }

    private static StatColors ParseStatColors(StatColorsJson? json)
    {
        if (json == null)
            return new StatColors();

        return new StatColors
        {
            Strength = ParseColor(json.Strength) ?? new Color(200, 80, 80),
            Agility = ParseColor(json.Agility) ?? new Color(80, 200, 80),
            Vitality = ParseColor(json.Vitality) ?? new Color(200, 160, 80),
            Intelligence = ParseColor(json.Intelligence) ?? new Color(80, 120, 200),
            Wisdom = ParseColor(json.Wisdom) ?? new Color(160, 80, 200)
        };
    }

    private static ScrollbarColors ParseScrollbarColors(ScrollbarColorsJson? json)
    {
        if (json == null)
            return new ScrollbarColors();

        return new ScrollbarColors
        {
            Track = ParseColor(json.Track) ?? new Color(40, 40, 40, 150),
            Thumb = ParseColor(json.Thumb) ?? new Color(120, 120, 120, 200)
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
        public TextColorsJson? Text { get; set; }
        public SelectionColorsJson? Selection { get; set; }
        public StatusBarColorsJson? StatusBar { get; set; }
        public StatColorsJson? Stats { get; set; }
        public ScrollbarColorsJson? Scrollbar { get; set; }
    }

    private class PanelStyleJson
    {
        public int[]? BackgroundColor { get; set; }
        public int[]? BorderColor { get; set; }
        public int? BorderWidth { get; set; }
        public int? ContentPadding { get; set; }
    }

    private class TextColorsJson
    {
        public int[]? Primary { get; set; }
        public int[]? Secondary { get; set; }
        public int[]? Muted { get; set; }
        public int[]? Error { get; set; }
        public int[]? Warning { get; set; }
        public int[]? Title { get; set; }
        public int[]? Highlight { get; set; }
        public int[]? Subtitle { get; set; }
        public int[]? SectionHeader { get; set; }
        public int[]? Placeholder { get; set; }
        public int[]? Shadow { get; set; }
    }

    private class SelectionColorsJson
    {
        public int[]? SelectedBackground { get; set; }
        public int[]? HoverBackground { get; set; }
        public int[]? SelectionBorder { get; set; }
        public int[]? HoverBorder { get; set; }
        public int[]? InvalidDrop { get; set; }
        public int[]? ValidDrop { get; set; }
        public int[]? EmptySlot { get; set; }
        public int[]? SlotHover { get; set; }
        public int[]? CloseButton { get; set; }
    }

    private class StatusBarColorsJson
    {
        public int[]? HealthFill { get; set; }
        public int[]? HealthBackground { get; set; }
        public int[]? ManaFill { get; set; }
        public int[]? ManaBackground { get; set; }
        public int[]? ProgressFill { get; set; }
        public int[]? ProgressBackground { get; set; }
    }

    private class StatColorsJson
    {
        public int[]? Strength { get; set; }
        public int[]? Agility { get; set; }
        public int[]? Vitality { get; set; }
        public int[]? Intelligence { get; set; }
        public int[]? Wisdom { get; set; }
    }

    private class ScrollbarColorsJson
    {
        public int[]? Track { get; set; }
        public int[]? Thumb { get; set; }
    }
}

public class UIThemeData
{
    public PanelStyle Panel { get; set; } = new();
    public PanelStyle Button { get; set; } = new();
    public PanelStyle ItemSlot { get; set; } = new();
    public PanelStyle Tooltip { get; set; } = new();
    public TextColors Text { get; set; } = new();
    public SelectionColors Selection { get; set; } = new();
    public StatusBarColors StatusBar { get; set; } = new();
    public StatColors Stats { get; set; } = new();
    public ScrollbarColors Scrollbar { get; set; } = new();
}

public class PanelStyle
{
    public Color BackgroundColor { get; set; } = new Color(40, 40, 40, 220);
    public Color BorderColor { get; set; } = new Color(80, 80, 80);
    public int BorderWidth { get; set; } = 2;
    public int ContentPadding { get; set; } = 16;
}

public class TextColors
{
    public Color Primary { get; set; } = Color.White;
    public Color Secondary { get; set; } = Color.LightGray;
    public Color Muted { get; set; } = Color.Gray;
    public Color Error { get; set; } = Color.Red;
    public Color Warning { get; set; } = Color.Yellow;
    public Color Title { get; set; } = new Color(220, 200, 160);
    public Color Highlight { get; set; } = new Color(200, 180, 140);
    public Color Subtitle { get; set; } = new Color(160, 160, 160);
    public Color SectionHeader { get; set; } = new Color(150, 150, 150);
    public Color Placeholder { get; set; } = new Color(100, 100, 100);
    public Color Shadow { get; set; } = Color.Black;
}

public class SelectionColors
{
    public Color SelectedBackground { get; set; } = new Color(80, 70, 50);
    public Color HoverBackground { get; set; } = new Color(60, 60, 60);
    public Color SelectionBorder { get; set; } = new Color(200, 180, 140);
    public Color HoverBorder { get; set; } = new Color(150, 150, 150, 180);
    public Color InvalidDrop { get; set; } = new Color(200, 50, 50, 180);
    public Color ValidDrop { get; set; } = new Color(50, 200, 50, 180);
    public Color EmptySlot { get; set; } = new Color(50, 50, 50, 150);
    public Color SlotHover { get; set; } = new Color(60, 60, 80, 220);
    public Color CloseButton { get; set; } = new Color(180, 60, 60);
}

public class StatusBarColors
{
    public Color HealthFill { get; set; } = new Color(180, 40, 40);
    public Color HealthBackground { get; set; } = new Color(60, 20, 20);
    public Color ManaFill { get; set; } = new Color(40, 80, 180);
    public Color ManaBackground { get; set; } = new Color(20, 30, 60);
    public Color ProgressFill { get; set; } = new Color(80, 200, 80);
    public Color ProgressBackground { get; set; } = new Color(40, 40, 40);
}

public class StatColors
{
    public Color Strength { get; set; } = new Color(200, 80, 80);
    public Color Agility { get; set; } = new Color(80, 200, 80);
    public Color Vitality { get; set; } = new Color(200, 160, 80);
    public Color Intelligence { get; set; } = new Color(80, 120, 200);
    public Color Wisdom { get; set; } = new Color(160, 80, 200);

    public Color GetColorForStat(StatType stat)
    {
        return stat switch
        {
            StatType.Strength => Strength,
            StatType.Agility => Agility,
            StatType.Vitality => Vitality,
            StatType.Intelligence => Intelligence,
            StatType.Wisdom => Wisdom,
            _ => Color.Gray
        };
    }
}

public class ScrollbarColors
{
    public Color Track { get; set; } = new Color(40, 40, 40, 150);
    public Color Thumb { get; set; } = new Color(120, 120, 120, 200);
}
