using Plugin.SharedTransitions;

namespace Example.Maui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // MainPage = new AppShell();
        MainPage = new SharedTransitionNavigationPage(new MainPage());
    }
}