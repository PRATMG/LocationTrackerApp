using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace LocationTrackerApp;

public partial class App : Application
{
    private readonly Page _rootPage;

    public App(MainPage mainPage)
    {
        InitializeComponent();     // must match x:Class above
        _rootPage = mainPage;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(_rootPage);
}
