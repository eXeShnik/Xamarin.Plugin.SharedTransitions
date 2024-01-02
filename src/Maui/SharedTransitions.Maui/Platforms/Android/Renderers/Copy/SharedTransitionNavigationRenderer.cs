using Android.Runtime;
using Android.Views;
using AndroidX.Fragment.App;
using AndroidX.Navigation.Fragment;
using Microsoft.Maui.Handlers;
using PlatformView = Android.Views.View;
using Resource = SharedTransitions.Maui.Resource;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers.Copy;

public partial class SharedTransitionNavigationRenderer : INavigationViewHandler
{
    public static IPropertyMapper<IStackNavigationView, INavigationViewHandler> Mapper = new PropertyMapper<IStackNavigationView, INavigationViewHandler>(ViewMapper)
    {
    };

    public static CommandMapper<IStackNavigationView, INavigationViewHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
    };

    public SharedTransitionNavigationRenderer() : base(Mapper, CommandMapper)
    {
    }

    public SharedTransitionNavigationRenderer(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public SharedTransitionNavigationRenderer(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IStackNavigationView INavigationViewHandler.VirtualView => VirtualView;

    PlatformView INavigationViewHandler.PlatformView => PlatformView;
}

public partial class SharedTransitionNavigationRenderer : ViewHandler<IStackNavigationView, PlatformView>
{
    private StackNavigationManagerExt _stackNavigationManager;
    internal StackNavigationManagerExt StackNavigationManager => _stackNavigationManager;

    protected override PlatformView CreatePlatformView()
    {
        LayoutInflater? li = CreateNavigationManager().MauiContext?.GetLayoutInflater();
        _ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

        var view = li.Inflate(Resource.Layout.fragment_backstack, null).JavaCast<FragmentContainerView>();
        _ = view ?? throw new InvalidOperationException($"Resource.Layout.navigationlayout view not found");
        return view;
        // var li = LayoutInflater.From(CreateNavigationManager().MauiContext?.Context);
        // _ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");
        //
        // var @params = new CoordinatorLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
        // {
        //     Behavior = new AppBarLayout.ScrollingViewBehavior()
        // };
        //
        // var view = new FragmentContainerView(MauiContext!.Context!)
        // {
        //     Id = Resource.Id.nav_host,
        //     LayoutParameters = @params,
        // };
        //
        // var fragment = new MauiNavHostFragmentExt
        // {
        //     StackNavigationManager = CreateNavigationManager(),
        // };
        //
        // return view;
    }

    private StackNavigationManagerExt CreateNavigationManager()
    {
        _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

        return _stackNavigationManager ??= new StackNavigationManagerExt(MauiContext);
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        _stackNavigationManager?.Connect(VirtualView);

        platformView.ViewAttachedToWindow += OnViewAttachedToWindow;
        platformView.ViewDetachedFromWindow += OnViewDetachedFromWindow;
    }

    private void OnViewDetachedFromWindow(object sender, PlatformView.ViewDetachedFromWindowEventArgs e)
    {
        PlatformView.LayoutChange -= OnLayoutChanged;
    }

    private void OnViewAttachedToWindow(object sender, PlatformView.ViewAttachedToWindowEventArgs e)
    {
        PlatformView.LayoutChange += OnLayoutChanged;
    }

    void OnLayoutChanged(object sender, PlatformView.LayoutChangeEventArgs e) =>
        VirtualView.Arrange(new Rect(0, 0, e.Right - e.Left, e.Bottom - e.Top));

    void RequestNavigation(NavigationRequest ea)
    {
        _stackNavigationManager?.RequestNavigation(ea);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.ViewAttachedToWindow -= OnViewAttachedToWindow;
        platformView.ViewDetachedFromWindow -= OnViewDetachedFromWindow;
        platformView.LayoutChange -= OnLayoutChanged;

        _stackNavigationManager?.Disconnect();
        base.DisconnectHandler(platformView);
    }

    public static void RequestNavigation(INavigationViewHandler arg1, IStackNavigation arg2, object arg3)
    {
        if (arg1 is SharedTransitionNavigationRenderer platformHandler && arg3 is NavigationRequest ea)
            platformHandler.RequestNavigation(ea);
    }
}

[Register("sharedTransitions.Maui.MauiNavHostFragmentExt")]
class MauiNavHostFragmentExt : NavHostFragment
{
    public StackNavigationManagerExt StackNavigationManager { get; set; }

    public MauiNavHostFragmentExt()
    {
    }

    protected MauiNavHostFragmentExt(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }
}