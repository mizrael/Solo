using Microsoft.UI.Xaml;

namespace SpriteSheetEditor.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => SpriteSheetEditor.MauiProgram.CreateMauiApp();
}
