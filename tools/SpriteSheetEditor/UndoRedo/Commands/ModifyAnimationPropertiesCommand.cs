using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public record AnimationState(string Name, int Fps, bool Loop)
{
    public static AnimationState From(AnimationDefinition animation)
        => new(animation.Name, animation.Fps, animation.Loop);

    public void ApplyTo(AnimationDefinition animation)
    {
        animation.Name = Name;
        animation.Fps = Fps;
        animation.Loop = Loop;
    }
}

public class ModifyAnimationPropertiesCommand : IUndoableCommand
{
    private readonly AnimationDefinition _animation;
    private readonly AnimationState _beforeState;
    private readonly AnimationState _afterState;

    public string Description => $"Modify animation '{_animation.Name}'";

    public ModifyAnimationPropertiesCommand(
        AnimationDefinition animation,
        AnimationState beforeState,
        AnimationState afterState)
    {
        _animation = animation;
        _beforeState = beforeState;
        _afterState = afterState;
    }

    public void Execute()
    {
        _afterState.ApplyTo(_animation);
    }

    public void Undo()
    {
        _beforeState.ApplyTo(_animation);
    }

    public void Dispose()
    {
    }
}
