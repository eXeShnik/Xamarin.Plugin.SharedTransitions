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
#if IOS
                            .AddHandler<SharedTransitionNavigationPage, SharedTransitionNavigationRenderer>()
#elif ANDROID
                            .AddHandler<SharedTransitionNavigationPage, SharedTransitionNavigationRendererNew>()
#endif
                            .AddHandler<SharedTransitionShell, SharedTransitionShellRenderer>();
                    }
                    else
                    {
                        collection
#if IOS
                            .AddHandler<NavigationPage, SharedTransitionNavigationRenderer>()
#elif ANDROID
                            .AddHandler<NavigationPage, SharedTransitionNavigationRendererNew>()
#endif
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