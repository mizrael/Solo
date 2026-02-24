using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Solo.UI.Widgets;

public class PopupWidget : PanelWidget
{
    private readonly List<Widget> _views = new();
    private Widget? _activeView;
    private Action? _activeOnClosed;

    public Action? OnUpdatePosition { get; set; }

    public PopupWidget()
    {
        ShowCloseButton = true;
        Visible = false;
        OnCloseClicked += HandleClose;
    }

    public void RegisterView(Widget view)
    {
        ArgumentNullException.ThrowIfNull(view);
        if (_views.Contains(view))
            return;
        view.Visible = false;
        _views.Add(view);
        AddChild(view);
    }

    public void ShowView(Widget view, Action? onClosed = null)
    {
        if (!_views.Contains(view))
            throw new ArgumentException("View must be registered before it can be shown.", nameof(view));
        foreach (var v in _views)
            v.Visible = v == view;
        _activeView = view;
        _activeOnClosed = onClosed;
        Visible = true;
        InvalidateMeasure();
    }

    public void Toggle(Widget view, Action? onClosed = null)
    {
        if (Visible && _activeView == view)
            Hide();
        else
            ShowView(view, onClosed);
    }

    public void Hide()
    {
        Visible = false;
        foreach (var v in _views)
            v.Visible = false;
        _activeView = null;
        _activeOnClosed = null;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        if (Visible)
            OnUpdatePosition?.Invoke();
        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    private void HandleClose()
    {
        var callback = _activeOnClosed;
        Hide();
        callback?.Invoke();
    }
}
