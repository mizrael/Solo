using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class AddFramesToAnimationCommand : IUndoableCommand
{
    private readonly AnimationDefinition _animation;
    private readonly List<AnimationFrame> _frames;

    public string Description => $"Add {_frames.Count} frame(s) to '{_animation.Name}'";

    public AddFramesToAnimationCommand(AnimationDefinition animation, List<AnimationFrame> frames)
    {
        _animation = animation;
        _frames = frames;
    }

    public void Execute()
    {
        foreach (var frame in _frames)
        {
            if (!_animation.Frames.Contains(frame))
            {
                _animation.Frames.Add(frame);
            }
        }
    }

    public void Undo()
    {
        foreach (var frame in _frames)
        {
            _animation.Frames.Remove(frame);
        }
    }

    public void Dispose()
    {
    }
}
