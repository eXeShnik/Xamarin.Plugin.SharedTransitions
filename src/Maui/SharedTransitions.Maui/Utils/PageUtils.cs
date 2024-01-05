namespace Plugin.SharedTransitions.Shared.Utils;

public static class PageUtils
{
    public static void RemoveFromMap(this ITransitionMapper transitionMap, Page page)
    {
        transitionMap.RemoveFromPage(page);

        if (page is ITransitionAware)
        {
            MessagingCenter.Unsubscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs>(page, "SendTransitionStarted");
            MessagingCenter.Unsubscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs>(page, "SendTransitionEnded");
            MessagingCenter.Unsubscribe<SharedTransitionNavigationPage, SharedTransitionEventArgs>(page, "SendTransitionCancelled");
        }
    }
}