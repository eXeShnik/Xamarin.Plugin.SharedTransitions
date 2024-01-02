using System.Reflection;
using Android.App;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Fragment.App;
using AndroidX.Navigation;
using AndroidX.Navigation.Fragment;
using Java.Lang;
using Microsoft.Maui.Platform;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;
using AView = Android.Views.View;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.Copy;

public static class Extenstions
{
    public static bool IsDisposed(this Java.Lang.Object obj)
    {
        return obj.Handle == IntPtr.Zero;
    }

    public static bool IsDisposed(this global::Android.Runtime.IJavaObject obj)
    {
        return obj.Handle == IntPtr.Zero;
    }

    public static bool IsAlive(this Java.Lang.Object obj)
    {
        if (obj == null)
            return false;

        return!obj.IsDisposed();
    }

    public static bool IsAlive(this global::Android.Runtime.IJavaObject obj)
    {
        if (obj == null)
            return false;

        return!obj.IsDisposed();
    }

    internal static bool IsDestroyed(this Context? context)
    {
        if (context == null)
            return true;

        if (context.GetActivity() is FragmentActivity fa)
        {
            if (fa.IsDisposed())
                return true;

            var stateCheck = AndroidX.Lifecycle.Lifecycle.State.Destroyed;

            if (stateCheck != null &&
                fa.Lifecycle.CurrentState == stateCheck)
            {
                return true;
            }

            if (fa.IsDestroyed)
                return true;
        }

        return context.IsDisposed();
    }

    internal static bool IsPlatformContextDestroyed(this IElementHandler? handler)
    {
        var context = handler?.MauiContext?.Context;
        return context.IsDestroyed();
    }

    public static FragmentTransaction RemoveEx(this FragmentTransaction fragmentTransaction, Fragment fragment)
    {
        return fragmentTransaction.Remove(fragment);
    }

    public static FragmentTransaction AddEx(this FragmentTransaction fragmentTransaction, int containerViewId, Fragment fragment)
    {
        return fragmentTransaction.Add(containerViewId, fragment);
    }

    public static FragmentTransaction ReplaceEx(this FragmentTransaction fragmentTransaction, int containerViewId, Fragment fragment)
    {
        return fragmentTransaction.Replace(containerViewId, fragment);
    }

    public static FragmentTransaction HideEx(this FragmentTransaction fragmentTransaction, Fragment fragment)
    {
        return fragmentTransaction.Hide(fragment);
    }

    public static FragmentTransaction ShowEx(this FragmentTransaction fragmentTransaction, Fragment fragment)
    {
        return fragmentTransaction.Show(fragment);
    }

    public static FragmentTransaction SetTransitionEx(this FragmentTransaction fragmentTransaction, int transit)
    {
        return fragmentTransaction.SetTransition(transit);
    }

    public static FragmentTransaction SetReorderingAllowedEx(this FragmentTransaction fragmentTransaction, bool reorderingAllowed)
    {
        return fragmentTransaction.SetReorderingAllowed(reorderingAllowed);
    }

    public static int CommitAllowingStateLossEx(this FragmentTransaction fragmentTransaction)
    {
        return fragmentTransaction.CommitAllowingStateLoss();
    }

    public static bool ExecutePendingTransactionsEx(this FragmentManager fragmentManager)
    {
        return fragmentManager.ExecutePendingTransactions();
    }

    public static FragmentTransaction BeginTransactionEx(this FragmentManager fragmentManager)
    {
        return fragmentManager.BeginTransaction();
    }

    public static bool IsDestroyed(this FragmentManager? obj, Context? context)
    {
        if (obj == null || obj.IsDisposed())
            return true;

        if (context == null)
            return true;

        if (obj.IsDestroyed)
            return true;

        return context.IsDestroyed();
    }

    public static void IterateBackStack(this NavController navController, Action<FragmentNavigator.Destination> action)
    {
        var iterator = navController.Graph.Iterator();

        while (iterator.HasNext)
        {
            if (iterator.Next() is FragmentNavigator.Destination nvd)
            {
                try
                {
                    if (navController.GetBackStackEntry(nvd.Id).Destination is FragmentNavigator.Destination found)
                        action.Invoke(found);
                }
                catch (IllegalArgumentException) { }
            }
        }
    }

    public static NavigationRootManager GetNavigationRootManager(this IMauiContext mauiContext) =>
        mauiContext.Services.GetRequiredService<NavigationRootManager>();
    public static LayoutInflater GetLayoutInflater(this IMauiContext mauiContext)
    {
        var layoutInflater = mauiContext.Services.GetService<LayoutInflater>();

        if (!layoutInflater.IsAlive() && mauiContext.Context != null)
        {
            var activity = mauiContext.Context.GetActivity();

            if (activity != null)
                layoutInflater = LayoutInflater.From(activity);
        }

        return layoutInflater ?? throw new InvalidOperationException("LayoutInflater Not Found");
    }

    public static FragmentManager GetFragmentManager(this IMauiContext mauiContext)
    {
        var fragmentManager = mauiContext.Services.GetService<FragmentManager>();

        return fragmentManager
               ?? mauiContext.Context?.GetFragmentManager()
               ?? throw new InvalidOperationException("FragmentManager Not Found");
    }

    public static AppCompatActivity GetActivity(this IMauiContext mauiContext) =>
        (mauiContext.Context?.GetActivity() as AppCompatActivity)
        ?? throw new InvalidOperationException("AppCompatActivity Not Found");

    private static MethodInfo _addWeakSpecificMethod;
    private static MethodInfo _addSpecificMethod;

    private static void InitMauiContextMethods()
    {
        var type = typeof(MauiContext);

        if (_addWeakSpecificMethod == null)
            _addWeakSpecificMethod = type.GetMethod("AddWeakSpecific", BindingFlags.Instance | BindingFlags.NonPublic);

        if (_addSpecificMethod == null)
            _addSpecificMethod = type.GetMethod("AddSpecific", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    private static Dictionary<Type, MethodInfo> _addWeakSpecificMethods = new Dictionary<Type, MethodInfo>();
    private static Dictionary<Type, MethodInfo> _addSpecificMethods = new Dictionary<Type, MethodInfo>();

    private static void AddWeakSpecific<TService>(this MauiContext mauiContext, TService instance) where TService : class
    {
        InitMauiContextMethods();

        var type = typeof(TService);

        if (!_addWeakSpecificMethods.ContainsKey(type))
        {
            _addWeakSpecificMethods.Add(type, _addWeakSpecificMethod.MakeGenericMethod(typeof(TService)));
        }

        _addWeakSpecificMethods[type].Invoke(mauiContext, new object[] { instance });
    }

    private static void AddSpecific<TService>(this MauiContext mauiContext, TService instance) where TService : class
    {
        InitMauiContextMethods();

        var type = typeof(TService);

        if (!_addSpecificMethods.ContainsKey(type))
        {
            _addSpecificMethods.Add(type, _addSpecificMethod.MakeGenericMethod(typeof(TService)));
        }

        _addSpecificMethods[type].Invoke(mauiContext, new object[] { instance });
    }

    public static IMauiContext MakeScoped(
        this IMauiContext mauiContext,
        LayoutInflater layoutInflater = null,
        FragmentManager fragmentManager = null,
        Context context = null,
        bool registerNewNavigationRoot = false
    )
    {
        var scopedContext = new MauiContext(mauiContext.Services);

        if (layoutInflater != null)
        {
            scopedContext.AddWeakSpecific(layoutInflater);
        }

        if (fragmentManager != null)
            scopedContext.AddWeakSpecific(fragmentManager);

        if (context != null)
            scopedContext.AddWeakSpecific(context);

        if (registerNewNavigationRoot)
        {
            if (fragmentManager == null)
                throw new InvalidOperationException("If you're creating a new Navigation Root you need to use a new Fragment Manager");

            scopedContext.AddSpecific(new NavigationRootManager(scopedContext));
        }

        return scopedContext;
    }

    internal static AView ToPlatform(
        this IView view,
        IMauiContext fragmentMauiContext,
        Context context,
        LayoutInflater layoutInflater,
        FragmentManager childFragmentManager
    )
    {
        if (view.Handler?.MauiContext is MauiContext scopedMauiContext)
        {
            // If this handler belongs to a different activity then we need to 
            // recreate the view.
            // If it's the same activity we just update the layout inflater
            // and the fragment manager so that the platform view doesn't recreate
            // underneath the users feet
            if (scopedMauiContext.GetActivity() == context.GetActivity() &&
                view.Handler.PlatformView is AView platformView)
            {
                scopedMauiContext.AddWeakSpecific(layoutInflater);
                scopedMauiContext.AddWeakSpecific(childFragmentManager);
                return platformView;
            }
        }

        return view.ToPlatform(fragmentMauiContext.MakeScoped(layoutInflater: layoutInflater, fragmentManager: childFragmentManager));
    }

    internal static IServiceProvider GetApplicationServices(this IMauiContext mauiContext)
    {
        if (IPlatformApplication.Current?.Services is not null)
            return IPlatformApplication.Current.Services;

        throw new InvalidOperationException("Unable to find Application Services");
    }

    public static Activity GetPlatformWindow(this IMauiContext mauiContext) =>
        mauiContext.Services.GetRequiredService<Activity>();
}