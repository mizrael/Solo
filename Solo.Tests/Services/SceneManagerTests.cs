using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Solo.Services;
using Xunit;

namespace Solo.Tests.Services;

public sealed class SceneManagerTests
{
    private readonly SceneManager _manager = new();

    [Fact]
    public void Scenes_WhenEmpty_YieldsEmptySequence()
    {
        Assert.Empty(_manager.Scenes);
        Assert.Null(_manager.Scenes.OfType<TestScene>().FirstOrDefault());
        Assert.Null(_manager.Current);
    }

    [Fact]
    public void Scenes_WhenScenesArePushed_EnumeratesTopToBottom()
    {
        var bottom = PushScene<TestScene>("bottom");
        var middle = PushScene<OverlayScene>("middle");
        var top = PushScene<OverlayScene>("top");

        Assert.Equal(new Scene[] { top, middle, bottom }, _manager.Scenes);
    }

    [Fact]
    public void Scenes_WhenSceneIsBeneathOverlays_TypedLookupFindsIt()
    {
        var scene = PushScene<TestScene>("scene");
        PushScene<OverlayScene>("first-overlay");
        var top = PushScene<OverlayScene>("second-overlay");

        var result = _manager.Scenes.OfType<TestScene>().FirstOrDefault();

        Assert.Same(scene, result);
        Assert.Same(top, _manager.Current);
        Assert.Equal(3, _manager.Scenes.Count());
    }

    [Fact]
    public void Scenes_WhenMultipleScenesMatch_TypedLookupReturnsTopmost()
    {
        PushScene<TestScene>("bottom");
        var topmostMatch = PushScene<TestScene>("middle");
        PushScene<OverlayScene>("top");

        Assert.Same(topmostMatch, _manager.Scenes.OfType<TestScene>().FirstOrDefault());
    }

    [Fact]
    public void Scenes_WhenNoSceneMatches_TypedLookupReturnsNull()
    {
        PushScene<OverlayScene>("overlay");

        Assert.Null(_manager.Scenes.OfType<TestScene>().FirstOrDefault());
    }

    [Fact]
    public void Scenes_WhenRetained_ReflectsSubsequentPushAndPop()
    {
        var scenes = _manager.Scenes;
        Assert.Empty(scenes);

        var bottom = PushScene<TestScene>("bottom");
        var top = PushScene<OverlayScene>("top");
        Assert.Equal(new Scene[] { top, bottom }, scenes);

        _manager.PopScene();
        Assert.Same(bottom, Assert.Single(scenes));

        _manager.PopScene();
        Assert.Empty(scenes);
    }

    [Fact]
    public void Scenes_WhenAccessed_DoesNotExposeMutableStack()
    {
        PushScene<TestScene>("scene");

        Assert.IsNotType<Stack<Scene>>(_manager.Scenes);
        Assert.False(_manager.Scenes is ICollection<Scene>);
        Assert.False(_manager.Scenes is System.Collections.IList);
    }

    [Fact]
    public void Current_WhenScenesArePushedAndPopped_RemainsTopOrNull()
    {
        var bottom = PushScene<TestScene>("bottom");
        Assert.Same(bottom, _manager.Current);

        var top = PushScene<OverlayScene>("top");
        Assert.Same(top, _manager.Current);

        _manager.PopScene();
        Assert.Same(bottom, _manager.Current);

        _manager.PopScene();
        Assert.Null(_manager.Current);
    }

    private TScene PushScene<TScene>(string name) where TScene : Scene
    {
        // These inert fixtures bypass graphics initialization; only stack operations are tested.
        var scene = (TScene)RuntimeHelpers.GetUninitializedObject(typeof(TScene));
        _manager.AddScene(name, scene);
        _manager.PushScene(name);
        return scene;
    }

    private sealed class TestScene(Game game) : Scene(game);

    private sealed class OverlayScene(Game game) : Scene(game);
}
