using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Controls;

public class AnimationPreviewCanvas : Control, IDisposable
{
    private const int CheckerSize = 16;
    private static readonly SKColor CheckerLight = new(200, 200, 200);
    private static readonly SKColor CheckerDark = new(150, 150, 150);

    public static readonly StyledProperty<SKBitmap?> SourceImageProperty =
        AvaloniaProperty.Register<AnimationPreviewCanvas, SKBitmap?>(nameof(SourceImage));

    public static readonly StyledProperty<AnimationDefinition?> AnimationProperty =
        AvaloniaProperty.Register<AnimationPreviewCanvas, AnimationDefinition?>(nameof(Animation));

    public static readonly StyledProperty<int> CurrentFrameIndexProperty =
        AvaloniaProperty.Register<AnimationPreviewCanvas, int>(nameof(CurrentFrameIndex));

    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<AnimationPreviewCanvas, bool>(nameof(IsPlaying));

    public SKBitmap? SourceImage
    {
        get => GetValue(SourceImageProperty);
        set => SetValue(SourceImageProperty, value);
    }

    public AnimationDefinition? Animation
    {
        get => GetValue(AnimationProperty);
        set => SetValue(AnimationProperty, value);
    }

    public int CurrentFrameIndex
    {
        get => GetValue(CurrentFrameIndexProperty);
        set => SetValue(CurrentFrameIndexProperty, value);
    }

    public bool IsPlaying
    {
        get => GetValue(IsPlayingProperty);
        set => SetValue(IsPlayingProperty, value);
    }

    private WriteableBitmap? _renderTarget;
    private DispatcherTimer? _playbackTimer;

    public event EventHandler<int>? FrameChanged;

    static AnimationPreviewCanvas()
    {
        AffectsRender<AnimationPreviewCanvas>(SourceImageProperty, AnimationProperty, CurrentFrameIndexProperty);
    }

    public AnimationPreviewCanvas()
    {
        ClipToBounds = true;
        _playbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _playbackTimer.Tick += OnPlaybackTick;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == AnimationProperty)
        {
            UpdateTimerInterval();
            CurrentFrameIndex = 0;
            InvalidateVisual();
        }
        else if (change.Property == IsPlayingProperty)
        {
            if (IsPlaying)
            {
                UpdateTimerInterval();
                _playbackTimer?.Start();
            }
            else
            {
                _playbackTimer?.Stop();
            }
        }
    }

    private void UpdateTimerInterval()
    {
        if (_playbackTimer is null || Animation is null) return;

        var fps = Math.Max(1, Math.Min(60, Animation.Fps));
        _playbackTimer.Interval = TimeSpan.FromMilliseconds(1000.0 / fps);
    }

    private void OnPlaybackTick(object? sender, EventArgs e)
    {
        if (Animation is null || Animation.Frames.Count == 0) return;

        var nextFrame = CurrentFrameIndex + 1;
        if (nextFrame >= Animation.Frames.Count)
        {
            if (Animation.Loop)
            {
                nextFrame = 0;
            }
            else
            {
                Stop();
                return;
            }
        }

        CurrentFrameIndex = nextFrame;
        FrameChanged?.Invoke(this, CurrentFrameIndex);
        InvalidateVisual();
    }

    public void Play()
    {
        if (Animation is null || Animation.Frames.Count == 0) return;
        IsPlaying = true;
    }

    public void Pause()
    {
        IsPlaying = false;
    }

    public void Stop()
    {
        IsPlaying = false;
        CurrentFrameIndex = 0;
        FrameChanged?.Invoke(this, CurrentFrameIndex);
        InvalidateVisual();
    }

    public void TogglePlayPause()
    {
        if (IsPlaying)
            Pause();
        else
            Play();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _playbackTimer?.Stop();
        _renderTarget?.Dispose();
        _renderTarget = null;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var width = (int)bounds.Width;
        var height = (int)bounds.Height;

        if (width <= 0 || height <= 0) return;

        if (_renderTarget == null || _renderTarget.PixelSize.Width != width || _renderTarget.PixelSize.Height != height)
        {
            _renderTarget?.Dispose();
            _renderTarget = new WriteableBitmap(
                new PixelSize(width, height),
                new Vector(96, 96),
                Avalonia.Platform.PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul);
        }

        using (var lockedBitmap = _renderTarget.Lock())
        {
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info, lockedBitmap.Address, lockedBitmap.RowBytes);
            var canvas = surface.Canvas;

            canvas.Clear(new SKColor(64, 64, 64));

            var currentFrame = GetCurrentFrame();
            if (currentFrame is not null && SourceImage is not null)
            {
                var sprite = currentFrame.Sprite;
                var frameWidth = sprite.Width;
                var frameHeight = sprite.Height;

                if (frameWidth > 0 && frameHeight > 0)
                {
                    var scale = Math.Min((float)width / frameWidth, (float)height / frameHeight) * 0.9f;
                    var scaledWidth = frameWidth * scale;
                    var scaledHeight = frameHeight * scale;
                    var offsetX = (width - scaledWidth) / 2;
                    var offsetY = (height - scaledHeight) / 2;

                    canvas.Save();
                    canvas.Translate(offsetX, offsetY);
                    canvas.Scale(scale);

                    DrawCheckerboard(canvas, frameWidth, frameHeight);
                    DrawFrame(canvas, currentFrame);

                    canvas.Restore();
                }
            }
            else
            {
                DrawCheckerboard(canvas, width, height);
            }
        }

        context.DrawImage(_renderTarget, new Rect(0, 0, width, height));
    }

    private AnimationFrame? GetCurrentFrame()
    {
        if (Animation is null || Animation.Frames.Count == 0) return null;

        var index = Math.Clamp(CurrentFrameIndex, 0, Animation.Frames.Count - 1);
        return Animation.Frames[index];
    }

    private void DrawCheckerboard(SKCanvas canvas, int width, int height)
    {
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
    }

    private void DrawFrame(SKCanvas canvas, AnimationFrame frame)
    {
        if (SourceImage is null) return;

        var sprite = frame.Sprite;
        var srcRect = new SKRect(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height);
        var destRect = new SKRect(0, 0, sprite.Width, sprite.Height);

        canvas.DrawBitmap(SourceImage, srcRect, destRect);
    }

    public void Dispose()
    {
        _playbackTimer?.Stop();
        _playbackTimer = null;
        _renderTarget?.Dispose();
        _renderTarget = null;
    }
}
