# SpriteSheet Editor Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a visual MAUI tool for creating and editing spritesheet JSON definition files.

**Architecture:** MVVM pattern with SkiaSharp canvas for rendering. MainViewModel holds document state, SpriteCanvas handles all drawing and input, JsonExporter handles file I/O matching existing Solo engine format.

**Tech Stack:** .NET 8, MAUI, SkiaSharp.Views.Maui.Controls, System.Text.Json, CommunityToolkit.Mvvm

---

## Task 1: Project Setup

**Files:**
- Create: `tools/SpriteSheetEditor/SpriteSheetEditor.csproj`
- Create: `tools/SpriteSheetEditor/App.xaml`
- Create: `tools/SpriteSheetEditor/App.xaml.cs`
- Create: `tools/SpriteSheetEditor/MauiProgram.cs`
- Create: `tools/SpriteSheetEditor/MainPage.xaml`
- Create: `tools/SpriteSheetEditor/MainPage.xaml.cs`
- Modify: `Solo.sln` (add project reference)

**Step 1: Create project file**

Create `tools/SpriteSheetEditor/SpriteSheetEditor.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0-windows10.0.19041.0</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <RootNamespace>SpriteSheetEditor</RootNamespace>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <EnableDefaultCssItems>false</EnableDefaultCssItems>
    <ApplicationTitle>SpriteSheet Editor</ApplicationTitle>
    <ApplicationId>com.solo.spritesheeteditor</ApplicationId>
    <ApplicationVersion>1</ApplicationVersion>
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</SupportedOSPlatformVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageReference Include="SkiaSharp.Views.Maui.Controls" Version="2.88.7" />
  </ItemGroup>

</Project>
```

**Step 2: Create MauiProgram.cs**

Create `tools/SpriteSheetEditor/MauiProgram.cs`:

```csharp
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace SpriteSheetEditor;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

**Step 3: Create App.xaml**

Create `tools/SpriteSheetEditor/App.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SpriteSheetEditor.App">
</Application>
```

**Step 4: Create App.xaml.cs**

Create `tools/SpriteSheetEditor/App.xaml.cs`:

```csharp
namespace SpriteSheetEditor;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }
}
```

**Step 5: Create MainPage.xaml placeholder**

Create `tools/SpriteSheetEditor/MainPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="SpriteSheetEditor.MainPage"
             Title="SpriteSheet Editor">
    <Label Text="SpriteSheet Editor - Setup Complete"
           VerticalOptions="Center"
           HorizontalOptions="Center" />
</ContentPage>
```

**Step 6: Create MainPage.xaml.cs**

Create `tools/SpriteSheetEditor/MainPage.xaml.cs`:

```csharp
namespace SpriteSheetEditor;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
}
```

**Step 7: Add project to solution**

Run:
```bash
dotnet sln Solo.sln add tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```

**Step 8: Build and verify**

Run:
```bash
dotnet build tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```
Expected: Build succeeds

**Step 9: Commit**

```bash
git add tools/SpriteSheetEditor/ Solo.sln
git commit -m "feat(tools): scaffold SpriteSheet Editor MAUI project"
```

---

## Task 2: Data Models

**Files:**
- Create: `tools/SpriteSheetEditor/Models/SpriteDefinition.cs`
- Create: `tools/SpriteSheetEditor/Models/SpriteSheetDocument.cs`
- Create: `tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj`
- Create: `tools/SpriteSheetEditor.Tests/Models/SpriteDefinitionTests.cs`

**Step 1: Create test project**

Create `tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.7">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SpriteSheetEditor\SpriteSheetEditor.csproj" />
  </ItemGroup>

</Project>
```

**Step 2: Write failing test for SpriteDefinition**

Create `tools/SpriteSheetEditor.Tests/Models/SpriteDefinitionTests.cs`:

```csharp
using FluentAssertions;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Tests.Models;

public class SpriteDefinitionTests
{
    [Fact]
    public void SpriteDefinition_ShouldStoreAllProperties()
    {
        var sprite = new SpriteDefinition
        {
            Name = "test_sprite",
            X = 10,
            Y = 20,
            Width = 64,
            Height = 128
        };

        sprite.Name.Should().Be("test_sprite");
        sprite.X.Should().Be(10);
        sprite.Y.Should().Be(20);
        sprite.Width.Should().Be(64);
        sprite.Height.Should().Be(128);
    }

    [Fact]
    public void SpriteDefinition_Bounds_ShouldReturnCorrectRectangle()
    {
        var sprite = new SpriteDefinition
        {
            X = 10,
            Y = 20,
            Width = 64,
            Height = 128
        };

        var bounds = sprite.Bounds;

        bounds.X.Should().Be(10);
        bounds.Y.Should().Be(20);
        bounds.Width.Should().Be(64);
        bounds.Height.Should().Be(128);
    }

    [Fact]
    public void SpriteDefinition_ContainsPoint_ShouldReturnTrueForPointInside()
    {
        var sprite = new SpriteDefinition { X = 10, Y = 20, Width = 64, Height = 128 };

        sprite.ContainsPoint(50, 80).Should().BeTrue();
        sprite.ContainsPoint(10, 20).Should().BeTrue();  // top-left corner
        sprite.ContainsPoint(73, 147).Should().BeTrue(); // bottom-right - 1
    }

    [Fact]
    public void SpriteDefinition_ContainsPoint_ShouldReturnFalseForPointOutside()
    {
        var sprite = new SpriteDefinition { X = 10, Y = 20, Width = 64, Height = 128 };

        sprite.ContainsPoint(5, 80).Should().BeFalse();   // left of bounds
        sprite.ContainsPoint(80, 80).Should().BeFalse();  // right of bounds
        sprite.ContainsPoint(50, 10).Should().BeFalse();  // above bounds
        sprite.ContainsPoint(50, 200).Should().BeFalse(); // below bounds
    }
}
```

**Step 3: Run test to verify it fails**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: FAIL - SpriteDefinition class not found

**Step 4: Implement SpriteDefinition**

Create `tools/SpriteSheetEditor/Models/SpriteDefinition.cs`:

```csharp
using System.Drawing;
using System.Text.Json.Serialization;

namespace SpriteSheetEditor.Models;

public class SpriteDefinition
{
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    [JsonIgnore]
    public Rectangle Bounds => new(X, Y, Width, Height);

    public bool ContainsPoint(int px, int py)
    {
        return px >= X && px < X + Width && py >= Y && py < Y + Height;
    }
}
```

**Step 5: Run test to verify it passes**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: PASS

**Step 6: Write test for SpriteSheetDocument**

Create `tools/SpriteSheetEditor.Tests/Models/SpriteSheetDocumentTests.cs`:

```csharp
using FluentAssertions;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Tests.Models;

public class SpriteSheetDocumentTests
{
    [Fact]
    public void SpriteSheetDocument_ShouldInitializeWithEmptySpritesList()
    {
        var doc = new SpriteSheetDocument();

        doc.Sprites.Should().NotBeNull();
        doc.Sprites.Should().BeEmpty();
    }

    [Fact]
    public void SpriteSheetDocument_GenerateSpriteName_ShouldUseSheetNameAndIndex()
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = "avatars" };

        doc.GenerateSpriteName(0).Should().Be("avatars_sprite_0");
        doc.GenerateSpriteName(5).Should().Be("avatars_sprite_5");
    }

    [Fact]
    public void SpriteSheetDocument_GetNextSpriteIndex_ShouldReturnZeroForEmptyList()
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = "test" };

        doc.GetNextSpriteIndex().Should().Be(0);
    }

    [Fact]
    public void SpriteSheetDocument_GetNextSpriteIndex_ShouldReturnNextAfterHighest()
    {
        var doc = new SpriteSheetDocument
        {
            SpriteSheetName = "test",
            Sprites =
            [
                new SpriteDefinition { Name = "test_sprite_0" },
                new SpriteDefinition { Name = "test_sprite_5" },
                new SpriteDefinition { Name = "custom_name" }
            ]
        };

        doc.GetNextSpriteIndex().Should().Be(6);
    }
}
```

**Step 7: Run test to verify it fails**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: FAIL - SpriteSheetDocument class not found

**Step 8: Implement SpriteSheetDocument**

Create `tools/SpriteSheetEditor/Models/SpriteSheetDocument.cs`:

```csharp
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace SpriteSheetEditor.Models;

public partial class SpriteSheetDocument
{
    public string SpriteSheetName { get; set; } = string.Empty;
    public List<SpriteDefinition> Sprites { get; set; } = [];

    [JsonIgnore]
    public SKBitmap? LoadedImage { get; set; }

    [JsonIgnore]
    public string? ImageFilePath { get; set; }

    public string GenerateSpriteName(int index)
    {
        return $"{SpriteSheetName}_sprite_{index}";
    }

    public int GetNextSpriteIndex()
    {
        var maxIndex = -1;
        var pattern = SpriteNamePattern();

        foreach (var sprite in Sprites)
        {
            var match = pattern.Match(sprite.Name);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var index))
            {
                maxIndex = Math.Max(maxIndex, index);
            }
        }

        return maxIndex + 1;
    }

    [GeneratedRegex(@"_sprite_(\d+)$")]
    private static partial Regex SpriteNamePattern();
}
```

**Step 9: Run tests to verify they pass**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: All PASS

**Step 10: Commit**

```bash
git add tools/SpriteSheetEditor/Models/ tools/SpriteSheetEditor.Tests/
git commit -m "feat(tools): add SpriteDefinition and SpriteSheetDocument models"
```

---

## Task 3: JSON Serialization Service

**Files:**
- Create: `tools/SpriteSheetEditor/Services/JsonExporter.cs`
- Create: `tools/SpriteSheetEditor.Tests/Services/JsonExporterTests.cs`

**Step 1: Write failing tests for JsonExporter**

Create `tools/SpriteSheetEditor.Tests/Services/JsonExporterTests.cs`:

```csharp
using FluentAssertions;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.Services;

namespace SpriteSheetEditor.Tests.Services;

public class JsonExporterTests
{
    [Fact]
    public void Serialize_ShouldProduceCorrectJsonFormat()
    {
        var doc = new SpriteSheetDocument
        {
            SpriteSheetName = "avatars",
            Sprites =
            [
                new SpriteDefinition { Name = "warrior", X = 0, Y = 0, Width = 256, Height = 256 },
                new SpriteDefinition { Name = "mage", X = 256, Y = 0, Width = 256, Height = 256 }
            ]
        };

        var json = JsonExporter.Serialize(doc);

        json.Should().Contain("\"spriteSheetName\": \"avatars\"");
        json.Should().Contain("\"name\": \"warrior\"");
        json.Should().Contain("\"x\": 0");
        json.Should().Contain("\"width\": 256");
        json.Should().NotContain("LoadedImage");
        json.Should().NotContain("ImageFilePath");
        json.Should().NotContain("Bounds");
    }

    [Fact]
    public void Deserialize_ShouldLoadCorrectValues()
    {
        var json = """
        {
          "spriteSheetName": "test",
          "sprites": [
            { "name": "sprite1", "x": 10, "y": 20, "width": 64, "height": 128 }
          ]
        }
        """;

        var doc = JsonExporter.Deserialize(json);

        doc.SpriteSheetName.Should().Be("test");
        doc.Sprites.Should().HaveCount(1);
        doc.Sprites[0].Name.Should().Be("sprite1");
        doc.Sprites[0].X.Should().Be(10);
        doc.Sprites[0].Y.Should().Be(20);
        doc.Sprites[0].Width.Should().Be(64);
        doc.Sprites[0].Height.Should().Be(128);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveAllData()
    {
        var original = new SpriteSheetDocument
        {
            SpriteSheetName = "roundtrip_test",
            Sprites =
            [
                new SpriteDefinition { Name = "s1", X = 1, Y = 2, Width = 3, Height = 4 },
                new SpriteDefinition { Name = "s2", X = 5, Y = 6, Width = 7, Height = 8 }
            ]
        };

        var json = JsonExporter.Serialize(original);
        var restored = JsonExporter.Deserialize(json);

        restored.SpriteSheetName.Should().Be(original.SpriteSheetName);
        restored.Sprites.Should().HaveCount(2);
        restored.Sprites[0].Name.Should().Be("s1");
        restored.Sprites[1].X.Should().Be(5);
    }
}
```

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: FAIL - JsonExporter class not found

**Step 3: Implement JsonExporter**

Create `tools/SpriteSheetEditor/Services/JsonExporter.cs`:

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public static class JsonExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(SpriteSheetDocument document)
    {
        return JsonSerializer.Serialize(document, SerializerOptions);
    }

    public static SpriteSheetDocument Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SpriteSheetDocument>(json, SerializerOptions)
               ?? new SpriteSheetDocument();
    }

    public static async Task SaveAsync(SpriteSheetDocument document, string filePath)
    {
        var json = Serialize(document);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<SpriteSheetDocument> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return Deserialize(json);
    }
}
```

**Step 4: Run tests to verify they pass**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: All PASS

**Step 5: Commit**

```bash
git add tools/SpriteSheetEditor/Services/ tools/SpriteSheetEditor.Tests/Services/
git commit -m "feat(tools): add JSON serialization service"
```

---

## Task 4: Main ViewModel

**Files:**
- Create: `tools/SpriteSheetEditor/ViewModels/MainViewModel.cs`
- Create: `tools/SpriteSheetEditor.Tests/ViewModels/MainViewModelTests.cs`

**Step 1: Write failing tests for MainViewModel**

Create `tools/SpriteSheetEditor.Tests/ViewModels/MainViewModelTests.cs`:

```csharp
using FluentAssertions;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.ViewModels;

namespace SpriteSheetEditor.Tests.ViewModels;

public class MainViewModelTests
{
    [Fact]
    public void NewDocument_ShouldInitializeWithDefaults()
    {
        var vm = new MainViewModel();

        vm.Document.Should().NotBeNull();
        vm.Document.SpriteSheetName.Should().Be("untitled");
        vm.Document.Sprites.Should().BeEmpty();
    }

    [Fact]
    public void SelectedSprite_ShouldBeNullInitially()
    {
        var vm = new MainViewModel();

        vm.SelectedSprite.Should().BeNull();
    }

    [Fact]
    public void CurrentTool_ShouldDefaultToSelect()
    {
        var vm = new MainViewModel();

        vm.CurrentTool.Should().Be(EditorTool.Select);
    }

    [Fact]
    public void AddSprite_ShouldAddToDocumentAndSelect()
    {
        var vm = new MainViewModel();
        var sprite = new SpriteDefinition { Name = "test", X = 0, Y = 0, Width = 64, Height = 64 };

        vm.AddSprite(sprite);

        vm.Document.Sprites.Should().Contain(sprite);
        vm.SelectedSprite.Should().Be(sprite);
    }

    [Fact]
    public void DeleteSelectedSprite_ShouldRemoveAndClearSelection()
    {
        var vm = new MainViewModel();
        var sprite = new SpriteDefinition { Name = "test", X = 0, Y = 0, Width = 64, Height = 64 };
        vm.AddSprite(sprite);

        vm.DeleteSelectedSprite();

        vm.Document.Sprites.Should().BeEmpty();
        vm.SelectedSprite.Should().BeNull();
    }

    [Fact]
    public void ZoomLevel_ShouldDefaultToOne()
    {
        var vm = new MainViewModel();

        vm.ZoomLevel.Should().Be(1.0f);
    }

    [Fact]
    public void ImageSize_ShouldBeZeroWithNoImage()
    {
        var vm = new MainViewModel();

        vm.ImageWidth.Should().Be(0);
        vm.ImageHeight.Should().Be(0);
    }
}
```

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: FAIL - MainViewModel class not found

**Step 3: Implement MainViewModel**

Create `tools/SpriteSheetEditor/ViewModels/MainViewModel.cs`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.ViewModels;

public enum EditorTool
{
    Select,
    Draw
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private SpriteSheetDocument _document = new() { SpriteSheetName = "untitled" };

    [ObservableProperty]
    private SpriteDefinition? _selectedSprite;

    [ObservableProperty]
    private EditorTool _currentTool = EditorTool.Select;

    [ObservableProperty]
    private float _zoomLevel = 1.0f;

    [ObservableProperty]
    private float _panOffsetX;

    [ObservableProperty]
    private float _panOffsetY;

    public int ImageWidth => Document.LoadedImage?.Width ?? 0;
    public int ImageHeight => Document.LoadedImage?.Height ?? 0;
    public int SpriteCount => Document.Sprites.Count;

    public void AddSprite(SpriteDefinition sprite)
    {
        Document.Sprites.Add(sprite);
        SelectedSprite = sprite;
        OnPropertyChanged(nameof(SpriteCount));
    }

    public void DeleteSelectedSprite()
    {
        if (SelectedSprite is null) return;

        Document.Sprites.Remove(SelectedSprite);
        SelectedSprite = null;
        OnPropertyChanged(nameof(SpriteCount));
    }

    public void ClearAllSprites()
    {
        Document.Sprites.Clear();
        SelectedSprite = null;
        OnPropertyChanged(nameof(SpriteCount));
    }

    public SpriteDefinition? FindSpriteAt(int x, int y)
    {
        // Search in reverse to find topmost sprite first
        for (int i = Document.Sprites.Count - 1; i >= 0; i--)
        {
            if (Document.Sprites[i].ContainsPoint(x, y))
                return Document.Sprites[i];
        }
        return null;
    }

    public void NotifyImageChanged()
    {
        OnPropertyChanged(nameof(ImageWidth));
        OnPropertyChanged(nameof(ImageHeight));
    }
}
```

**Step 4: Run tests to verify they pass**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: All PASS

**Step 5: Commit**

```bash
git add tools/SpriteSheetEditor/ViewModels/ tools/SpriteSheetEditor.Tests/ViewModels/
git commit -m "feat(tools): add MainViewModel with editor state"
```

---

## Task 5: Grid Generation Logic

**Files:**
- Create: `tools/SpriteSheetEditor/Services/GridGenerator.cs`
- Create: `tools/SpriteSheetEditor.Tests/Services/GridGeneratorTests.cs`

**Step 1: Write failing tests**

Create `tools/SpriteSheetEditor.Tests/Services/GridGeneratorTests.cs`:

```csharp
using FluentAssertions;
using SpriteSheetEditor.Services;

namespace SpriteSheetEditor.Tests.Services;

public class GridGeneratorTests
{
    [Fact]
    public void Generate_ShouldCreateCorrectNumberOfSprites()
    {
        var result = GridGenerator.Generate("test", 1024, 1024, columns: 4, rows: 4);

        result.Sprites.Should().HaveCount(16);
    }

    [Fact]
    public void Generate_ShouldCalculateCorrectTileSize()
    {
        var result = GridGenerator.Generate("test", 1024, 512, columns: 4, rows: 2);

        result.Sprites[0].Width.Should().Be(256);
        result.Sprites[0].Height.Should().Be(256);
    }

    [Fact]
    public void Generate_ShouldPositionSpritesCorrectly()
    {
        var result = GridGenerator.Generate("test", 200, 100, columns: 2, rows: 2);

        // Row 0
        result.Sprites[0].X.Should().Be(0);
        result.Sprites[0].Y.Should().Be(0);
        result.Sprites[1].X.Should().Be(100);
        result.Sprites[1].Y.Should().Be(0);
        // Row 1
        result.Sprites[2].X.Should().Be(0);
        result.Sprites[2].Y.Should().Be(50);
        result.Sprites[3].X.Should().Be(100);
        result.Sprites[3].Y.Should().Be(50);
    }

    [Fact]
    public void Generate_ShouldNameSpritesSequentially()
    {
        var result = GridGenerator.Generate("avatars", 256, 256, columns: 2, rows: 2);

        result.Sprites[0].Name.Should().Be("avatars_sprite_0");
        result.Sprites[1].Name.Should().Be("avatars_sprite_1");
        result.Sprites[2].Name.Should().Be("avatars_sprite_2");
        result.Sprites[3].Name.Should().Be("avatars_sprite_3");
    }

    [Fact]
    public void CalculateTileSize_ShouldReturnIntegerDivision()
    {
        var (width, height) = GridGenerator.CalculateTileSize(1024, 768, 3, 4);

        width.Should().Be(341);  // 1024 / 3 = 341.33...
        height.Should().Be(192); // 768 / 4 = 192
    }

    [Fact]
    public void GetUncoveredPixels_ShouldReturnRemainderPixels()
    {
        var (uncoveredX, uncoveredY) = GridGenerator.GetUncoveredPixels(1024, 768, 3, 4);

        uncoveredX.Should().Be(1); // 1024 % 3 = 1
        uncoveredY.Should().Be(0); // 768 % 4 = 0
    }

    [Fact]
    public void HasUncoveredPixels_ShouldReturnTrueWhenNotEvenlyDivisible()
    {
        GridGenerator.HasUncoveredPixels(1024, 768, 3, 4).Should().BeTrue();
        GridGenerator.HasUncoveredPixels(1024, 1024, 4, 4).Should().BeFalse();
    }
}
```

**Step 2: Run test to verify it fails**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: FAIL - GridGenerator class not found

**Step 3: Implement GridGenerator**

Create `tools/SpriteSheetEditor/Services/GridGenerator.cs`:

```csharp
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public static class GridGenerator
{
    public static SpriteSheetDocument Generate(string sheetName, int imageWidth, int imageHeight, int columns, int rows)
    {
        var (tileWidth, tileHeight) = CalculateTileSize(imageWidth, imageHeight, columns, rows);

        var doc = new SpriteSheetDocument { SpriteSheetName = sheetName };
        var index = 0;

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                doc.Sprites.Add(new SpriteDefinition
                {
                    Name = $"{sheetName}_sprite_{index}",
                    X = col * tileWidth,
                    Y = row * tileHeight,
                    Width = tileWidth,
                    Height = tileHeight
                });
                index++;
            }
        }

        return doc;
    }

    public static (int width, int height) CalculateTileSize(int imageWidth, int imageHeight, int columns, int rows)
    {
        return (imageWidth / columns, imageHeight / rows);
    }

    public static (int uncoveredX, int uncoveredY) GetUncoveredPixels(int imageWidth, int imageHeight, int columns, int rows)
    {
        return (imageWidth % columns, imageHeight % rows);
    }

    public static bool HasUncoveredPixels(int imageWidth, int imageHeight, int columns, int rows)
    {
        var (uncoveredX, uncoveredY) = GetUncoveredPixels(imageWidth, imageHeight, columns, rows);
        return uncoveredX > 0 || uncoveredY > 0;
    }
}
```

**Step 4: Run tests to verify they pass**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: All PASS

**Step 5: Commit**

```bash
git add tools/SpriteSheetEditor/Services/GridGenerator.cs tools/SpriteSheetEditor.Tests/Services/GridGeneratorTests.cs
git commit -m "feat(tools): add grid generation logic"
```

---

## Task 6: Sprite Canvas Control

**Files:**
- Create: `tools/SpriteSheetEditor/Controls/SpriteCanvas.cs`

**Step 1: Create the SkiaSharp canvas control**

Create `tools/SpriteSheetEditor/Controls/SpriteCanvas.cs`:

```csharp
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using SpriteSheetEditor.Models;
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

    // Drawing state
    private bool _isDrawing;
    private SKPoint _drawStart;
    private SKPoint _drawCurrent;

    // Drag state
    private bool _isDragging;
    private SpriteDefinition? _dragSprite;
    private SKPoint _dragOffset;

    // Resize state
    private bool _isResizing;
    private int _resizeHandle = -1; // 0=TL, 1=TR, 2=BR, 3=BL

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
            new SKPoint(rect.Left, rect.Top),      // 0: TL
            new SKPoint(rect.Right, rect.Top),     // 1: TR
            new SKPoint(rect.Right, rect.Bottom),  // 2: BR
            new SKPoint(rect.Left, rect.Bottom)    // 3: BL
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

        if (ViewModel.CurrentTool == EditorTool.Draw)
        {
            _isDrawing = true;
            _drawStart = imagePoint;
            _drawCurrent = imagePoint;
        }
        else // Select tool
        {
            // Check resize handles first
            if (ViewModel.SelectedSprite is { } selected)
            {
                var rect = new SKRect(selected.X, selected.Y, selected.X + selected.Width, selected.Y + selected.Height);
                var handles = GetHandlePositions(rect);
                for (var i = 0; i < handles.Length; i++)
                {
                    if (IsNearPoint(imagePoint, handles[i], HandleSize))
                    {
                        _isResizing = true;
                        _resizeHandle = i;
                        return;
                    }
                }
            }

            // Check sprite selection/drag
            var sprite = ViewModel.FindSpriteAt((int)imagePoint.X, (int)imagePoint.Y);
            if (sprite != null)
            {
                ViewModel.SelectedSprite = sprite;
                _isDragging = true;
                _dragSprite = sprite;
                _dragOffset = new SKPoint(imagePoint.X - sprite.X, imagePoint.Y - sprite.Y);
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
            case 0: // TL
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
            case 1: // TR
                var newWidth1 = x - sprite.X;
                var newHeight1 = sprite.Y + sprite.Height - y;
                if (newWidth1 > 0 && newHeight1 > 0)
                {
                    sprite.Y = y;
                    sprite.Width = newWidth1;
                    sprite.Height = newHeight1;
                }
                break;
            case 2: // BR
                var newWidth2 = x - sprite.X;
                var newHeight2 = y - sprite.Y;
                if (newWidth2 > 0 && newHeight2 > 0)
                {
                    sprite.Width = newWidth2;
                    sprite.Height = newHeight2;
                }
                break;
            case 3: // BL
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
                ViewModel.AddSprite(sprite);
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
    }

    private static bool IsNearPoint(SKPoint a, SKPoint b, float threshold)
    {
        return Math.Abs(a.X - b.X) <= threshold && Math.Abs(a.Y - b.Y) <= threshold;
    }

    public void ZoomIn() { if (ViewModel != null) ViewModel.ZoomLevel = Math.Min(ViewModel.ZoomLevel * 1.25f, 10f); }
    public void ZoomOut() { if (ViewModel != null) ViewModel.ZoomLevel = Math.Max(ViewModel.ZoomLevel / 1.25f, 0.1f); }

    public void FitToWindow()
    {
        if (ViewModel is null || ViewModel.ImageWidth == 0) return;

        var scaleX = (float)Width / ViewModel.ImageWidth;
        var scaleY = (float)Height / ViewModel.ImageHeight;
        ViewModel.ZoomLevel = Math.Min(scaleX, scaleY) * 0.9f;
        ViewModel.PanOffsetX = (float)(Width - ViewModel.ImageWidth * ViewModel.ZoomLevel) / 2;
        ViewModel.PanOffsetY = (float)(Height - ViewModel.ImageHeight * ViewModel.ZoomLevel) / 2;
    }
}
```

**Step 2: Build and verify**

Run:
```bash
dotnet build tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```
Expected: Build succeeds

**Step 3: Commit**

```bash
git add tools/SpriteSheetEditor/Controls/
git commit -m "feat(tools): add SpriteCanvas control with draw/select/resize"
```

---

## Task 7: Main UI Layout

**Files:**
- Modify: `tools/SpriteSheetEditor/MainPage.xaml`
- Modify: `tools/SpriteSheetEditor/MainPage.xaml.cs`

**Step 1: Update MainPage.xaml with full layout**

Replace `tools/SpriteSheetEditor/MainPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:controls="clr-namespace:SpriteSheetEditor.Controls"
             xmlns:vm="clr-namespace:SpriteSheetEditor.ViewModels"
             x:Class="SpriteSheetEditor.MainPage"
             Title="SpriteSheet Editor">

    <Grid RowDefinitions="Auto,*,Auto">
        <!-- Toolbar -->
        <HorizontalStackLayout Grid.Row="0" Padding="8" Spacing="8" BackgroundColor="#2d2d30">
            <Button Text="Open Image" Clicked="OnOpenImageClicked" />
            <Button Text="Open JSON" Clicked="OnOpenJsonClicked" />
            <Button Text="Save JSON" Clicked="OnSaveJsonClicked" />
            <BoxView WidthRequest="1" BackgroundColor="#555" />
            <Button x:Name="SelectToolButton" Text="Select" Clicked="OnSelectToolClicked" BackgroundColor="#0078d4" />
            <Button x:Name="DrawToolButton" Text="Draw" Clicked="OnDrawToolClicked" />
            <BoxView WidthRequest="1" BackgroundColor="#555" />
            <Button Text="Grid..." Clicked="OnGridClicked" />
            <Button Text="Fit" Clicked="OnFitClicked" />
        </HorizontalStackLayout>

        <!-- Main Content -->
        <Grid Grid.Row="1" ColumnDefinitions="*,280">
            <!-- Canvas -->
            <controls:SpriteCanvas x:Name="Canvas" Grid.Column="0" ViewModel="{Binding}" />

            <!-- Right Panel -->
            <Grid Grid.Column="1" RowDefinitions="*,Auto,*" BackgroundColor="#252526">
                <!-- Sprite List -->
                <Frame Grid.Row="0" Margin="8" Padding="8" BackgroundColor="#1e1e1e" BorderColor="#3e3e42">
                    <Grid RowDefinitions="Auto,*">
                        <Label Text="Sprites" FontAttributes="Bold" TextColor="White" />
                        <CollectionView Grid.Row="1" x:Name="SpriteListView"
                                        ItemsSource="{Binding Document.Sprites}"
                                        SelectedItem="{Binding SelectedSprite, Mode=TwoWay}"
                                        SelectionMode="Single">
                            <CollectionView.ItemTemplate>
                                <DataTemplate>
                                    <Label Text="{Binding Name}" TextColor="White" Padding="4" />
                                </DataTemplate>
                            </CollectionView.ItemTemplate>
                        </CollectionView>
                    </Grid>
                </Frame>

                <BoxView Grid.Row="1" HeightRequest="1" BackgroundColor="#3e3e42" />

                <!-- Properties Panel -->
                <Frame Grid.Row="2" Margin="8" Padding="8" BackgroundColor="#1e1e1e" BorderColor="#3e3e42">
                    <VerticalStackLayout Spacing="8">
                        <Label Text="Properties" FontAttributes="Bold" TextColor="White" />

                        <Label Text="Name:" TextColor="#ccc" />
                        <Entry x:Name="NameEntry" Text="{Binding SelectedSprite.Name, Mode=TwoWay}"
                               BackgroundColor="#333" TextColor="White" />

                        <Grid ColumnDefinitions="*,*" ColumnSpacing="8">
                            <VerticalStackLayout Grid.Column="0">
                                <Label Text="X:" TextColor="#ccc" />
                                <Entry x:Name="XEntry" Text="{Binding SelectedSprite.X, Mode=TwoWay}"
                                       Keyboard="Numeric" BackgroundColor="#333" TextColor="White" />
                            </VerticalStackLayout>
                            <VerticalStackLayout Grid.Column="1">
                                <Label Text="Y:" TextColor="#ccc" />
                                <Entry x:Name="YEntry" Text="{Binding SelectedSprite.Y, Mode=TwoWay}"
                                       Keyboard="Numeric" BackgroundColor="#333" TextColor="White" />
                            </VerticalStackLayout>
                        </Grid>

                        <Grid ColumnDefinitions="*,*" ColumnSpacing="8">
                            <VerticalStackLayout Grid.Column="0">
                                <Label Text="Width:" TextColor="#ccc" />
                                <Entry x:Name="WidthEntry" Text="{Binding SelectedSprite.Width, Mode=TwoWay}"
                                       Keyboard="Numeric" BackgroundColor="#333" TextColor="White" />
                            </VerticalStackLayout>
                            <VerticalStackLayout Grid.Column="1">
                                <Label Text="Height:" TextColor="#ccc" />
                                <Entry x:Name="HeightEntry" Text="{Binding SelectedSprite.Height, Mode=TwoWay}"
                                       Keyboard="Numeric" BackgroundColor="#333" TextColor="White" />
                            </VerticalStackLayout>
                        </Grid>

                        <Button Text="Delete Sprite" Clicked="OnDeleteSpriteClicked"
                                BackgroundColor="#c42b1c" TextColor="White" />
                    </VerticalStackLayout>
                </Frame>
            </Grid>
        </Grid>

        <!-- Status Bar -->
        <HorizontalStackLayout Grid.Row="2" Padding="8,4" Spacing="16" BackgroundColor="#007acc">
            <Label x:Name="ZoomLabel" Text="Zoom: 100%" TextColor="White" />
            <Label x:Name="ImageSizeLabel" Text="Image: --" TextColor="White" />
            <Label x:Name="SpriteCountLabel" Text="Sprites: 0" TextColor="White" />
        </HorizontalStackLayout>
    </Grid>

</ContentPage>
```

**Step 2: Update MainPage.xaml.cs**

Replace `tools/SpriteSheetEditor/MainPage.xaml.cs`:

```csharp
using SkiaSharp;
using SpriteSheetEditor.Services;
using SpriteSheetEditor.ViewModels;

namespace SpriteSheetEditor;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateStatusBar();
        UpdateToolButtons();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        ZoomLabel.Text = $"Zoom: {_viewModel.ZoomLevel * 100:F0}%";
        ImageSizeLabel.Text = _viewModel.ImageWidth > 0
            ? $"Image: {_viewModel.ImageWidth}x{_viewModel.ImageHeight}"
            : "Image: --";
        SpriteCountLabel.Text = $"Sprites: {_viewModel.SpriteCount}";
    }

    private void UpdateToolButtons()
    {
        SelectToolButton.BackgroundColor = _viewModel.CurrentTool == EditorTool.Select
            ? Color.FromArgb("#0078d4") : Color.FromArgb("#3e3e42");
        DrawToolButton.BackgroundColor = _viewModel.CurrentTool == EditorTool.Draw
            ? Color.FromArgb("#0078d4") : Color.FromArgb("#3e3e42");
    }

    private async void OnOpenImageClicked(object? sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select Image",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".png", ".jpg", ".jpeg", ".bmp"] }
            })
        });

        if (result is null) return;

        using var stream = await result.OpenReadAsync();
        _viewModel.Document.LoadedImage = SKBitmap.Decode(stream);
        _viewModel.Document.ImageFilePath = result.FullPath;
        _viewModel.Document.SpriteSheetName = Path.GetFileNameWithoutExtension(result.FileName);
        _viewModel.NotifyImageChanged();
        Canvas.FitToWindow();
    }

    private async void OnOpenJsonClicked(object? sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select Spritesheet JSON",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".json"] }
            })
        });

        if (result is null) return;

        var doc = await JsonExporter.LoadAsync(result.FullPath);
        doc.LoadedImage = _viewModel.Document.LoadedImage;
        doc.ImageFilePath = _viewModel.Document.ImageFilePath;
        _viewModel.Document = doc;

        var loadImage = await DisplayAlert("Load Image?",
            "Would you like to load an image file for this spritesheet?", "Yes", "No");

        if (loadImage)
        {
            OnOpenImageClicked(sender, e);
        }
    }

    private async void OnSaveJsonClicked(object? sender, EventArgs e)
    {
        var fileName = $"{_viewModel.Document.SpriteSheetName}.json";

#if WINDOWS
        var hwnd = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
        var savePicker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            SuggestedFileName = fileName
        };
        savePicker.FileTypeChoices.Add("JSON", [".json"]);

        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        var file = await savePicker.PickSaveFileAsync();

        if (file is not null)
        {
            await JsonExporter.SaveAsync(_viewModel.Document, file.Path);
            await DisplayAlert("Saved", $"Spritesheet saved to {file.Path}", "OK");
        }
#endif
    }

    private void OnSelectToolClicked(object? sender, EventArgs e)
    {
        _viewModel.CurrentTool = EditorTool.Select;
        UpdateToolButtons();
    }

    private void OnDrawToolClicked(object? sender, EventArgs e)
    {
        _viewModel.CurrentTool = EditorTool.Draw;
        UpdateToolButtons();
    }

    private async void OnGridClicked(object? sender, EventArgs e)
    {
        if (_viewModel.ImageWidth == 0)
        {
            await DisplayAlert("No Image", "Please load an image first.", "OK");
            return;
        }

        var columnsStr = await DisplayPromptAsync("Grid Generation", "Number of columns:", initialValue: "4", keyboard: Keyboard.Numeric);
        if (string.IsNullOrEmpty(columnsStr) || !int.TryParse(columnsStr, out var columns) || columns < 1) return;

        var rowsStr = await DisplayPromptAsync("Grid Generation", "Number of rows:", initialValue: "4", keyboard: Keyboard.Numeric);
        if (string.IsNullOrEmpty(rowsStr) || !int.TryParse(rowsStr, out var rows) || rows < 1) return;

        var (tileWidth, tileHeight) = GridGenerator.CalculateTileSize(_viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows);
        var totalSprites = columns * rows;

        var message = $"Tile size: {tileWidth}x{tileHeight}\nWill create {totalSprites} sprites.";

        if (GridGenerator.HasUncoveredPixels(_viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows))
        {
            var (uncoveredX, uncoveredY) = GridGenerator.GetUncoveredPixels(_viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows);
            message += $"\n\n⚠️ Warning: {uncoveredX} pixel(s) horizontally and {uncoveredY} pixel(s) vertically will be uncovered.";
        }

        if (_viewModel.SpriteCount > 0)
        {
            message += $"\n\nThis will replace all {_viewModel.SpriteCount} existing sprites.";
        }

        var proceed = await DisplayAlert("Generate Grid", message, "Generate", "Cancel");
        if (!proceed) return;

        var newDoc = GridGenerator.Generate(_viewModel.Document.SpriteSheetName, _viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows);
        newDoc.LoadedImage = _viewModel.Document.LoadedImage;
        newDoc.ImageFilePath = _viewModel.Document.ImageFilePath;
        _viewModel.Document = newDoc;
        _viewModel.SelectedSprite = null;
    }

    private void OnFitClicked(object? sender, EventArgs e)
    {
        Canvas.FitToWindow();
    }

    private void OnDeleteSpriteClicked(object? sender, EventArgs e)
    {
        _viewModel.DeleteSelectedSprite();
    }
}
```

**Step 3: Build and verify**

Run:
```bash
dotnet build tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```
Expected: Build succeeds

**Step 4: Run the application**

Run:
```bash
dotnet run --project tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```
Expected: Application launches with toolbar, canvas, and properties panel

**Step 5: Commit**

```bash
git add tools/SpriteSheetEditor/MainPage.xaml tools/SpriteSheetEditor/MainPage.xaml.cs
git commit -m "feat(tools): implement main UI layout with toolbar and properties panel"
```

---

## Task 8: Keyboard Shortcuts

**Files:**
- Modify: `tools/SpriteSheetEditor/MainPage.xaml.cs`

**Step 1: Add keyboard handling**

Add to `MainPage.xaml.cs` constructor after `UpdateToolButtons()`:

```csharp
// Add keyboard handler
this.Focused += (s, e) => Focus();
```

Add new method to `MainPage.xaml.cs`:

```csharp
protected override void OnHandlerChanged()
{
    base.OnHandlerChanged();

#if WINDOWS
    var nativeWindow = (Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement)?.XamlRoot?.Content;
    if (nativeWindow is Microsoft.UI.Xaml.UIElement element)
    {
        element.KeyDown += OnKeyDown;
    }
#endif
}

#if WINDOWS
private async void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
{
    var ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
        .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
    var shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
        .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

    switch (e.Key)
    {
        case Windows.System.VirtualKey.O when ctrl && shift:
            OnOpenJsonClicked(null, EventArgs.Empty);
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.O when ctrl:
            OnOpenImageClicked(null, EventArgs.Empty);
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.S when ctrl:
            OnSaveJsonClicked(null, EventArgs.Empty);
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.G when ctrl:
            OnGridClicked(null, EventArgs.Empty);
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Number0 when ctrl:
            Canvas.FitToWindow();
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Number1:
            _viewModel.CurrentTool = EditorTool.Select;
            UpdateToolButtons();
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Number2:
            _viewModel.CurrentTool = EditorTool.Draw;
            UpdateToolButtons();
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Delete:
            _viewModel.DeleteSelectedSprite();
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Escape:
            _viewModel.SelectedSprite = null;
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Add:
        case (Windows.System.VirtualKey)187: // = key (+ without shift)
            Canvas.ZoomIn();
            e.Handled = true;
            break;
        case Windows.System.VirtualKey.Subtract:
        case (Windows.System.VirtualKey)189: // - key
            Canvas.ZoomOut();
            e.Handled = true;
            break;
    }
}
#endif
```

**Step 2: Build and test**

Run:
```bash
dotnet build tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```
Expected: Build succeeds

**Step 3: Commit**

```bash
git add tools/SpriteSheetEditor/MainPage.xaml.cs
git commit -m "feat(tools): add keyboard shortcuts"
```

---

## Task 9: Add Solution Test Project Reference

**Files:**
- Modify: `Solo.sln`

**Step 1: Add test project to solution**

Run:
```bash
dotnet sln Solo.sln add tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```

**Step 2: Run all tests**

Run:
```bash
dotnet test tools/SpriteSheetEditor.Tests/SpriteSheetEditor.Tests.csproj
```
Expected: All tests pass

**Step 3: Commit**

```bash
git add Solo.sln
git commit -m "chore: add SpriteSheetEditor.Tests to solution"
```

---

## Task 10: Final Integration Test

**Step 1: Build entire solution**

Run:
```bash
dotnet build Solo.sln
```
Expected: Build succeeds with no errors

**Step 2: Run the application and verify functionality**

Run:
```bash
dotnet run --project tools/SpriteSheetEditor/SpriteSheetEditor.csproj
```

Manual test checklist:
- [ ] Open an image file (PNG/JPG)
- [ ] Generate grid (4x4)
- [ ] Select a sprite from the list
- [ ] Rename sprite in properties panel
- [ ] Drag sprite to new position
- [ ] Resize sprite using handles
- [ ] Draw a new sprite with Draw tool
- [ ] Delete a sprite
- [ ] Save JSON file
- [ ] Open the saved JSON file
- [ ] Verify all keyboard shortcuts work

**Step 3: Final commit**

```bash
git add -A
git commit -m "feat(tools): complete SpriteSheet Editor MVP"
```

---

## Summary

The SpriteSheet Editor is now complete with:
- MAUI application with SkiaSharp canvas
- Full MVVM architecture
- JSON import/export matching existing format
- Grid generation with column/row input
- Manual rectangle drawing
- Sprite selection, move, resize
- Properties panel with editable fields
- Keyboard shortcuts
- Comprehensive test coverage
