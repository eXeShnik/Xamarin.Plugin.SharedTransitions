using Microsoft.Maui.Handlers;
using ContentView = Microsoft.Maui.Platform.ContentView;

namespace Plugin.SharedTransitions.Platforms.iOS.Renderers
{
    public class SharedTransitionPageRenderer : PageHandler
    {
        protected override void DisconnectHandler(ContentView nativeView)
        {
            if (Application.Current != null && Application.Current.MainPage is ISharedTransitionContainer shellPage)
            {
                shellPage.TransitionMap.RemoveFromPage((Page)VirtualView);
            }

            if (VirtualView.Parent is ISharedTransitionContainer navPage)
            {
                navPage.TransitionMap.RemoveFromPage((Page)VirtualView);
            }
            
            base.DisconnectHandler(nativeView);
        }
    }
}