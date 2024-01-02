using Microsoft.Maui.Handlers;
using View = Android.Views.View;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.New;

public class SharedTransitionNavigationRendererNew : NavigationViewHandler
{
    protected override View CreatePlatformView()
    {
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
        var field = typeof(NavigationViewHandler).GetField("_stackNavigationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field!.SetValue(this, new StackNavigationManagerNew(MauiContext!));
        return base.CreatePlatformView();
    }
}