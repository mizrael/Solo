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
///
/// When <see cref="Scrollable"/> is true the tab strip stays pinned to the top of the
/// widget and the active tab content scrolls within the remaining vertical space, with
/// a vertical scrollbar painted in the right gutter (mirroring <see cref="PanelWidget"/>).
/// </summary>
public class TabbedPanelWidget : Widget
{
    private const int TabStripVerticalPadding = 6;
    private const int TabStripHorizontalPadding = 12;
    private const int ScrollSpeed = 30;

    /// <summary>Shared scissor-test-enabled rasterizer state for scrollable tabbed panels.
    /// Mirrors the equivalent constant in <see cref="PanelWidget"/> so we don't re-create
    /// driver-side state on every frame.</summary>
    private static readonly RasterizerState ScissorEnabledRasterizer = new() { ScissorTestEnable = true };

    private readonly IReadOnlyList<TabPage> _tabs;
    private int _activeIndex;
    private float _scrollOffset;
    private int _previousScrollWheelValue;
    private bool _scrollWheelInitialized;

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
            // Reset scroll so the new tab starts at its top instead of inheriting
            // the previous tab's scroll position.
            _scrollOffset = 0;
            ActiveTabChanged?.Invoke(previous, value);
        }
    }

    public IReadOnlyList<TabPage> Tabs => _tabs;

    /// <summary>
    /// When true, the active tab content scrolls vertically inside the area below the
    /// tab strip and a scrollbar is painted in the right gutter. Children should NOT
    /// also be Scrollable when this is enabled — the tabbed container owns scrolling.
    /// </summary>
    public bool Scrollable { get; set; }

    /// <summary>
    /// Inner horizontal padding (in logical UI pixels) applied between the panel's
    /// left/right edges and the active tab's content area. Useful to keep tab content
    /// away from the scrollbar gutter and add visual breathing room. Defaults to 0.
    /// Note: this is intentionally horizontal-only and asymmetric with
    /// <see cref="PanelWidget.ContentPadding"/> (which applies uniformly on all
    /// sides) because <see cref="TabbedPanelWidget"/> has a pinned tab strip at the
    /// top and an optional scrollbar on the right that need independent control.
    /// Vertical spacing is controlled separately by <see cref="ContentTopPadding"/>.
    /// </summary>
    public int ContentHorizontalPadding { get; set; }

    /// <summary>
    /// Inner vertical padding (in logical UI pixels) inserted between the bottom of
    /// the tab strip and the top of the active tab's content. Defaults to 0.
    /// </summary>
    public int ContentTopPadding { get; set; }

    /// <summary>
    /// Current scroll offset (in logical UI pixels). Clamped to <c>[0, MaxScrollOffset]</c>.
    /// Mouse wheel events update this automatically when <see cref="Scrollable"/> is true
    /// and the cursor is over the panel.
    /// </summary>
    public float ScrollOffset
    {
        get => _scrollOffset;
        set => _scrollOffset = Math.Max(0, Math.Min(value, MaxScrollOffset));
    }

    /// <summary>Switches to the next tab. No wrap; clamped at the last index.</summary>
    public void Next() => ActiveIndex = Math.Min(_activeIndex + 1, _tabs.Count - 1);

    /// <summary>Switches to the previous tab. No wrap; clamped at the first index.</summary>
    public void Previous() => ActiveIndex = Math.Max(_activeIndex - 1, 0);

    /// <summary>Hook called before <see cref="ActiveIndex"/> changes. Return false to
    /// cancel the switch. Default returns true.</summary>
    protected virtual bool OnTabActivating(int previousIndex, int nextIndex) => true;

    private int TabStripHeight
    {
        get
        {
            // Headless test contexts may not have a Font configured. Real
            // rendering paths always have a Font.
            var font = UITheme.Font;
            if (font == null)
                return TabStripVerticalPadding * 2;
            return font.LineSpacing + TabStripVerticalPadding * 2;
        }
    }

    /// <summary>Translation applied to children when scrolling so click positions and
    /// rendered positions both move together. Hidden tabs are not visible but still
    /// shifted (cheap and consistent).</summary>
    protected override Vector2 ChildRenderOffset =>
        Scrollable ? new Vector2(0, -_scrollOffset) : Vector2.Zero;

    /// <summary>When scrolling, child interactions outside the visible content area
    /// (e.g. scrolled-up portions or the tab strip area itself) are blocked so a
    /// scrolled-out widget can't intercept clicks meant for the tab strip.</summary>
    protected override Rectangle? ChildInteractionClipBounds =>
        Scrollable ? ContentBounds : null;

    /// <summary>The visible content area (in screen space) below the tab strip and
    /// excluding the scrollbar gutter when <see cref="Scrollable"/> is true.</summary>
    private Rectangle ContentBounds
    {
        get
        {
            var screenPos = ScreenPosition;
            int gutter = Scrollable ? PanelWidget.ScrollbarWidth : 0;
            int top = TabStripHeight + ContentTopPadding;
            return new Rectangle(
                (int)screenPos.X + ContentHorizontalPadding,
                (int)screenPos.Y + top,
                Math.Max(0, (int)Size.X - gutter - ContentHorizontalPadding * 2),
                Math.Max(0, (int)Size.Y - top)
            );
        }
    }

    /// <summary>Natural height of the active tab's content. Used to compute the
    /// scrollbar thumb size and clamp <see cref="ScrollOffset"/>.</summary>
    private float ActiveContentHeight =>
        _tabs.Count == 0 ? 0 : _tabs[_activeIndex].Content.DesiredSize.Y;

    /// <summary>Maximum valid <see cref="ScrollOffset"/> for the current viewport.</summary>
    private float MaxScrollOffset
    {
        get
        {
            if (!Scrollable)
                return 0;
            return Math.Max(0, ActiveContentHeight - ContentBounds.Height);
        }
    }

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        // When scrollable, give children unlimited height so they report their natural
        // size; the tabbed panel itself fills the available rectangle and provides the
        // scrollbar. When not scrollable, fall back to the original "biggest tab wins"
        // behaviour: the panel grows to fit its tallest child.
        float gutter = Scrollable ? PanelWidget.ScrollbarWidth : 0;
        float horizontalChrome = gutter + ContentHorizontalPadding * 2;
        float verticalChrome = TabStripHeight + ContentTopPadding;
        float contentMeasureWidth = Math.Max(0, availableWidth - horizontalChrome);
        float contentMeasureHeight = Scrollable
            ? float.MaxValue
            : Math.Max(0, availableHeight - verticalChrome);

        float maxW = 0;
        float maxH = 0;
        foreach (var tab in _tabs)
        {
            tab.Content.Measure(contentMeasureWidth, contentMeasureHeight);
            maxW = Math.Max(maxW, tab.Content.DesiredSize.X);
            maxH = Math.Max(maxH, tab.Content.DesiredSize.Y);
        }

        // Ensure the requested width is at least wide enough to fit every tab title in
        // the strip; otherwise narrow content + long titles would clip/overlap labels.
        float titleStripWidth = MeasureTabStripWidth();
        float requestedWidth = Math.Max(maxW + horizontalChrome, titleStripWidth);

        // Scrollable: fill the parent area vertically (scrollbar handles overflow).
        // Non-scrollable: grow to fit the tallest tab.
        // float.MaxValue is the codebase convention for "unconstrained" height
        // (e.g. TooltipWidget); reflecting it back as desired height would yield
        // a nonsensical size, so fall back to content-driven sizing in that case.
        bool hasBoundedHeight = availableHeight > 0 && availableHeight < float.MaxValue;
        float requestedHeight = (Scrollable && hasBoundedHeight)
            ? availableHeight
            : maxH + verticalChrome;

        return new Vector2(requestedWidth, requestedHeight);
    }

    /// <summary>
    /// Splits a tab strip into per-tab widths, sharing the room in proportion to how
    /// much each title needs so a short title never hoards space a long one is missing.
    /// The widths always sum to <paramref name="totalWidth"/> exactly, so the strip
    /// paints edge-to-edge with no gap and no dead zone.
    /// </summary>
    public static int[] ComputeTabWidths(IReadOnlyList<float> titleWidths, int horizontalPadding, int totalWidth)
    {
        int count = titleWidths.Count;
        var widths = new int[count];
        if (count == 0 || totalWidth <= 0)
            return widths;

        var natural = new float[count];
        float naturalTotal = 0;
        for (int i = 0; i < count; i++)
        {
            natural[i] = titleWidths[i] + horizontalPadding * 2;
            naturalTotal += natural[i];
        }

        if (naturalTotal <= 0)
        {
            // No font, or every title measured empty: fall back to an even split.
            for (int i = 0; i < count; i++)
                natural[i] = 1;
            naturalTotal = count;
        }

        float scale = totalWidth / naturalTotal;
        int consumed = 0;
        for (int i = 0; i < count - 1; i++)
        {
            widths[i] = (int)(natural[i] * scale);
            consumed += widths[i];
        }

        // The last tab absorbs the rounding remainder so the strip fills exactly.
        widths[count - 1] = totalWidth - consumed;
        return widths;
    }

    /// <summary>
    /// Turns per-tab widths into left offsets relative to the start of the strip.
    /// </summary>
    public static int[] ComputeTabOffsets(IReadOnlyList<int> tabWidths)
    {
        var offsets = new int[tabWidths.Count];
        int x = 0;
        for (int i = 0; i < tabWidths.Count; i++)
        {
            offsets[i] = x;
            x += tabWidths[i];
        }
        return offsets;
    }

    private int[] CurrentTabWidths()
    {
        var font = UITheme.Font;
        var titleWidths = new float[_tabs.Count];
        for (int i = 0; i < _tabs.Count; i++)
            titleWidths[i] = font?.MeasureString(_tabs[i].Title).X ?? 0;
        return ComputeTabWidths(titleWidths, TabStripHorizontalPadding, (int)Size.X);
    }

    private float MeasureTabStripWidth()
    {
        // Headless test contexts may not have a Font configured; fall back to
        // letting content drive width. Real rendering paths always have a Font.
        var font = UITheme.Font;
        if (font == null)
            return 0;
        float total = 0;
        foreach (var tab in _tabs)
            total += font.MeasureString(tab.Title).X + TabStripHorizontalPadding * 2;
        return total;
    }

    protected override void ArrangeCore(Vector2 finalSize)
    {
        foreach (var tab in _tabs)
        {
            tab.Content.Position = new Vector2(ContentHorizontalPadding, TabStripHeight + ContentTopPadding);
            tab.Content.Arrange(tab.Content.DesiredSize);
        }

        // Re-clamp the scroll offset after layout in case the active tab shrank or the
        // viewport grew, leaving us scrolled past the new max.
        if (Scrollable)
            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, MaxScrollOffset));
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        // Mouse-wheel scrolling: only consume wheel deltas while the cursor is over
        // this widget so other scrollable siblings on screen aren't double-scrolled.
        // Initialize the wheel value on the first frame so a pre-existing absolute
        // scroll value from before the panel was opened doesn't translate into a
        // huge spurious delta and snap the panel to the bottom on its first frame.
        if (!_scrollWheelInitialized)
        {
            _previousScrollWheelValue = mouseState.ScrollWheelValue;
            _scrollWheelInitialized = true;
        }
        else if (Scrollable && Bounds.Contains(mouseState.X, mouseState.Y))
        {
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollWheelValue;
            if (scrollDelta != 0)
                ScrollOffset -= scrollDelta / 120f * ScrollSpeed;
        }
        _previousScrollWheelValue = mouseState.ScrollWheelValue;

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
        if (totalWidth <= 0 || _tabs.Count == 0)
            return null;

        var tabWidths = CurrentTabWidths();
        var offsets = ComputeTabOffsets(tabWidths);
        int relativeX = mouseX - tabAreaBounds.X;
        for (int i = _tabs.Count - 1; i >= 0; i--)
        {
            if (relativeX >= offsets[i])
                return i;
        }
        return 0;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;
        RenderTabStrip(spriteBatch);
        RenderCore(spriteBatch);

        if (Scrollable)
        {
            RenderScrollableChildren(spriteBatch);
        }
        else
        {
            // Children render themselves; only the active tab's Content has Visible = true.
            foreach (var child in Children)
                child.Render(spriteBatch);
        }
    }

    private void RenderScrollableChildren(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var contentBounds = ContentBounds;

        // End the current batch so we can swap rasterizer state to one with scissor
        // testing enabled and clip child rendering to the content area.
        spriteBatch.End();

        var originalScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
        var originalRasterizerState = spriteBatch.GraphicsDevice.RasterizerState;

        var scale = UITheme.UIScale;
        var sampler = scale < 1f ? SamplerState.LinearClamp : SamplerState.PointClamp;
        var matrix = scale < 1f ? UITheme.UIScaleMatrix : (Matrix?)null;
        var ourScissor = new Rectangle(
            (int)(contentBounds.X * scale),
            (int)(contentBounds.Y * scale),
            (int)(contentBounds.Width * scale),
            (int)(contentBounds.Height * scale)
        );
        spriteBatch.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(ourScissor, originalScissor);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            sampler,
            null,
            ScissorEnabledRasterizer,
            null,
            matrix
        );

        // Children render at their (positions + ChildRenderOffset which subtracts scroll).
        foreach (var child in Children)
            child.Render(spriteBatch);

        spriteBatch.End();

        // Restore original state for the scrollbar and any subsequent draws.
        spriteBatch.GraphicsDevice.ScissorRectangle = originalScissor;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            sampler,
            null,
            originalRasterizerState,
            null,
            matrix
        );

        RenderScrollbar(spriteBatch, pixel, contentBounds);
    }

    private void RenderScrollbar(SpriteBatch spriteBatch, Texture2D pixel, Rectangle contentBounds)
    {
        if (MaxScrollOffset <= 0)
            return;

        int scrollbarHeight = contentBounds.Height;
        if (scrollbarHeight <= 0)
            return;

        int scrollbarX = (int)ScreenPosition.X + (int)Size.X - PanelWidget.ScrollbarWidth;

        // Track
        spriteBatch.Draw(pixel,
            new Rectangle(scrollbarX, contentBounds.Y, PanelWidget.ScrollbarWidth, scrollbarHeight),
            UITheme.Scrollbar.Track);

        // Thumb. Clamp to scrollbarHeight so the minimum-height enforcement (20px)
        // doesn't push the thumb above the track when the visible area is tiny.
        float thumbRatio = contentBounds.Height / (contentBounds.Height + MaxScrollOffset);
        int thumbHeight = Math.Min(scrollbarHeight, Math.Max(20, (int)(scrollbarHeight * thumbRatio)));
        float scrollRatio = MaxScrollOffset > 0 ? _scrollOffset / MaxScrollOffset : 0;
        int thumbY = contentBounds.Y + (int)((scrollbarHeight - thumbHeight) * scrollRatio);

        spriteBatch.Draw(pixel,
            new Rectangle(scrollbarX, thumbY, PanelWidget.ScrollbarWidth, thumbHeight),
            UITheme.Scrollbar.Thumb);
    }

    private void RenderTabStrip(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var font = UITheme.Font;
        var screenPos = ScreenPosition;
        int stripHeight = TabStripHeight;
        int totalWidth = (int)Size.X;
        if (totalWidth <= 0 || _tabs.Count == 0)
            return;

        var tabWidths = CurrentTabWidths();
        var offsets = ComputeTabOffsets(tabWidths);

        for (int i = 0; i < _tabs.Count; i++)
        {
            // Widths come from the same calculator HitTestTabStrip uses, so the painted
            // strip and the clickable strip can never drift apart.
            int x = (int)screenPos.X + offsets[i];
            var tabBounds = new Rectangle(x, (int)screenPos.Y, tabWidths[i], stripHeight);

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
