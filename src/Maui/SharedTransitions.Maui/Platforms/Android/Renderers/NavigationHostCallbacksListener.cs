using Android.Content;
using Android.OS;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using View = Android.Views.View;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers;

public class NavigationHostCallbacksListener :
    FragmentManager.FragmentLifecycleCallbacks,
    NavController.IOnDestinationChangedListener
{
    private readonly NavController _navController;
    private readonly FragmentManager _childFragmentManager;
    private readonly Action<FragmentManager, Fragment, Bundle> _onFragmentCreated;

    // private readonly FragmentManager.FragmentLifecycleCallbacks _defaultFragmentLifecycleCallbacks;
    // private readonly NavController.IOnDestinationChangedListener _defaultOnDestinationChangedListener;

    public NavigationHostCallbacksListener(
        NavController navController,
        FragmentManager childFragmentManager,
        object defaultCallback,
        Action<FragmentManager, Fragment, Bundle> onFragmentCreated = null
    )
    {
        _navController = navController;
        _childFragmentManager = childFragmentManager;
        _onFragmentCreated = onFragmentCreated;

        // _defaultFragmentLifecycleCallbacks = defaultCallback as FragmentManager.FragmentLifecycleCallbacks;
        // _defaultOnDestinationChangedListener = defaultCallback as NavController.IOnDestinationChangedListener;

        _navController.AddOnDestinationChangedListener(this);
        _childFragmentManager.RegisterFragmentLifecycleCallbacks(this, false);
    }

    #region IOnDestinationChangedListener

    void NavController.IOnDestinationChangedListener.OnDestinationChanged(NavController p0, NavDestination p1, Bundle p2)
    {
        // _defaultOnDestinationChangedListener.OnDestinationChanged(p0, p1, p2);
    }

    #endregion

    #region FragmentLifecycleCallbacks

    public override void OnFragmentResumed(FragmentManager fm, Fragment f)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentResumed(fm, f);
    }

    public override void OnFragmentViewDestroyed(FragmentManager fm, Fragment f)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentViewDestroyed(fm, f);
    }

    public override void OnFragmentCreated(FragmentManager fm, Fragment f, Bundle savedInstanceState)
    {
        _onFragmentCreated?.Invoke(fm, f, savedInstanceState);
        // _defaultFragmentLifecycleCallbacks.OnFragmentCreated(fm, f, savedInstanceState);
    }

    public override void OnFragmentPreCreated(FragmentManager fm, Fragment f, Bundle savedInstanceState)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentPreCreated(fm, f, savedInstanceState);
    }

    public override void OnFragmentPreAttached(FragmentManager fm, Fragment f, Context context)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentPreAttached(fm, f, context);
    }

    public override void OnFragmentStarted(FragmentManager fm, Fragment f)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentStarted(fm, f);
    }

    public override void OnFragmentAttached(FragmentManager fm, Fragment f, Context context)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentAttached(fm, f, context);
    }

    public override void OnFragmentSaveInstanceState(FragmentManager fm, Fragment f, Bundle outState)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentPreCreated(fm, f, outState);
    }

    public override void OnFragmentViewCreated(FragmentManager fm, Fragment f, View v, Bundle savedInstanceState)
    {
        // _defaultFragmentLifecycleCallbacks.OnFragmentViewCreated(fm, f, v, savedInstanceState);
    }

    #endregion

    internal void Disconnect()
    {

        if (_navController != null && _navController.Handle != IntPtr.Zero)
            _navController.RemoveOnDestinationChangedListener(this);

        _childFragmentManager?.UnregisterFragmentLifecycleCallbacks(this);
    }
}