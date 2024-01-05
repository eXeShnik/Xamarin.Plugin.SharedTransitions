using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.Fragment.App;
using Microsoft.Maui.Platform;
using Animation = Android.Views.Animations.Animation;
using AView = Android.Views.View;
using Resource = SharedTransitions.Maui.Resource;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.Copy;

public class NavigationViewFragmentExt : Fragment
{
    AView _currentView;
    FragmentContainerView _fragmentContainerView;
    StackNavigationManagerExt _navigationManager;

    FragmentContainerView FragmentContainerView =>
        _fragmentContainerView ?? throw new InvalidOperationException($"FragmentContainerView cannot be null here");

    MauiNavHostFragment NavHostFragment =>
        this.ParentFragment as MauiNavHostFragment;

    internal StackNavigationManagerExt NavigationManager
    {
        get => _navigationManager ?? NavHostFragment?.StackNavigationManager ?? throw new InvalidOperationException($"NavigationManager cannot be null here");
        set => _navigationManager = value;
    }

    protected NavigationViewFragmentExt(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }

    public NavigationViewFragmentExt()
    {
    }

    public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        _fragmentContainerView ??= container as FragmentContainerView;

        // When shuffling around the back stack sometimes we'll need a page to detach and then reattach.
        // This mainly happens when users are removing or inserting pages. If we only have one page
        // and the user inserts a page at index zero we push a page onto the native backstack so
        // that we can get a toolbar with a back button. 

        // Internally Android destroys the first fragment and then creates a second fragment. 
        // Android removes the view associated with the first fragment and then adds the view 
        // now associated with the second fragment. In our case this is the same view.

        // This is all a bit silly because the page is just getting added and removed to the same
        // view. Unfortunately FragmentContainerView is sealed so we can't inherit from it and influence
        // when the views are being added and removed. If this ends up causing too much shake up
        // Then we can try some other approaches like just modifying the navbar ourselves to include a back button
        // Even if there's only one page on the stack

        _currentView =
            NavigationManager
                .CurrentPage
                .ToPlatform(NavigationManager.MauiContext, RequireContext(), inflater, ChildFragmentManager);

        _currentView.RemoveFromParent();

        return _currentView;
    }

    public override void OnResume()
    {
        if (_currentView == null || NavigationManager.NavHost == null)
            return;

        if (_currentView.Parent == null)
        {
            // Re-add the view to the container if Android removed it
            // see comment inside OnCreateView for more information
            FragmentContainerView.AddView(_currentView);
        }

        base.OnResume();
    }

    public override void OnDestroy()
    {
        _currentView = null;
        _fragmentContainerView = null;

        base.OnDestroy();
    }

    public override Animation OnCreateAnimation(int transit, bool enter, int nextAnim)
    {
        var id = 0;
    
        Animation returnValue;
    
        // This means the operation currently being processed shouldn't be animated
        // This will happen if a user inserts or removes a root page
        if (NavigationManager.IsPopping == null || !NavigationManager.IsAnimated)
        {
            returnValue = null;
        }
        else
        {
            // Once we have Function Mappers figured out all of this code can
            // move to a function mapper as a way to customize animations from code
            if (NavigationManager.IsPopping.Value)
            {
                if (!enter)
                {
                    id = Resource.Animation.nav_default_pop_exit_anim;
                }
                else
                {
                    id = Resource.Animation.nav_default_pop_enter_anim;
                }
            }
            else
            {
                if (enter)
                {
                    id = Resource.Animation.nav_default_enter_anim;
                }
                else
                {
                    id = Resource.Animation.nav_default_exit_anim;
                }
            }
    
            if (id > 0)
            {
                returnValue = AnimationUtils.LoadAnimation(Context, id);
            }
            else
            {
                returnValue = base.OnCreateAnimation(transit, enter, id);
            }
        }
    
        return returnValue!;
    }
}