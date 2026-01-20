using SpriteSheetEditor.Models;
using SpriteSheetEditor.ViewModels;
using Xunit;

namespace SpriteSheetEditor.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void NewDocument_ShouldInitializeWithDefaults()
    {
        var vm = new MainViewModel();

        Assert.NotNull(vm.Document);
        Assert.Equal("untitled", vm.Document.SpriteSheetName);
        Assert.Empty(vm.Document.Sprites);
    }

    [Fact]
    public void SelectedSprite_ShouldBeNullInitially()
    {
        var vm = new MainViewModel();

        Assert.Null(vm.SelectedSprite);
    }

    [Fact]
    public void CurrentTool_ShouldDefaultToSelect()
    {
        var vm = new MainViewModel();

        Assert.Equal(EditorTool.Select, vm.CurrentTool);
    }

    [Fact]
    public void AddSprite_ShouldAddToDocumentAndSelect()
    {
        var vm = new MainViewModel();
        var sprite = new SpriteDefinition { Name = "test", X = 0, Y = 0, Width = 64, Height = 64 };

        vm.AddSprite(sprite);

        Assert.Contains(sprite, vm.Document.Sprites);
        Assert.Equal(sprite, vm.SelectedSprite);
    }

    [Fact]
    public void DeleteSelectedSprite_ShouldRemoveAndClearSelection()
    {
        var vm = new MainViewModel();
        var sprite = new SpriteDefinition { Name = "test", X = 0, Y = 0, Width = 64, Height = 64 };
        vm.AddSprite(sprite);

        vm.DeleteSelectedSprite();

        Assert.Empty(vm.Document.Sprites);
        Assert.Null(vm.SelectedSprite);
    }

    [Fact]
    public void ZoomLevel_ShouldDefaultToOne()
    {
        var vm = new MainViewModel();

        Assert.Equal(1.0f, vm.ZoomLevel);
    }

    [Fact]
    public void ImageSize_ShouldBeZeroWithNoImage()
    {
        var vm = new MainViewModel();

        Assert.Equal(0, vm.ImageWidth);
        Assert.Equal(0, vm.ImageHeight);
    }
}
