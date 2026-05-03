using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solo.UI.Widgets;

/// <summary>
/// Generic tab container. Renders a horizontal tab strip on top and the active tab's
/// content widget below. Tab switching is exposed via <see cref="ActiveIndex"/>,
/// <see cref="Next"/>, and <see cref="Previous"/>; mouse clicks on the strip jump
/// directly to a tab. Subclasses can intercept switches by overriding
/// <see cref="OnTabActivating"/>.
/// </summary>
public class TabbedPanelWidget : Widget
{
    private const int TabStripVerticalPadding = 6;
    private const int TabStripHorizontalPadding = 12;

    private readonly IReadOnlyList<TabPage> _tabs;
    private int _activeIndex;

    /// <summary>Raised after a successful tab switch with (previousIndex, newIndex).</summary>
    public event Action<int, int>? ActiveTabChanged;

    public TabbedPanelWidget(IReadOnlyList<TabPage> tabs)
    {
        if (tabs == null) throw new ArgumentNullException(nameof(tabs));
        if (tabs.Count == 0) throw new ArgumentException("TabbedPanelWidget requires at least one tab.", nameof(tabs));

        // Snapshot so subsequent mutations to a caller-owned List<TabPage> cannot
        // desync our Children/visibility state from the public Tabs collection.
        var snapshot = new TabPage[tabs.Count];
        var seen = new HashSet<Widget>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            if (!seen.Add(tab.Content))
                throw new ArgumentException(
                    "TabPage Content widgets must be distinct instances; AddChild reparents shared widgets and would leave tabs blank.",
                    nameof(tabs));
            snapshot[i] = tab;
        }
        _tabs = snapshot;

        for (int i = 0; i < _tabs.Count; i++)
        {
            var content = _tabs[i].Content;
            content.Visible = (i == 0);
            AddChild(content);
        }
    }

    /// <summary>Currently active tab. Setting raises <see cref="ActiveTabChanged"/>
    /// (unless cancelled via <see cref="OnTabActivating"/> or already current).</summary>
    public int ActiveIndex
    {
        get => _activeIndex;
        set
        {
            if (value < 0 || value >= _tabs.Count)
                throw new ArgumentOutOfRangeException(nameof(value), value,
                    $"ActiveIndex must be in [0, {_tabs.Count - 1}].");
            if (value == _activeIndex)
                return;

            int previous = _activeIndex;
            if (!OnTabActivating(previous, value))
                return;

            _tabs[previous].Content.Visible = false;
            _tabs[value].Content.Visible = true;
            _activeIndex = value;
            ActiveTabChanged?.Invoke(previous, value);
        }
    }

    public IReadOnlyList<TabPage> Tabs => _tabs;

    /// <summary>Switches to the next tab. No wrap; clamped at the last index.</summary>
    public void Next() => ActiveIndex = Math.Min(_activeIndex + 1, _tabs.Count - 1);

    /// <summary>Switches to the previous tab. No wrap; clamped at the first index.</summary>
    public void Previous() => ActiveIndex = Math.Max(_activeIndex - 1, 0);

    /// <summary>Hook called before <see cref="ActiveIndex"/> changes. Return false to
    /// cancel the switch. Default returns true.</summary>
    protected virtual bool OnTabActivating(int previousIndex, int nextIndex) => true;

    private int TabStripHeight => UITheme.LineHeight + TabStripVerticalPadding * 2;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        float contentAvailableHeight = Math.Max(0, availableHeight - TabStripHeight);
        float maxW = 0;
        float maxH = 0;
        foreach (var tab in _tabs)
        {
            tab.Content.Measure(availableWidth, contentAvailableHeight);
            maxW = Math.Max(maxW, tab.Content.DesiredSize.X);
            maxH = Math.Max(maxH, tab.Content.DesiredSize.Y);
        }

        // Ensure the requested width is at least wide enough to fit every tab title in
        // the strip; otherwise narrow content + long titles would clip/overlap labels.
        float titleStripWidth = MeasureTabStripWidth();
        float requestedWidth = Math.Max(maxW, titleStripWidth);

        return new Vector2(requestedWidth, maxH + TabStripHeight);
    }

    private float MeasureTabStripWidth()
    {
        try
        {
            var font = UITheme.Font;
            if (font == null)
                return 0;
            float maxTitle = 0;
            foreach (var tab in _tabs)
                maxTitle = Math.Max(maxTitle, font.MeasureString(tab.Title).X);
            return (maxTitle + TabStripHorizontalPadding * 2) * _tabs.Count;
        }
        catch (NullReferenceException)
        {
            // Headless test contexts may not have a Font configured; fall back to
            // letting content drive width. Real rendering paths always have a Font.
            return 0;
        }
    }

    protected override void ArrangeCore(Vector2 finalSize)
    {
        foreach (var tab in _tabs)
        {
            tab.Content.Position = new Vector2(0, TabStripHeight);
            tab.Content.Arrange(tab.Content.DesiredSize);
        }
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        bool clicked = mouseState.LeftButton == ButtonState.Pressed
            && previousMouseState.LeftButton == ButtonState.Released;
        if (!clicked)
            return;

        var mousePoint = new Point(mouseState.X, mouseState.Y);
        if (IsInteractionClipped(mousePoint))
            return;

        int? hit = HitTestTabStrip(mouseState.X, mouseState.Y);
        if (hit.HasValue)
            ActiveIndex = hit.Value;
    }

    private int? HitTestTabStrip(int mouseX, int mouseY)
    {
        var screenPos = ScreenPosition;
        var tabAreaBounds = new Rectangle(
            (int)screenPos.X,
            (int)screenPos.Y,
            (int)Size.X,
            TabStripHeight);
        if (!tabAreaBounds.Contains(mouseX, mouseY))
            return null;

        int totalWidth = (int)Size.X;
        int baseTabWidth = totalWidth / _tabs.Count;
        if (baseTabWidth <= 0)
            return null;

        int relativeX = mouseX - tabAreaBounds.X;
        // Last tab absorbs the remainder pixels (see RenderTabStrip), so any click
        // beyond `baseTabWidth * (count - 1)` belongs to the last tab.
        int lastTabStart = baseTabWidth * (_tabs.Count - 1);
        int index = relativeX >= lastTabStart ? _tabs.Count - 1 : relativeX / baseTabWidth;
        return Math.Clamp(index, 0, _tabs.Count - 1);
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;
        RenderTabStrip(spriteBatch);
        RenderCore(spriteBatch);
        // Children render themselves; only the active tab's Content has Visible = true.
        foreach (var child in Children)
            child.Render(spriteBatch);
    }

    private void RenderTabStrip(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var font = UITheme.Font;
        var screenPos = ScreenPosition;
        int stripHeight = TabStripHeight;
        int totalWidth = (int)Size.X;
        int baseTabWidth = totalWidth / _tabs.Count;
        if (baseTabWidth <= 0)
            return;

        for (int i = 0; i < _tabs.Count; i++)
        {
            // Last tab absorbs the integer-truncation remainder so the strip paints
            // edge-to-edge and matches HitTestTabStrip exactly (no gap, no dead zone).
            int x = (int)screenPos.X + i * baseTabWidth;
            int width = (i == _tabs.Count - 1) ? totalWidth - i * baseTabWidth : baseTabWidth;
            var tabBounds = new Rectangle(x, (int)screenPos.Y, width, stripHeight);

            bool isActive = (i == _activeIndex);
            var bgColor = isActive ? UITheme.Selection.SelectedBackground : UITheme.Selection.HoverBackground;
            spriteBatch.Draw(pixel, tabBounds, bgColor);

            // Bottom border highlight on active tab so it visually attaches to content.
            if (isActive)
            {
                var underline = new Rectangle(tabBounds.X, tabBounds.Bottom - 2, tabBounds.Width, 2);
                spriteBatch.Draw(pixel, underline, UITheme.Selection.SelectionBorder);
            }

            var title = _tabs[i].Title;
            var titleSize = font.MeasureString(title);
            var textPos = new Vector2(
                tabBounds.X + (tabBounds.Width - titleSize.X) / 2f,
                tabBounds.Y + (tabBounds.Height - titleSize.Y) / 2f);
            var textColor = isActive ? UITheme.Text.Primary : UITheme.Text.Muted;
            spriteBatch.DrawString(font, title, textPos, textColor);
        }
    }
}
