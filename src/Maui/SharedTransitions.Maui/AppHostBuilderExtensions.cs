using Plugin.SharedTransitions;

#if IOS
using Plugin.SharedTransitions.Platforms.iOS.Renderers;
#elif ANDROID
using Plugin.SharedTransitions.Platforms.Android.Renderers;
using Plugin.SharedTransitions.Platforms.Android.Renderers.Copy;
using Plugin.SharedTransitions.Platforms.Android.Renderers.New;
#endif

namespace SharedTransitions.Maui;

public static class AppHostBuilderExtensions
{
    public static MauiAppBuilder ConfigureSharedTransitionsPlugin(this MauiAppBuilder builder, bool replaceDefaultHandlers = false)
    {
        builder
            .ConfigureMauiHandlers(
                collection =>
                {
                    if (!replaceDefaultHandlers)
                    {
                        collection
                            .AddHandler<SharedTransitionNavigationPage, SharedTransitionNavigationRenderer>()
                            .AddHandler<SharedTransitionShell, SharedTransitionShellRenderer>();
                    }
                    else
                    {
                        collection
                            .AddHandler<NavigationPage, SharedTransitionNavigationRenderer>()
                            .AddHandler<Shell, SharedTransitionShellRenderer>();
                    }

                    collection.AddHandler<Page, SharedTransitionPageRenderer>();
                }
            )
            .ConfigureEffects(
                effects =>
                {
                    effects.Add<TransitionEffect, PlatformTransitionEffect>();
                }
            );
        ;

        return builder;
    }
}