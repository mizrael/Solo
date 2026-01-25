using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SkiaSharp;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.UndoRedo.Commands;
using SpriteSheetEditor.Utils;
using SpriteSheetEditor.ViewModels;
using System.Runtime.InteropServices;

namespace SpriteSheetEditor.Controls;

public class SpriteCanvas : Control
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

    public static readonly StyledProperty<MainViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<SpriteCanvas, MainViewModel?>(nameof(ViewModel));

    public MainViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
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

    private bool _isPanning;
    private SKPoint _panStart;
    private float _panStartOffsetX;
    private float _panStartOffsetY;

    private WriteableBitmap? _renderTarget;

    public bool IsEyedropperMode { get; set; }
    public ScrollViewer? ParentScrollViewer { get; set; }
    public event Action<SKColor>? ColorPicked;
    public event Action<SpriteDefinition>? SpriteDrawn;
    public event Action<SpriteDefinition, SpriteState, string>? SpriteModified;
    public event Action? PanChanged;

    static SpriteCanvas()
    {
        AffectsRender<SpriteCanvas>(ViewModelProperty);
    }

    public SpriteCanvas()
    {
        ClipToBounds = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (ViewModel is MainViewModel vm)
            vm.PropertyChanged -= OnViewModelPropertyChanged;

        _renderTarget?.Dispose();
        _renderTarget = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewModelProperty)
        {
            if (change.OldValue is MainViewModel oldVm)
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;

            if (change.NewValue is MainViewModel newVm)
                newVm.PropertyChanged += OnViewModelPropertyChanged;

            InvalidateVisual();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    public bool UseScrollViewMode { get; set; } = true;

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var width = (int)bounds.Width;
        var height = (int)bounds.Height;

        if (width <= 0 || height <= 0) return;

        // Create or resize render target
        if (_renderTarget == null || _renderTarget.PixelSize.Width != width || _renderTarget.PixelSize.Height != height)
        {
            _renderTarget?.Dispose();
            _renderTarget = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Bgra8888, Avalonia.Platform.AlphaFormat.Premul);
        }

        using (var lockedBitmap = _renderTarget.Lock())
        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info, lockedBitmap.Address, lockedBitmap.RowBytes);
            var canvas = surface.Canvas;

            canvas.Clear(new SKColor(64, 64, 64));

            if (ViewModel is not null)
            {
                canvas.Save();

                if (!UseScrollViewMode)
                {
                    canvas.Translate(ViewModel.PanOffsetX, ViewModel.PanOffsetY);
                }

                canvas.Scale(ViewModel.ZoomLevel);

                DrawCheckerboard(canvas);
                DrawImage(canvas);
                DrawSprites(canvas);
                DrawDrawingPreview(canvas);

                canvas.Restore();
            }
        }

        context.DrawImage(_renderTarget, new Rect(0, 0, width, height));
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

    private SKPoint ScreenToImage(Point screenPoint)
    {
        if (ViewModel is null) return new SKPoint((float)screenPoint.X, (float)screenPoint.Y);

        if (UseScrollViewMode)
        {
            return new SKPoint(
                (float)screenPoint.X / ViewModel.ZoomLevel,
                (float)screenPoint.Y / ViewModel.ZoomLevel
            );
        }

        return new SKPoint(
            ((float)screenPoint.X - ViewModel.PanOffsetX) / ViewModel.ZoomLevel,
            ((float)screenPoint.Y - ViewModel.PanOffsetY) / ViewModel.ZoomLevel
        );
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (ViewModel is null) return;

        var point = e.GetCurrentPoint(this);
        var imagePoint = ScreenToImage(point.Position);

        if (point.Properties.IsMiddleButtonPressed)
        {
            if (ViewModel?.Document.LoadedImage is null) return;

            _isPanning = true;
            _panStart = new SKPoint((float)point.Position.X, (float)point.Position.Y);

            if (UseScrollViewMode && ParentScrollViewer is not null)
            {
                _panStartOffsetX = (float)ParentScrollViewer.Offset.X;
                _panStartOffsetY = (float)ParentScrollViewer.Offset.Y;
            }
            else
            {
                _panStartOffsetX = ViewModel.PanOffsetX;
                _panStartOffsetY = ViewModel.PanOffsetY;
            }

            e.Pointer.Capture(this);
            e.Handled = true;
            return;
        }

        if (point.Properties.IsLeftButtonPressed)
        {
            HandlePress(imagePoint);
            e.Pointer.Capture(this);
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (ViewModel is null) return;

        var point = e.GetCurrentPoint(this);

        if (_isPanning)
        {
            var deltaX = (float)point.Position.X - _panStart.X;
            var deltaY = (float)point.Position.Y - _panStart.Y;

            if (UseScrollViewMode && ParentScrollViewer is not null)
            {
                var newScrollX = _panStartOffsetX - deltaX;
                var newScrollY = _panStartOffsetY - deltaY;
                ParentScrollViewer.Offset = new Vector(
                    Math.Max(0, newScrollX),
                    Math.Max(0, newScrollY));
            }
            else
            {
                ViewModel.PanOffsetX = _panStartOffsetX + deltaX;
                ViewModel.PanOffsetY = _panStartOffsetY + deltaY;
                ClampPanOffset();
                PanChanged?.Invoke();
                InvalidateVisual();
            }

            e.Handled = true;
            return;
        }

        if (point.Properties.IsLeftButtonPressed && (_isDrawing || _isDragging || _isResizing))
        {
            var imagePoint = ScreenToImage(point.Position);
            HandleMove(imagePoint);
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
            e.Handled = true;
            return;
        }

        if (_isDrawing || _isDragging || _isResizing)
        {
            var point = e.GetCurrentPoint(this);
            var imagePoint = ScreenToImage(point.Position);
            HandleRelease(imagePoint);
            e.Pointer.Capture(null);
            InvalidateVisual();
            e.Handled = true;
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (ViewModel?.Document.LoadedImage is null) return;

        if (e.Delta.Y > 0)
            ZoomIn();
        else if (e.Delta.Y < 0)
            ZoomOut();

        e.Handled = true;
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

        var containerWidth = UseScrollViewMode && ParentScrollViewer is not null
            ? (float)ParentScrollViewer.Bounds.Width
            : (float)Bounds.Width;
        var containerHeight = UseScrollViewMode && ParentScrollViewer is not null
            ? (float)ParentScrollViewer.Bounds.Height
            : (float)Bounds.Height;

        if (containerWidth <= 0 || containerHeight <= 0) return;

        var scaleX = containerWidth / ViewModel.ImageWidth;
        var scaleY = containerHeight / ViewModel.ImageHeight;
        ViewModel.ZoomLevel = Math.Min(scaleX, scaleY) * 0.9f;

        if (UseScrollViewMode && ParentScrollViewer is not null)
        {
            ViewModel.PanOffsetX = 0;
            ViewModel.PanOffsetY = 0;
            ParentScrollViewer.Offset = new Vector(0, 0);
        }
        else
        {
            ViewModel.PanOffsetX = (containerWidth - ViewModel.ImageWidth * ViewModel.ZoomLevel) / 2;
            ViewModel.PanOffsetY = (containerHeight - ViewModel.ImageHeight * ViewModel.ZoomLevel) / 2;
        }

        PanChanged?.Invoke();
    }

    private void ClampPanOffset()
    {
        if (ViewModel is null) return;

        var scaledWidth = ViewModel.ImageWidth * ViewModel.ZoomLevel;
        var scaledHeight = ViewModel.ImageHeight * ViewModel.ZoomLevel;

        var minX = (float)Bounds.Width - scaledWidth;
        var minY = (float)Bounds.Height - scaledHeight;

        ViewModel.PanOffsetX = Math.Clamp(ViewModel.PanOffsetX, Math.Min(minX, 0), Math.Max(0, minX));
        ViewModel.PanOffsetY = Math.Clamp(ViewModel.PanOffsetY, Math.Min(minY, 0), Math.Max(0, minY));
    }
}
