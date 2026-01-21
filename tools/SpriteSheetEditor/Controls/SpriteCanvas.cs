using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.UndoRedo.Commands;
using SpriteSheetEditor.Utils;
using SpriteSheetEditor.ViewModels;

namespace SpriteSheetEditor.Controls;

public class SpriteCanvas : SKCanvasView
{
    private const int CheckerSize = 16;
    private static readonly SKColor CheckerLight = new(200, 200, 200);
    private static readonly SKColor CheckerDark = new(150, 150, 150);
    private static readonly SKColor SpriteStroke = new(60, 120, 200);
    private static readonly SKColor SpriteFill = new(60, 120, 200, 50);
    private static readonly SKColor SelectedStroke = new(255, 150, 50);
    private static readonly SKColor SelectedFill = new(255, 150, 50, 50);
    private static readonly SKColor HandleColor = SKColors.White;

    private const float HandleSize = 8f;

    public static readonly BindableProperty ViewModelProperty =
        BindableProperty.Create(nameof(ViewModel), typeof(MainViewModel), typeof(SpriteCanvas),
            propertyChanged: OnViewModelChanged);

    public MainViewModel? ViewModel
    {
        get => (MainViewModel?)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private bool _isDrawing;
    private SKPoint _drawStart;
    private SKPoint _drawCurrent;

    private bool _isDragging;
    private SpriteDefinition? _dragSprite;
    private SKPoint _dragOffset;

    private bool _isResizing;
    private int _resizeHandle = -1;
    private SpriteState? _dragStartState;

    public bool IsEyedropperMode { get; set; }
    public event Action<SKColor>? ColorPicked;
    public event Action<SpriteDefinition>? SpriteDrawn;
    public event Action<SpriteDefinition, SpriteState, string>? SpriteModified;

    public SpriteCanvas()
    {
        EnableTouchEvents = true;
        Touch += OnTouch;
    }

    private static void OnViewModelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SpriteCanvas canvas)
        {
            if (oldValue is MainViewModel oldVm)
                oldVm.PropertyChanged -= canvas.OnViewModelPropertyChanged;

            if (newValue is MainViewModel newVm)
                newVm.PropertyChanged += canvas.OnViewModelPropertyChanged;

            canvas.InvalidateSurface();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvalidateSurface();
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.DarkGray);

        if (ViewModel is null) return;

        canvas.Save();
        canvas.Translate(ViewModel.PanOffsetX, ViewModel.PanOffsetY);
        canvas.Scale(ViewModel.ZoomLevel);

        DrawCheckerboard(canvas);
        DrawImage(canvas);
        DrawSprites(canvas);
        DrawDrawingPreview(canvas);

        canvas.Restore();
    }

    private void DrawCheckerboard(SKCanvas canvas)
    {
        var width = ViewModel?.ImageWidth ?? 512;
        var height = ViewModel?.ImageHeight ?? 512;

        canvas.Save();
        canvas.ClipRect(new SKRect(0, 0, width, height));

        using var lightPaint = new SKPaint { Color = CheckerLight };
        using var darkPaint = new SKPaint { Color = CheckerDark };

        for (var y = 0; y < height; y += CheckerSize)
        {
            for (var x = 0; x < width; x += CheckerSize)
            {
                var isLight = ((x / CheckerSize) + (y / CheckerSize)) % 2 == 0;
                var paint = isLight ? lightPaint : darkPaint;
                canvas.DrawRect(x, y, CheckerSize, CheckerSize, paint);
            }
        }

        canvas.Restore();
    }

    private void DrawImage(SKCanvas canvas)
    {
        if (ViewModel?.Document.LoadedImage is { } image)
        {
            canvas.DrawBitmap(image, 0, 0);
        }
    }

    private void DrawSprites(SKCanvas canvas)
    {
        if (ViewModel is null) return;

        using var strokePaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
        using var fillPaint = new SKPaint { Style = SKPaintStyle.Fill };

        foreach (var sprite in ViewModel.Document.Sprites)
        {
            var isSelected = sprite == ViewModel.SelectedSprite;
            strokePaint.Color = isSelected ? SelectedStroke : SpriteStroke;
            fillPaint.Color = isSelected ? SelectedFill : SpriteFill;

            var rect = new SKRect(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height);
            canvas.DrawRect(rect, fillPaint);
            canvas.DrawRect(rect, strokePaint);

            if (isSelected)
            {
                DrawResizeHandles(canvas, rect);
            }
        }
    }

    private void DrawResizeHandles(SKCanvas canvas, SKRect rect)
    {
        using var handlePaint = new SKPaint { Color = HandleColor, Style = SKPaintStyle.Fill };
        using var handleStroke = new SKPaint { Color = SelectedStroke, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };

        var handles = GetHandlePositions(rect);
        foreach (var pos in handles)
        {
            var handleRect = new SKRect(pos.X - HandleSize / 2, pos.Y - HandleSize / 2,
                                        pos.X + HandleSize / 2, pos.Y + HandleSize / 2);
            canvas.DrawRect(handleRect, handlePaint);
            canvas.DrawRect(handleRect, handleStroke);
        }
    }

    private static SKPoint[] GetHandlePositions(SKRect rect)
    {
        return
        [
            new SKPoint(rect.Left, rect.Top),
            new SKPoint(rect.Right, rect.Top),
            new SKPoint(rect.Right, rect.Bottom),
            new SKPoint(rect.Left, rect.Bottom)
        ];
    }

    private void DrawDrawingPreview(SKCanvas canvas)
    {
        if (!_isDrawing) return;

        using var strokePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2,
            Color = SelectedStroke,
            PathEffect = SKPathEffect.CreateDash([5, 5], 0)
        };

        var rect = GetDrawingRect();
        canvas.DrawRect(rect, strokePaint);
    }

    private SKRect GetDrawingRect()
    {
        var left = Math.Min(_drawStart.X, _drawCurrent.X);
        var top = Math.Min(_drawStart.Y, _drawCurrent.Y);
        var right = Math.Max(_drawStart.X, _drawCurrent.X);
        var bottom = Math.Max(_drawStart.Y, _drawCurrent.Y);
        return new SKRect(left, top, right, bottom);
    }

    private void OnTouch(object? sender, SKTouchEventArgs e)
    {
        if (ViewModel is null) return;

        var imagePoint = ScreenToImage(e.Location);

        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                HandlePress(imagePoint);
                break;
            case SKTouchAction.Moved:
                HandleMove(imagePoint);
                break;
            case SKTouchAction.Released:
                HandleRelease(imagePoint);
                break;
            case SKTouchAction.Cancelled:
                CancelCurrentOperation();
                break;
        }

        e.Handled = true;
        InvalidateSurface();
    }

    private SKPoint ScreenToImage(SKPoint screenPoint)
    {
        if (ViewModel is null) return screenPoint;
        return new SKPoint(
            (screenPoint.X - ViewModel.PanOffsetX) / ViewModel.ZoomLevel,
            (screenPoint.Y - ViewModel.PanOffsetY) / ViewModel.ZoomLevel
        );
    }

    private void HandlePress(SKPoint imagePoint)
    {
        if (ViewModel is null) return;

        if (IsEyedropperMode && ViewModel.Document.LoadedImage is SKBitmap image)
        {
            var imageX = (int)imagePoint.X;
            var imageY = (int)imagePoint.Y;

            if (imageX >= 0 && imageX < image.Width && imageY >= 0 && imageY < image.Height)
            {
                var color = image.GetPixel(imageX, imageY);
                ColorPicked?.Invoke(color);
            }

            IsEyedropperMode = false;
            return;
        }

        if (ViewModel.CurrentTool == EditorTool.Draw)
        {
            _isDrawing = true;
            _drawStart = imagePoint;
            _drawCurrent = imagePoint;
        }
        else
        {
            if (ViewModel.SelectedSprite is { } selected)
            {
                var rect = new SKRect(selected.X, selected.Y, selected.X + selected.Width, selected.Y + selected.Height);
                var handles = GetHandlePositions(rect);
                for (var i = 0; i < handles.Length; i++)
                {
                    if (SKColorUtils.IsNearPoint(imagePoint, handles[i], HandleSize))
                    {
                        _isResizing = true;
                        _resizeHandle = i;
                        _dragStartState = SpriteState.From(selected);
                        return;
                    }
                }
            }

            var sprite = ViewModel.FindSpriteAt((int)imagePoint.X, (int)imagePoint.Y);
            if (sprite != null)
            {
                ViewModel.SelectedSprite = sprite;
                _isDragging = true;
                _dragSprite = sprite;
                _dragOffset = new SKPoint(imagePoint.X - sprite.X, imagePoint.Y - sprite.Y);
                _dragStartState = SpriteState.From(sprite);
            }
            else
            {
                ViewModel.SelectedSprite = null;
            }
        }
    }

    private void HandleMove(SKPoint imagePoint)
    {
        if (_isDrawing)
        {
            _drawCurrent = imagePoint;
        }
        else if (_isDragging && _dragSprite != null)
        {
            _dragSprite.X = (int)(imagePoint.X - _dragOffset.X);
            _dragSprite.Y = (int)(imagePoint.Y - _dragOffset.Y);
        }
        else if (_isResizing && ViewModel?.SelectedSprite is { } sprite)
        {
            ResizeSprite(sprite, imagePoint);
        }
    }

    private void ResizeSprite(SpriteDefinition sprite, SKPoint imagePoint)
    {
        var x = (int)imagePoint.X;
        var y = (int)imagePoint.Y;

        switch (_resizeHandle)
        {
            case 0:
                var newWidth0 = sprite.X + sprite.Width - x;
                var newHeight0 = sprite.Y + sprite.Height - y;
                if (newWidth0 > 0 && newHeight0 > 0)
                {
                    sprite.X = x;
                    sprite.Y = y;
                    sprite.Width = newWidth0;
                    sprite.Height = newHeight0;
                }
                break;
            case 1:
                var newWidth1 = x - sprite.X;
                var newHeight1 = sprite.Y + sprite.Height - y;
                if (newWidth1 > 0 && newHeight1 > 0)
                {
                    sprite.Y = y;
                    sprite.Width = newWidth1;
                    sprite.Height = newHeight1;
                }
                break;
            case 2:
                var newWidth2 = x - sprite.X;
                var newHeight2 = y - sprite.Y;
                if (newWidth2 > 0 && newHeight2 > 0)
                {
                    sprite.Width = newWidth2;
                    sprite.Height = newHeight2;
                }
                break;
            case 3:
                var newWidth3 = sprite.X + sprite.Width - x;
                var newHeight3 = y - sprite.Y;
                if (newWidth3 > 0 && newHeight3 > 0)
                {
                    sprite.X = x;
                    sprite.Width = newWidth3;
                    sprite.Height = newHeight3;
                }
                break;
        }
    }

    private void HandleRelease(SKPoint imagePoint)
    {
        if (_isDrawing && ViewModel != null)
        {
            var rect = GetDrawingRect();
            if (rect.Width > 5 && rect.Height > 5)
            {
                var index = ViewModel.Document.GetNextSpriteIndex();
                var sprite = new SpriteDefinition
                {
                    Name = ViewModel.Document.GenerateSpriteName(index),
                    X = (int)rect.Left,
                    Y = (int)rect.Top,
                    Width = (int)rect.Width,
                    Height = (int)rect.Height
                };
                SpriteDrawn?.Invoke(sprite);
            }
        }
        else if (_isDragging && _dragSprite != null && _dragStartState.HasValue)
        {
            var currentState = SpriteState.From(_dragSprite);
            if (!currentState.Equals(_dragStartState.Value))
            {
                SpriteModified?.Invoke(_dragSprite, _dragStartState.Value, "Move sprite");
            }
        }
        else if (_isResizing && ViewModel?.SelectedSprite is { } resizedSprite && _dragStartState.HasValue)
        {
            var currentState = SpriteState.From(resizedSprite);
            if (!currentState.Equals(_dragStartState.Value))
            {
                SpriteModified?.Invoke(resizedSprite, _dragStartState.Value, "Resize sprite");
            }
        }

        CancelCurrentOperation();
    }

    private void CancelCurrentOperation()
    {
        _isDrawing = false;
        _isDragging = false;
        _dragSprite = null;
        _isResizing = false;
        _resizeHandle = -1;
        _dragStartState = null;
    }

    public void ZoomIn()
    {
        if (ViewModel?.Document.LoadedImage is null) return;
        ViewModel.ZoomLevel = Math.Min(ViewModel.ZoomLevel * 1.25f, 10f);
    }

    public void ZoomOut()
    {
        if (ViewModel?.Document.LoadedImage is null) return;
        ViewModel.ZoomLevel = Math.Max(ViewModel.ZoomLevel / 1.25f, 0.1f);
    }

    public void FitToWindow()
    {
        if (ViewModel is null || ViewModel.ImageWidth == 0) return;

        var scaleX = (float)Width / ViewModel.ImageWidth;
        var scaleY = (float)Height / ViewModel.ImageHeight;
        ViewModel.ZoomLevel = Math.Min(scaleX, scaleY) * 0.9f;
        ViewModel.PanOffsetX = (float)(Width - ViewModel.ImageWidth * ViewModel.ZoomLevel) / 2;
        ViewModel.PanOffsetY = (float)(Height - ViewModel.ImageHeight * ViewModel.ZoomLevel) / 2;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

#if WINDOWS
        if (Handler?.PlatformView is Microsoft.UI.Xaml.UIElement element)
        {
            element.PointerWheelChanged += OnPointerWheelChanged;
        }
#endif
    }

#if WINDOWS
    private void OnPointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (ViewModel?.Document.LoadedImage is null) return;

        var point = e.GetCurrentPoint((Microsoft.UI.Xaml.UIElement)sender);
        var delta = point.Properties.MouseWheelDelta;

        if (delta > 0)
            ZoomIn();
        else if (delta < 0)
            ZoomOut();

        e.Handled = true;
    }
#endif
}
