using System.Reflection;
using Android.OS;
using Android.Views;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Platform;
using Debug = System.Diagnostics.Debug;
using View = Android.Views.View;
using AndroidX.Transitions;
using Resource = SharedTransitions.Maui.Resource;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.New;

public class NavigationViewFragmentNew : NavigationViewFragment
{
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        var view = (ContentViewGroup) base.OnCreateView(inflater, container, savedInstanceState);

        var transition = TransitionInflater.From(Context!)
            .InflateTransition(Resource.Transition.navigation_transition)!
            .SetDuration(300);
        
        SharedElementEnterTransition = transition;
        SharedElementReturnTransition = transition;
        
        return view;
    }
}