namespace Plugin.SharedTransitions;

public static class PageExtensions
{
    /// <summary>
    /// Find the current page based on the MainPage type
    /// </summary>
    /// <param name="mainPage">Application current mainpage</param>
    internal static Page GetCurrentPage(this Page mainPage)
    {
        mainPage = mainPage.GetCurrentPageFromModal();

        switch (mainPage)
        {
            case Shell appShell:
                return appShell.GetCurrentShellPage();
            case SharedTransitionNavigationPage navPage:
                return navPage.Navigation?.NavigationStack?.Last()?.GetCurrentPageFromModal();
            case FlyoutPage flyoutPage:
                return flyoutPage.Detail.GetCurrentPage();
            case TabbedPage tabbedPage:
                return tabbedPage.CurrentPage.GetCurrentPage();
            default:
                return null;
        }
    }

    /// <summary>
    /// Find the current page from the main shell
    /// </summary>
    internal static Page GetCurrentShellPage(this Shell shell)
    {
        var currentSection = shell?.CurrentItem?.CurrentItem?.CurrentItem;

        if (currentSection != null && currentSection is NavigableElement navigableElement)
        {
            var lastElement = navigableElement.Navigation.NavigationStack?.Last();

            if (lastElement != null)
                return lastElement.GetCurrentPageFromModal();

            if (currentSection is IShellContentController shellContentController)
                return shellContentController.Page?.GetCurrentPageFromModal();
        }

        return null;
    }

    internal static Page GetCurrentPageFromModal(this Page page)
    {
        return page.Navigation?.ModalStack?.Count > 0
            ? page.Navigation.ModalStack.Last().GetCurrentPage()
            : page;
    }
}