// using System.ComponentModel;
// using Android.Content;
// using Android.OS;
// using AndroidX.Navigation;
// using AndroidX.Navigation.Fragment;
// using Microsoft.Maui.Platform;
// using Fragment = AndroidX.Fragment.App.Fragment;
// using FragmentManager = AndroidX.Fragment.App.FragmentManager;
// using Resource = SharedTransitions.Maui.Resource;
// using SupportTransitions = AndroidX.Transitions;
//
// namespace Plugin.SharedTransitions.Platforms.Android.Renderers;
//
// public class StackNavigationManagerExt : StackNavigationManager, ITransitionRenderer
// {
//     private readonly SharedTransitionNavigationRenderer _sharedTransitionNavigationRenderer;
//     
//     private NavigationHostCallbacksListener _callbacks;
//
//     public StackNavigationManagerExt(SharedTransitionNavigationRenderer sharedTransitionNavigationRenderer) : base(sharedTransitionNavigationRenderer.MauiContext!)
//     {
//         _sharedTransitionNavigationRenderer = sharedTransitionNavigationRenderer;
//     }
//
//     public override void Connect(IView navigationView)
//     {
//         base.Connect(navigationView);
//         _navigationTransition = new NavigationTransition(this);
//     }
//
//     public override void Disconnect()
//     {
//         _callbacks.Disconnect();
//         _callbacks = null;
//
//         base.Disconnect();
//     }
//
//     private NavHostFragment _navHostFragment;
//     public override void RequestNavigation(NavigationRequest e)
//     {
//         base.RequestNavigation(e);
//
//         if (_callbacks != null)
//             return;
//
//         _navHostFragment = GetType()
//             .BaseType!
//             .GetProperty("NavHost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
//             .GetValue(this) as NavHostFragment;
//
//         var defaultCallback = GetType()
//             .BaseType!
//             .GetField("_fragmentLifecycleCallbacks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
//             .GetValue(this);
//
//         //TODO: MB ELSE
//         SupportFragmentManager = _navHostFragment!.ChildFragmentManager;
//
//         _callbacks = new NavigationHostCallbacksListener(_navHostFragment!.NavController, _navHostFragment!.ChildFragmentManager, defaultCallback, OnFragmentCreated);
//     }
//     
//     void ApplyNavigationRequest(NavigationRequest args)
// 		{
// 			if (IsNavigating)
// 			{
// 				// This should really never fire for the developer. Our xplat code should be handling waiting for navigation to
// 				// complete before requesting another navigation from Core
// 				// Maybe some day we'll put a navigation queue into Core? For now we won't
// 				throw new InvalidOperationException("Previous Navigation Request is still Processing");
// 			}
//
// 			if (args.NavigationStack.Count == 0)
// 			{
// 				throw new InvalidOperationException("NavigationStack cannot be empty");
// 			}
//
// 			ActiveRequestedArgs = args;
// 			IReadOnlyList<IView> newPageStack = args.NavigationStack;
// 			bool animated = args.Animated;
// 			var navController = NavController;
// 			var previousNavigationStack = NavigationStack;
// 			var previousNavigationStackCount = previousNavigationStack.Count;
// 			bool initialNavigation = NavigationStack.Count == 0;
//
// 			// This updates the graphs public navigation stack property so it's outwardly correct
// 			// But we've saved off the previous stack so we can correctly interpret navigation
// 			UpdateNavigationStack(newPageStack);
//
// 			// This indicates that this is the first navigation request so we need to initialize the graph
// 			if (initialNavigation)
// 			{
// 				IsInitialNavigation = true;
// 				Initialize(args.NavigationStack);
// 				return;
// 			}
//
// 			// If the new stack isn't changing the visible page or the app bar then we just ignore
// 			// the changes because there's no point to applying these to the platform back stack
// 			// We only apply changes when the currently visible page changes and/or the appbar
// 			// will change (gain a back button)
// 			if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
// 			{
// 				NavigationFinished(NavigationView);
//
// 				// There's only one page on the stack then we trigger back button visibility logic
// 				// so that it can add a back button if it needs to
// 				if (previousNavigationStackCount == 1 || newPageStack.Count == 1)
// 					TriggerBackButtonVisibleUpdate();
//
// 				return;
// 			}
//
// 			// The incoming fragment uses these variables to pick the correct animation for the current
// 			// incoming navigation request
// 			if (newPageStack[newPageStack.Count - 1] == previousNavigationStack[previousNavigationStackCount - 1])
// 			{
// 				IsPopping = null;
// 			}
// 			else
// 			{
//
// 				IsPopping = newPageStack.Count < previousNavigationStackCount;
// 			}
//
// 			IsAnimated = animated;
//
// 			var fragmentNavDestinations = new List<FragmentNavigator.Destination>();
// 			navController.IterateBackStack(d => fragmentNavDestinations.Add(d));
//
// 			// Current BackStack has less entries then incoming new page stack
// 			// This will add Back Stack Entries until the back stack and the new stack 
// 			// match up
// 			if (fragmentNavDestinations.Count < newPageStack.Count)
// 			{
// 				for (int i = fragmentNavDestinations.Count; i < newPageStack.Count; i++)
// 				{
// 					var dest = AddFragmentDestination();
// 					navController.Navigate(dest.Id);
// 				}
// 			}
// 			// User just wants to replace the currently visible page but the number
// 			// of items in the stack are still the same. 
// 			// In theory we could just prompt the currently active fragment to swap out the new PageView
// 			// But this way we get an animation
// 			else if (newPageStack.Count == fragmentNavDestinations.Count)
// 			{
// 				int lastFragId = fragmentNavDestinations[newPageStack.Count - 1].Id;
// 				navController.PopBackStack();
// 				navController.Navigate(lastFragId);
// 			}
// 			// Our back stack has more entries on it then  
// 			else
// 			{
// 				int popToId = fragmentNavDestinations[newPageStack.Count - 1].Id;
// 				navController.PopBackStack(popToId, false);
// 			}
//
// 			// We only keep destinations around that are on the backstack
// 			// This iterates over the new backstack and removes any destinations
// 			// that are no longer apart of the back stack
//
// 			var iterateNewStack = NavController.Graph.Iterator();
// 			int startId = -1;
//
// 			navController.IterateBackStack(nvd =>
// 			{
// 				if (startId == -1)
// 					startId = nvd.Id;
// 				fragmentNavDestinations.Remove(nvd);
// 			});
//
// 			foreach (var activeDestinations in fragmentNavDestinations)
// 			{
// 				NavGraph.Remove(activeDestinations);
// 			}
//
// 			// If we end up removing the destination that was initially the StartDestination
// 			// The Navigation Graph can get really confused
// 			if (NavGraph.StartDestination != startId)
// 				NavGraph.StartDestination = startId;
//
// 			// The NavigationIcon on the toolbar gets set inside the Navigate call so this is the earliest
// 			// point in time that we can setup toolbar colors for the incoming page
// 			TriggerBackButtonVisibleUpdate();
// 		}
//
// 		// void TriggerBackButtonVisibleUpdate()
// 		// {
// 		// 	if (NavigationView != null)
// 		// 	{
// 		// 		ToolbarElement?.Toolbar?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
// 		// 	}
// 		// }
//
//     private void OnFragmentCreated(FragmentManager fm, Fragment f, Bundle bundle)
//     {
//         f.SharedElementEnterTransition = InflateTransitionInContext();
//     }
//
//     public override FragmentNavigator.Destination AddFragmentDestination()
//     {
//         return base.AddFragmentDestination();
//     }
//
//     protected override void OnDestinationChanged(NavController navController, NavDestination navDestination, Bundle bundle)
//     {
//         base.OnDestinationChanged(navController, navDestination, bundle);
//     }
//
//     protected override void OnNavigationViewFragmentDestroyed(FragmentManager fm, NavigationViewFragment navHostPageFragment)
//     {
//         base.OnNavigationViewFragmentDestroyed(fm, navHostPageFragment);
//     }
//
//     protected override void OnNavigationViewFragmentResumed(FragmentManager fm, NavigationViewFragment navHostPageFragment)
//     {
//         base.OnNavigationViewFragmentResumed(fm, navHostPageFragment);
//     }
//
//     public FragmentManager SupportFragmentManager { get; set; }
//     public string SelectedGroup { get; set; }
//     public BackgroundAnimation BackgroundAnimation { get; set; }
//     public ITransitionMapper TransitionMap { get; set; }
//     public bool IsInTabbedPage { get; set; }
//
//     /// <summary>
//     /// Track the page we need to get the custom properties for the shared transitions
//     /// </summary>
//     private Page _propertiesContainer;
//     public Page PropertiesContainer
//     {
//         get => _propertiesContainer;
//         set
//         {
//             if (_propertiesContainer == value)
//                 return;
//
//             //container has a different value from the one we are passing.
//             //We need to unsubscribe event, set the new value, then resubscribe for the new container
//             if (_propertiesContainer != null)
//                 _propertiesContainer.PropertyChanged -= PropertiesContainerOnPropertyChanged;
//
//             _propertiesContainer = value;
//
//             if (_propertiesContainer != null)
//             {
//                 _propertiesContainer.PropertyChanged += PropertiesContainerOnPropertyChanged;
//                 UpdateBackgroundTransition();
//                 UpdateTransitionDuration();
//                 UpdateSelectedGroup();
//             }
//         }
//     }
//     
//     public Page LastPageInStack { get; set; }
//
//     /// <summary>
//     /// Apply the custom transition in context
//     /// </summary>
//     /// <param name="context"></param>
//     public SupportTransitions.Transition InflateTransitionInContext()
//     {
//         return SupportTransitions.TransitionInflater.From(MauiContext!.Context!)!
//             .InflateTransition(Resource.Transition.navigation_transition)!
//             .SetDuration(_transitionDuration)
//             .AddListener(new NavigationTransitionListener(this));
//     }
//
//     private bool _isPush;
//     private bool _popToRoot;
//     private int _transitionDuration;
//     private NavigationTransition _navigationTransition;
//
// //         protected override void SetupPageTransition(FragmentTransaction transaction, bool isPush)
// //         {
// //             if (_popToRoot || Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
// //             {
// //                 base.SetupPageTransition(transaction, isPush);
// //             }
// //             else
// //             {
// //                 LastPageInStack = VirtualView.CurrentPage;
// //                 _navigationTransition.SetupPageTransition(transaction, isPush);
// //             }
// //         }
//
// //         public override void AddView(View child)
// //         {
// //             base.AddView(child);
// //
// //             if (!(child is Toolbar) && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
// //             {
// //                 if (((SharedTransitionNavigationPage)VirtualView).Parent is TabbedPage ||
// //                     ((SharedTransitionNavigationPage)VirtualView).Parent is FlyoutPage)
// //                 {
// //                     IsInTabbedPage = true;
// //                     var fragment = child.ParentFragment(SupportFragmentManager);
// //
// //                     if (fragment != null)
// //                         fragment.SharedElementEnterTransition = InflateTransitionInContext();
// //                 }
// //                 else
// //                 {
// //                     SupportFragmentManager.Fragments.Last().SharedElementEnterTransition = InflateTransitionInContext();
// //                 }
// //             }
// //         }
//
//          void PropertiesContainerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
//          {
//              if (e.PropertyName == SharedTransitionNavigationPage.BackgroundAnimationProperty.PropertyName)
//              {
//                  UpdateBackgroundTransition();
//              }
//              else if (e.PropertyName == SharedTransitionNavigationPage.TransitionDurationProperty.PropertyName)
//              {
//                  UpdateTransitionDuration();
//              }
//              else if (e.PropertyName == SharedTransitionNavigationPage.TransitionSelectedGroupProperty.PropertyName)
//              {
//                  UpdateSelectedGroup();
//              }
//          }
//
//          void RecreateFragment(Page page)
//          {
//              // if (page == null) return;
//              // //We need a bit of reflection to find the fragment associated to the page we need to display
//              // try
//              // {
//              //     var getPageFragment = typeof(NavigationPageRenderer).GetTypeInfo().GetDeclaredMethod("GetPageFragment");
//              //     if (getPageFragment != null)
//              //     {
//              //         var fragmentToDisplay = (Fragment)getPageFragment.Invoke(this, new object[] { page });
//              //         var transaction = SupportFragmentManager.BeginTransaction();
//              //         transaction.Detach(fragmentToDisplay);
//              //         transaction.Attach(fragmentToDisplay);
//              //         transaction.CommitAllowingStateLoss();
//              //     }
//              // }
//              // catch (Exception e)
//              // {
//              //     System.Diagnostics.Debug.WriteLine(e);
//              // }
//          }
//
//          void UpdateBackgroundTransition()
//          {
//              BackgroundAnimation = SharedTransitionNavigationPage.GetBackgroundAnimation(PropertiesContainer);
//          }
//
//          void UpdateTransitionDuration()
//          {
//              // TransitionDuration = (int)SharedTransitionNavigationPage.GetTransitionDuration(PropertiesContainer);
//          }
//
//          void UpdateSelectedGroup()
//          {
//              SelectedGroup = SharedTransitionNavigationPage.GetTransitionSelectedGroup(PropertiesContainer);
//          }
//          
//          public void SharedTransitionStarted()
//          {
//              ((ISharedTransitionContainer)_sharedTransitionNavigationRenderer.VirtualView).SendTransitionStarted(TransitionArgs());
//          }
//
//          public void SharedTransitionEnded()
//          {
//              ((ISharedTransitionContainer)_sharedTransitionNavigationRenderer.VirtualView).SendTransitionEnded(TransitionArgs());
//          }
//
//          public void SharedTransitionCancelled()
//          {
//              ((ISharedTransitionContainer)_sharedTransitionNavigationRenderer.VirtualView).SendTransitionCancelled(TransitionArgs());
//          }
// //
// //         void RecreateFragment(Page page)
// //         {
// //             if (page == null) return;
// //             //We need a bit of reflection to find the fragment associated to the page we need to display
// //             try
// //             {
// //                 var getPageFragment = typeof(NavigationPageRenderer).GetTypeInfo().GetDeclaredMethod("GetPageFragment");
// //                 if (getPageFragment != null)
// //                 {
// //                     var fragmentToDisplay = (Fragment)getPageFragment.Invoke(this, new object[] { page });
// //                     var transaction = SupportFragmentManager.BeginTransaction();
// //                     transaction.Detach(fragmentToDisplay);
// //                     transaction.Attach(fragmentToDisplay);
// //                     transaction.CommitAllowingStateLoss();
// //                 }
// //             }
// //             catch (Exception e)
// //             {
// //                 System.Diagnostics.Debug.WriteLine(e);
// //             }
// //         }
//
//     private SharedTransitionEventArgs TransitionArgs()
//     {
//         if (_isPush)
//         {
//             return new SharedTransitionEventArgs
//             {
//                 PageFrom = PropertiesContainer,
//                 PageTo = LastPageInStack,
//                 NavOperation = NavOperation.Push
//             };
//         }
//
//         return new SharedTransitionEventArgs
//         {
//             PageFrom = LastPageInStack,
//             PageTo = PropertiesContainer,
//             NavOperation = NavOperation.Pop
//         };
//     }
// }