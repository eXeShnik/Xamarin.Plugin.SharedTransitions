using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers;

public class SharedTransitionPageRenderer : PageHandler
{
    protected override void DisconnectHandler(ContentViewGroup platformView)
    {
#if IOS
        if (Application.Current != null && Application.Current.MainPage is ISharedTransitionContainer shellPage)
        {
            shellPage.TransitionMap.RemoveFromMap((Page)VirtualView);
        }

        if (VirtualView.Parent is ISharedTransitionContainer navPage)
        {
            navPage.TransitionMap.RemoveFromMap((Page)VirtualView);
        }
#endif

        base.DisconnectHandler(platformView);
    }
}