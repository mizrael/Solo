using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text.Json;

namespace Solo.UI;

public static class UITheme
{
    private const float ReferenceHeight = 1080f;

    private static UIThemeData _theme = new();
    private static float _uiScale = 1f;
    private static Matrix _uiScaleMatrix = Matrix.Identity;
    private static Dictionary<string, Color> _customColors = new();
    private static Dictionary<string, float> _customFloats = new();

    public static float UIScale => _uiScale;
    public static Matrix UIScaleMatrix => _uiScaleMatrix;

    public static void UpdateUIScale(int viewportHeight)
    {
        _uiScale = Math.Min(1f, viewportHeight / ReferenceHeight);
        _uiScaleMatrix = Matrix.CreateScale(_uiScale, _uiScale, 1f);
    }

    public static SpriteFont Font => _theme.Fonts.Default;
    public static SpriteFont TooltipFont => _theme.Fonts.Tooltip;
    public static SpriteFont TitleFont => _theme.Fonts.Title;

    public static int LineHeight => Font.LineSpacing;
    public static int MenuItemHeight => LineHeight + 16;
    public static int BreadcrumbHeight => (int)(Font.LineSpacing * 1.3f) + 8 + 1;
    public static int LabelRowHeight => LineHeight + 8;
    public static int ButtonRowHeight => LineHeight + 24;
    public static int SectionSpacing => Math.Max(8, LineHeight / 2);

    public static PanelStyle Panel => _theme.Panel;
    public static ButtonStyle Button => _theme.Button;
    public static PanelStyle Input => _theme.Input;
    public static PanelStyle Tooltip => _theme.Tooltip;
    public static TextColors Text => _theme.Text;
    public static SelectionColors Selection => _theme.Selection;
    public static StatusBarColors StatusBar => _theme.StatusBar;
    public static ScrollbarColors Scrollbar => _theme.Scrollbar;

    public static Color GetColor(string key, Color? defaultValue = null)
    {
        if (_customColors.TryGetValue(key, out var color))
            return color;
        return defaultValue ?? Color.White;
    }

    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (_customFloats.TryGetValue(key, out var value))
            return value;
        return defaultValue;
    }

    private const string DefaultFontAsset = "Font";

    public static void Load(string path, ContentManager content)
    {
        UIThemeJson? data = null;

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            data = JsonSerializer.Deserialize<UIThemeJson>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        else
        {
            Console.WriteLine($"UITheme: Config file not found at {path}, using defaults");
        }

        var fonts = LoadFonts(content, data?.Fonts);

        if (data != null)
        {
            _theme = new UIThemeData
            {
                Fonts = fonts,
                Panel = ParsePanelStyle(data.Panel),
                Button = ParseButtonStyle(data.Button),
                Input = ParsePanelStyle(data.Input),
                Tooltip = ParsePanelStyle(data.Tooltip),
                Text = ParseTextColors(data.Text),
                Selection = ParseSelectionColors(data.Selection),
                StatusBar = ParseStatusBarColors(data.StatusBar),
                Scrollbar = ParseScrollbarColors(data.Scrollbar)
            };

            ParseCustomSection(data.Custom);
        }
        else
        {
            _theme = new UIThemeData { Fonts = fonts };
        }
    }

    private static ThemeFonts LoadFonts(ContentManager content, FontsJson? fonts)
    {
        var defaultAsset = fonts?.Default ?? DefaultFontAsset;
        return new ThemeFonts
        {
            Default = content.Load<SpriteFont>(defaultAsset),
            Tooltip = content.Load<SpriteFont>(fonts?.Tooltip ?? defaultAsset),
            Title = content.Load<SpriteFont>(fonts?.Title ?? defaultAsset)
        };
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

    private static ButtonStyle ParseButtonStyle(ButtonStyleJson? json)
    {
        if (json == null)
            return new ButtonStyle();

        return new ButtonStyle
        {
            BackgroundColor = ParseColor(json.BackgroundColor) ?? new Color(60, 60, 60, 230),
            BorderColor = ParseColor(json.BorderColor) ?? new Color(100, 100, 100),
            BorderWidth = json.BorderWidth ?? 2,
            ContentPadding = json.ContentPadding ?? 0,
            HoverBackgroundColor = ParseColor(json.HoverBackgroundColor) ?? new Color(80, 75, 65, 240),
            HoverBorderColor = ParseColor(json.HoverBorderColor) ?? new Color(200, 180, 140),
            DisabledBackgroundColor = ParseColor(json.DisabledBackgroundColor) ?? new Color(35, 35, 35, 200),
            DisabledBorderColor = ParseColor(json.DisabledBorderColor) ?? new Color(50, 50, 50)
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
            ProgressFill = ParseColor(json.ProgressFill) ?? new Color(80, 200, 80),
            ProgressBackground = ParseColor(json.ProgressBackground) ?? new Color(40, 40, 40)
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

    private static void ParseCustomSection(Dictionary<string, Dictionary<string, int[]>>? custom)
    {
        _customColors.Clear();
        _customFloats.Clear();

        if (custom == null)
            return;

        foreach (var section in custom)
        {
            foreach (var entry in section.Value)
            {
                var key = $"{section.Key}.{entry.Key}";
                var color = ParseColor(entry.Value);
                if (color.HasValue)
                    _customColors[key] = color.Value;
            }
        }
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

    private class UIThemeJson
    {
        public FontsJson? Fonts { get; set; }
        public PanelStyleJson? Panel { get; set; }
        public ButtonStyleJson? Button { get; set; }
        public PanelStyleJson? Input { get; set; }
        public PanelStyleJson? Tooltip { get; set; }
        public TextColorsJson? Text { get; set; }
        public SelectionColorsJson? Selection { get; set; }
        public StatusBarColorsJson? StatusBar { get; set; }
        public ScrollbarColorsJson? Scrollbar { get; set; }
        public Dictionary<string, Dictionary<string, int[]>>? Custom { get; set; }
    }

    private class FontsJson
    {
        public string? Default { get; set; }
        public string? Tooltip { get; set; }
        public string? Title { get; set; }
    }

    private class PanelStyleJson
    {
        public int[]? BackgroundColor { get; set; }
        public int[]? BorderColor { get; set; }
        public int? BorderWidth { get; set; }
        public int? ContentPadding { get; set; }
    }

    private class ButtonStyleJson : PanelStyleJson
    {
        public int[]? HoverBackgroundColor { get; set; }
        public int[]? HoverBorderColor { get; set; }
        public int[]? DisabledBackgroundColor { get; set; }
        public int[]? DisabledBorderColor { get; set; }
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
        public int[]? ProgressFill { get; set; }
        public int[]? ProgressBackground { get; set; }
    }

    private class ScrollbarColorsJson
    {
        public int[]? Track { get; set; }
        public int[]? Thumb { get; set; }
    }
}

public class UIThemeData
{
    public ThemeFonts Fonts { get; set; } = new();
    public PanelStyle Panel { get; set; } = new();
    public ButtonStyle Button { get; set; } = new();
    public PanelStyle Input { get; set; } = new();
    public PanelStyle Tooltip { get; set; } = new();
    public TextColors Text { get; set; } = new();
    public SelectionColors Selection { get; set; } = new();
    public StatusBarColors StatusBar { get; set; } = new();
    public ScrollbarColors Scrollbar { get; set; } = new();
}

public class ThemeFonts
{
    public SpriteFont Default { get; set; } = null!;
    public SpriteFont Tooltip { get; set; } = null!;
    public SpriteFont Title { get; set; } = null!;
}

public class PanelStyle
{
    public Color BackgroundColor { get; set; } = new Color(40, 40, 40, 220);
    public Color BorderColor { get; set; } = new Color(80, 80, 80);
    public int BorderWidth { get; set; } = 2;
    public int ContentPadding { get; set; } = 16;
}

public class ButtonStyle : PanelStyle
{
    public Color HoverBackgroundColor { get; set; } = new Color(80, 75, 65, 240);
    public Color HoverBorderColor { get; set; } = new Color(200, 180, 140);
    public Color DisabledBackgroundColor { get; set; } = new Color(35, 35, 35, 200);
    public Color DisabledBorderColor { get; set; } = new Color(50, 50, 50);
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
    public Color ProgressFill { get; set; } = new Color(80, 200, 80);
    public Color ProgressBackground { get; set; } = new Color(40, 40, 40);
}

public class ScrollbarColors
{
    public Color Track { get; set; } = new Color(40, 40, 40, 150);
    public Color Thumb { get; set; } = new Color(120, 120, 120, 200);
}
