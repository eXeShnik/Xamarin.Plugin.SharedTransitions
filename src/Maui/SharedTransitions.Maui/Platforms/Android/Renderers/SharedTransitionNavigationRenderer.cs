// using Microsoft.Maui.Controls.Internals;
// using Microsoft.Maui.Handlers;
// using View = Android.Views.View;
//
// namespace Plugin.SharedTransitions.Platforms.Android.Renderers;
// /*
//  * IMPORTANT NOTES:
//  * Read the dedicate comments in code for more info about those fixes.
//  *
//  * Pop a controller with transitions groups:
//  * Fix to allow the group to be set with binding
//  *
//  */
//
// /// <summary>
// /// Platform Renderer for the NavigationPage responsible to manage the Shared Transitions
// /// </summary>
// [Preserve(AllMembers = true)]
// public class SharedTransitionNavigationRenderer : NavigationViewHandler
// {
//     protected override View CreatePlatformView()
//     {
//         _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
//
//         var field = typeof(NavigationViewHandler).GetField("_stackNavigationManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//         field?.SetValue(this, new StackNavigationManagerExt(this));
//
//         return base.CreatePlatformView();
//     }
// }