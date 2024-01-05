using System.Reflection;
using Android.OS;
using Android.Views;
using Microsoft.Maui.Platform;
using View = Android.Views.View;
using AndroidX.Transitions;
using Debug = System.Diagnostics.Debug;
using Resource = SharedTransitions.Maui.Resource;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.New;

public class NavigationViewFragmentNew : NavigationViewFragment
{
    private StackNavigationManagerNew _stackNavigationManagerNew;

    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var view = (ContentViewGroup)base.OnCreateView(inflater, container, savedInstanceState);

        _stackNavigationManagerNew = GetType().BaseType!.GetProperty("NavigationManager", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(this) as StackNavigationManagerNew ??
                                     throw new NullReferenceException("NavigationManager is null");

        var transition = TransitionInflater.From(Context!)
            .InflateTransition(Resource.Transition.navigation_transition)!
            .SetDuration(300);

        SharedElementEnterTransition = transition;
        
        Debug.WriteLine($"Create page {_stackNavigationManagerNew.CurrentPage} {_stackNavigationManagerNew.CurrentPage.GetHashCode()}");

        return view;
    }

    public override void OnDestroyView()
    {
        Debug.WriteLine($"Destroy page {_stackNavigationManagerNew.CurrentPage} {_stackNavigationManagerNew.CurrentPage.GetHashCode()}");
        base.OnDestroyView();
    }
}