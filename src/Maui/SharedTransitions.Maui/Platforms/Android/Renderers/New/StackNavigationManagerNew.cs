using System.ComponentModel;
using System.Reflection;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using JetBrains.Annotations;
using Microsoft.Maui.Platform;
using Plugin.SharedTransitions.Shared.Utils;
using AView = Android.Views.View;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.New;

public class StackNavigationManagerNew : StackNavigationManager, ITransitionRenderer
{
    private static readonly NavigationRequest EmptyRequest = new NavigationRequest(new List<IView>(), true);

    private PropertyInfo _activeRequestedArgsPropertyInfo,
        _navHostPropertyInfo,
        _isInitialNavigationPropertyInfo,
        _isPoppingPropertyInfo,
        _isAnimatedPropertyInfo,
        _navGraphPropertyInfo,
        _fragmentNavigatorPropertyInfo;

    private MethodInfo _updateNavigationStackMethodInfo,
        _initializeMethodInfo,
        _triggerBackButtonVisibleUpdateMethodInfo,
        _navigationFinishedMethodInfo;
    
    private FieldInfo _fragmentLifecycleCallbacksFieldInfo;

    private IStackNavigationView _navigationView;
    private bool _isPush;

    public ITransitionMapper TransitionMap { get; set; }

    public StackNavigationManagerNew([NotNull]IMauiContext mauiContext) : base(mauiContext)
    {
        
    }

    public override void Connect(IView navigationView)
    {
        base.Connect(navigationView);
        _navigationView = (IStackNavigationView)navigationView;
        TransitionMap = ((ISharedTransitionContainer)_navigationView).TransitionMap;
    }

    public override void Disconnect()
    {
        base.Disconnect();
        _navigationView = null;
    }

    public override FragmentNavigator.Destination AddFragmentDestination()
    {
        var destination = new FragmentNavigator.Destination(_fragmentNavigatorPropertyInfo!.GetPropertyValue<FragmentNavigator>(this)!);
        var canonicalName = Java.Lang.Class.FromType(typeof(NavigationViewFragmentNew)).CanonicalName;

        if (canonicalName != null)
            destination.SetClassName(canonicalName);

        destination.Id = AView.GenerateViewId();
        _navGraphPropertyInfo.GetPropertyValue<NavGraph>(this).AddDestination(destination);
        return destination;
    }

    public override void RequestNavigation(NavigationRequest e)
    {
        CheckBaseClassReflectionInfo();

        try
        {
            base.RequestNavigation(EmptyRequest);
        }
        catch (Exception ex)
        {
            //ignore
        }

        try
        {
            ApplyNavigationRequest(e);

            PropertiesContainer = (Page)NavigationStack.Last();
            LastPageInStack = NavigationStack.Count > 1 ? (Page)NavigationStack[^2] : null;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
    
    private void CheckBaseClassReflectionInfo()
    {
        if (_fragmentNavigatorPropertyInfo != null)
            return;
        
        var type = GetType();
        _fragmentNavigatorPropertyInfo = type.BaseType!.GetProperty("FragmentNavigator", BindingFlags.Instance | BindingFlags.NonPublic);
        _navGraphPropertyInfo = type.BaseType!.GetProperty("NavGraph", BindingFlags.Instance | BindingFlags.NonPublic);
        _activeRequestedArgsPropertyInfo = type.BaseType!.GetProperty("ActiveRequestedArgs", BindingFlags.Instance | BindingFlags.NonPublic);
        _navHostPropertyInfo = type.BaseType!.GetProperty("NavHost", BindingFlags.Instance | BindingFlags.NonPublic);
        _isInitialNavigationPropertyInfo = type.BaseType!.GetProperty("IsInitialNavigation", BindingFlags.Instance | BindingFlags.NonPublic);
        _isPoppingPropertyInfo = type.BaseType!.GetProperty("IsPopping", BindingFlags.Instance | BindingFlags.NonPublic);
        _isAnimatedPropertyInfo = type.BaseType!.GetProperty("IsAnimated", BindingFlags.Instance | BindingFlags.NonPublic);
        _updateNavigationStackMethodInfo = type.BaseType!.GetMethod("UpdateNavigationStack", BindingFlags.Instance | BindingFlags.NonPublic);
        _initializeMethodInfo = type.BaseType!.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.NonPublic);
        _triggerBackButtonVisibleUpdateMethodInfo = type.BaseType!.GetMethod("TriggerBackButtonVisibleUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
        _navigationFinishedMethodInfo = type.BaseType!.GetMethod("NavigationFinished", BindingFlags.Instance | BindingFlags.NonPublic);
        _fragmentLifecycleCallbacksFieldInfo = type.BaseType!.GetField("_fragmentLifecycleCallbacks", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    void ApplyNavigationRequest(NavigationRequest args)
    {
        _isPush = false;
        _activeRequestedArgsPropertyInfo.SetValue(this, args);
        var newPageStack = args.NavigationStack;
        var animated = args.Animated;
        var navHost = _navHostPropertyInfo.GetPropertyValue<NavHostFragment>(this);
        var navGraph = _navGraphPropertyInfo.GetPropertyValue<NavGraph>(this);
        var navController = navHost.NavController;
        var previousNavigationStack = NavigationStack;
        var previousNavigationStackCount = previousNavigationStack.Count;
        var initialNavigation = NavigationStack.Count == 0;

        // This updates the graphs public navigation stack property so it's outwardly correct
        // But we've saved off the previous stack so we can correctly interpret navigation
        _updateNavigationStackMethodInfo.Invoke(this, new object[] { newPageStack });

        // This indicates that this is the first navigation request so we need to initialize the graph
        if (initialNavigation)
        {
            _isInitialNavigationPropertyInfo.SetValue(this, true);
            _initializeMethodInfo.Invoke(this, new object[] { args.NavigationStack });
            return;
        }

        // If the new stack isn't changing the visible page or the app bar then we just ignore
        // the changes because there's no point to applying these to the platform back stack
        // We only apply changes when the currently visible page changes and/or the appbar
        // will change (gain a back button)
        if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
        {
            _navigationFinishedMethodInfo.Invoke(this, new object[] { _navigationView });

            // There's only one page on the stack then we trigger back button visibility logic
            // so that it can add a back button if it needs to
            if (previousNavigationStackCount == 1 || newPageStack.Count == 1)
                _triggerBackButtonVisibleUpdateMethodInfo.Invoke(this, null);

            return;
        }

        // The incoming fragment uses these variables to pick the correct animation for the current
        // incoming navigation request
        if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
        {
            _isPoppingPropertyInfo.SetValue(this, null);
        }
        else
        {
            _isPoppingPropertyInfo.SetValue(this, newPageStack.Count < previousNavigationStackCount);
        }

        _isAnimatedPropertyInfo.SetValue(this, animated);

        var iterator = navHost.NavController.BackQueue.Iterator();
        var fragmentNavDestinations = new List<FragmentNavigator.Destination>();

        while (iterator.HasNext)
        {
            if (iterator.Next() is NavBackStackEntry nbse &&
                nbse.Destination is FragmentNavigator.Destination nvd)
            {
                fragmentNavDestinations.Add(nvd);
            }
        }

        FragmentNavigator.Extras.Builder extrasBuilder = null;
        // Current BackStack has less entries then incoming new page stack
        // This will add Back Stack Entries until the back stack and the new stack 
        // match up
        if (fragmentNavDestinations.Count < newPageStack.Count)
        {
            _isPush = true;
            var shouldAddTransition = newPageStack.Count > 1;
            for (int i = fragmentNavDestinations.Count; i < newPageStack.Count; i++)
            {
                var dest = AddFragmentDestination();

                if (shouldAddTransition && i == newPageStack.Count - 1)
                {
                    var currentPage = (Page)newPageStack[i];
                    
                    var previousPage = (Page)newPageStack[i - 1];
                    var prevMap = TransitionMap.GetMap(previousPage, null, true);

                    extrasBuilder = new FragmentNavigator.Extras.Builder()
                        .AddSharedElements(prevMap.ToDictionary(k => (AView)k.NativeView.Target, v => currentPage.Id + "_" + v.TransitionName));
                }

                navController.Navigate(dest.Id, null, null, extrasBuilder?.Build());
            }
        }
        // User just wants to replace the currently visible page but the number
        // of items in the stack are still the same. 
        // In theory we could just prompt the currently active fragment to swap out the new PageView
        // But this way we get an animation
        else if (newPageStack.Count == fragmentNavDestinations.Count)
        {
            _isPush = true;
            int lastFragId = fragmentNavDestinations[newPageStack.Count - 1].Id;
            navController.PopBackStack();
            navController.Navigate(lastFragId);
        }
        // Our back stack has more entries on it then  
        else
        {
            var currentPage = (Page)newPageStack[^1];
            var prevMap = TransitionMap.GetMap(PropertiesContainer, null, true);

            extrasBuilder = new FragmentNavigator.Extras.Builder()
                .AddSharedElements(prevMap.ToDictionary(k => (AView)k.NativeView.Target, v => currentPage.Id + "_" + v.TransitionName));
            
            int popToId = fragmentNavDestinations[newPageStack.Count - 1].Id;
            // navController.Navigate(popToId, null, null, extrasBuilder?.Build());
            // navController.PopBackStack(popToId, false);
            navController.NavigateUp();
            
            TransitionMap.RemoveFromMap(PropertiesContainer);
            
            // var lastIndex = fragmentNavDestinations.Count - 1;
            //
            // navController.Graph.Remove(fragmentNavDestinations[lastIndex]);
            // fragmentNavDestinations.RemoveAt(lastIndex);
        }

        // We only keep destinations around that are on the backstack
        // This iterates over the new backstack and removes any destinations
        // that are no longer apart of the back stack
        var iterateNewStack = navHost.NavController.BackQueue.Iterator();
        int startId = -1;
        while (iterateNewStack.HasNext)
        {
            if (iterateNewStack.Next() is NavBackStackEntry nbse &&
                nbse.Destination is FragmentNavigator.Destination nvd)
            {
                fragmentNavDestinations.Remove(nvd);

                if (startId == -1)
                    startId = nvd.Id;
            }
        }

        foreach (var activeDestinations in fragmentNavDestinations)
        {
            navGraph.Remove(activeDestinations);
        }

        // If we end up removing the destination that was initially the StartDestination
        // The Navigation Graph can get really confused
        if (navGraph.StartDestination != startId)
            navGraph.StartDestination = startId;

        // The NavigationIcon on the toolbar gets set inside the Navigate call so this is the earliest
        // point in time that we can setup toolbar colors for the incoming page
        _triggerBackButtonVisibleUpdateMethodInfo.Invoke(this, null);
    }

    public FragmentManager SupportFragmentManager { get; set; }
    public string SelectedGroup { get; set; }
    public bool IsInTabbedPage { get; set; }
    public BackgroundAnimation BackgroundAnimation { get; set; }

    private Page _propertiesContainer;
    public Page PropertiesContainer
    {
        get => _propertiesContainer;
        set
        {
            if (_propertiesContainer == value)
                return;

            //container has a different value from the one we are passing.
            //We need to unsubscribe event, set the new value, then resubscribe for the new container
            if (_propertiesContainer != null)
                _propertiesContainer.PropertyChanged -= PropertiesContainerOnPropertyChanged;

            _propertiesContainer = value;

            if (_propertiesContainer != null)
            {
                _propertiesContainer.PropertyChanged += PropertiesContainerOnPropertyChanged;
                UpdateBackgroundTransition();
                UpdateTransitionDuration();
                UpdateSelectedGroup();
            }
        }
    }
    public Page LastPageInStack { get; set; }

    public AndroidX.Transitions.Transition InflateTransitionInContext()
    {
        throw new NotImplementedException();
    }
    
    public void SharedTransitionStarted()
    {
        ((ISharedTransitionContainer)_navigationView).SendTransitionStarted(TransitionArgs());
    }

    public void SharedTransitionEnded()
    {
        ((ISharedTransitionContainer)_navigationView).SendTransitionEnded(TransitionArgs());
    }

    public void SharedTransitionCancelled()
    {
        ((ISharedTransitionContainer)_navigationView).SendTransitionCancelled(TransitionArgs());
    }

    void UpdateBackgroundTransition()
    {
        BackgroundAnimation = SharedTransitionNavigationPage.GetBackgroundAnimation(PropertiesContainer);
    }

    void UpdateTransitionDuration()
    {
        
    }

    void UpdateSelectedGroup()
    {
        SelectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
    }
    
    void PropertiesContainerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == SharedTransitionNavigationPage.BackgroundAnimationProperty.PropertyName)
        {
            UpdateBackgroundTransition();
        }
        else if (e.PropertyName == SharedTransitionNavigationPage.TransitionDurationProperty.PropertyName)
        {
            UpdateTransitionDuration();
        }
        else if (e.PropertyName == SharedTransitionNavigationPage.TransitionSelectedGroupProperty.PropertyName)
        {
            UpdateSelectedGroup();
        }
    }

    SharedTransitionEventArgs TransitionArgs()
    {
        if (_isPush)
        {
            return new SharedTransitionEventArgs
            {
                PageFrom = PropertiesContainer,
                PageTo = LastPageInStack,
                NavOperation = NavOperation.Push
            };
        }

        return new SharedTransitionEventArgs
        {
            PageFrom = LastPageInStack,
            PageTo = PropertiesContainer,
            NavOperation = NavOperation.Pop
        };
    }
}