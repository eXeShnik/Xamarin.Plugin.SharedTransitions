#if ANDROID
using Android.OS;
using AndroidX.Core.View;
#endif
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Debug = System.Diagnostics.Debug;

namespace Plugin.SharedTransitions;

/// <summary>
/// Add specific information to View to activate the Shared Transitions
/// </summary>
public static class Transition
{
    /// <summary>
    /// Transition name used to associate views between pages
    /// </summary>
    public static readonly BindableProperty NameProperty = BindableProperty.CreateAttached(
        "Name",
        typeof(string),
        typeof(BindableObject),
        null,
        propertyChanged: OnPropertyChanged
    );

    /// <summary>
    /// Transition group for dynamic transitions
    /// </summary>
    public static readonly BindableProperty GroupProperty = BindableProperty.CreateAttached(
        "Group",
        typeof(string),
        typeof(BindableObject),
        null,
        propertyChanged: OnPropertyChanged
    );

    public static readonly BindableProperty LightSnapshotProperty = BindableProperty.CreateAttached(
        "LightSnapshot",
        typeof(bool),
        typeof(SharedTransitionNavigationPage),
        false,
        propertyChanged: OnPropertyChanged
    );


    /// <summary>
    /// Gets the shared transition name for the element
    /// </summary>
    /// <param name="bindable">Xamarin Forms Element</param>
    public static string GetName(BindableObject bindable)
    {
        return(string)bindable.GetValue(NameProperty);
    }

    /// <summary>
    /// Gets the shared transition group for the element
    /// </summary>
    /// <param name="bindable">Xamarin Forms Element</param>
    public static string GetGroup(BindableObject bindable)
    {
        return(string)bindable.GetValue(GroupProperty);
    }

    /// <summary>
    /// Performs a "Light" snapshot on iOS, rasterizing a copy of the current view to use during the transition.
    /// This method is faster but doesnt play well with transformations (size, borders, ecc...).
    /// Use it only when animating the element position!
    /// Does nothing on Anddroid
    /// </summary>
    /// <param name="bindable">Xamarin Forms Element</param>
    public static bool GetLightSnapshot(BindableObject bindable)
    {
        return(bool)bindable.GetValue(LightSnapshotProperty);
    }

    /// <summary>
    /// Sets the shared transition name for the element
    /// </summary>
    /// <param name="bindable">Xamarin Forms Element</param>
    /// <param name="value">The shared transition name</param>
    public static void SetName(BindableObject bindable, string value)
    {
        bindable.SetValue(NameProperty, value);
    }

    /// <summary>
    /// Sets the shared transition group for the element
    /// </summary>
    /// <param name="bindable">Xamarin Forms Element</param>
    /// <param name="value">The shared transition group</param>
    public static void SetGroup(BindableObject bindable, string value)
    {
        bindable.SetValue(GroupProperty, value);
    }

    /// <summary>
    /// Performs a "Light" snapshot on iOS, rasterizing a copy of the current view to use during the transition.
    /// This method is faster but doesnt play well with transformations (size, borders, ecc...).
    /// Use it only when animating the element position!
    /// Does nothing on Anddroid
    /// </summary>
    /// <param name="bindable">Xamarin Forms Element</param>
    /// <param name="value">The shared transition name</param>
    public static void SetLightSnapshot(BindableObject bindable, bool value)
    {
        bindable.SetValue(LightSnapshotProperty, value);
    }


    /// <summary>
    /// Registers the transition element in the TransitionStack
    /// </summary>
    /// <param name="view">Xamarin Forms View</param>
    /// <param name="nativeView">The platform native View</param>
    /// <param name="currentPage">The current page where the transition has been added</param>
    /// <returns>The unique Id of the native View</returns>
    public static void RegisterTransition(View view, object nativeView, Page currentPage)
    {
        var transitionName = GetName(view);
        var transitionGroup = GetGroup(view);

        if (!string.IsNullOrEmpty(transitionName) || !string.IsNullOrEmpty(transitionGroup))
        {
            var lightTransition = GetLightSnapshot(view);

            if (Application.Current.MainPage is ISharedTransitionContainer shellPage)
            {
                shellPage.TransitionMap.AddOrUpdate(currentPage, transitionName, transitionGroup, lightTransition, view, nativeView);
            }
            else if (currentPage.Parent is ISharedTransitionContainer navPage)
            {
                navPage.TransitionMap.AddOrUpdate(currentPage, transitionName, transitionGroup, lightTransition, view, nativeView);
            }
            else
            {
                throw new InvalidOperationException("Shared transitions effect can be attached only to element in a ISharedTransitionContainer");
            }
        }
    }

    /// <summary>
    /// Remove the transition configuration from the Stack
    /// </summary>
    /// <param name="view">View associated to the transition</param>
    /// <param name="currentPage">Container Page</param>
    public static void RemoveTransition(View view, Page currentPage)
    {
        switch (currentPage.Parent)
        {
            case SharedTransitionNavigationPage sharedTransitionNavigationPage:
                sharedTransitionNavigationPage.TransitionMap.Remove(currentPage, view);
                break;
            case SharedTransitionShell sharedTransitionshell:
                sharedTransitionshell.TransitionMap.Remove(currentPage, view);
                break;
        }
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable == null)
        {
            Debug.WriteLine("BindableObject should not be null at this point (Attached Property changed)");
            return;
        }

        Debug.WriteLine($"===== SHARED: update transition property for {bindable}: {oldValue} - {newValue}");

        var element = (View)bindable;
        var existing = element.Effects.FirstOrDefault(x => x is TransitionEffect);

        if (existing == null && newValue != null && newValue.ToString() != "")
        {
            element.Effects.Add(new TransitionEffect());
        }
        else if (existing != null && GetName(bindable) == null && GetGroup(bindable) == null)
        {
            element.Effects.Remove(new TransitionEffect());
        }
    }
}

/// <summary>
/// Transition effect used to specify tags to views
/// </summary>
/// <seealso cref="Xamarin.Forms.RoutingEffect" />
public class TransitionEffect : RoutingEffect
{

}

#if IOS
public class PlatformTransitionEffect : PlatformEffect
{
    private Page _currentPage;
    private Element _currentElement;

    protected override void OnAttached()
    {
        if (Application.Current!.MainPage is Shell appShell)
        {
            _currentPage = appShell.GetCurrentShellPage();
            UpdateTag();
        }
        else
        {
            FindContainerPageAndUpdateTag(Element);
        }
    }

    private void FindContainerPageAndUpdateTag(Element element)
    {
        _currentElement = element;
        var parent = _currentElement.Parent;
        if (parent != null && parent is Page page)
        {
            _currentPage = page;
            UpdateTag();
        }
        else if (parent != null)
        {
            FindContainerPageAndUpdateTag(parent);
        }
        else if (_currentPage == null)
        {
            _currentElement.PropertyChanged += CurrentElementOnPropertyChanged;
        }
    }

    protected void CurrentElementOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "Parent")
        {
            _currentElement.PropertyChanged -= CurrentElementOnPropertyChanged;
            FindContainerPageAndUpdateTag(_currentElement);
        }
    }

    protected override void OnDetached()
    {
        if (Element is View element)
            Transition.RemoveTransition(element, _currentPage);
    }

    protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
    {
        //Always check the LightSnapshopProperty in case it gets changed later by a binding
        if (args.PropertyName == Transition.NameProperty.PropertyName && Transition.GetName(Element) != null ||
            args.PropertyName == Transition.GroupProperty.PropertyName && Transition.GetGroup(Element) != null ||
            args.PropertyName == Transition.LightSnapshotProperty.PropertyName && Transition.GetLightSnapshot(Element) != null)
        {
            UpdateTag();
        }

        base.OnElementPropertyChanged(args);
    }

    /// <summary>
    /// Update the shared transition name and/or group
    /// </summary>
    void UpdateTag()
    {
        if (Element is View element && _currentPage != null)
        {
            if (Control != null)
            {
                Transition.RegisterTransition(element, Control, _currentPage);
            }
            else if (Container != null)
            {
                Transition.RegisterTransition(element, Container, _currentPage);
            }
        }
    }
}
#elif ANDROID
public class PlatformTransitionEffect : PlatformEffect
{
    private Page _currentPage;
    private Element _currentElement;
    protected override void OnAttached()
    {
        if (Application.Current?.MainPage is Shell appShell)
        {
            _currentPage = appShell.GetCurrentShellPage();
            UpdateTag();
        }
        else
        {
            FindContainerPageAndUpdateTag(Element);
        }
    }

    private void FindContainerPageAndUpdateTag(Element element)
    {
        _currentElement = element;
        var parent = _currentElement.Parent;
        if (parent != null && parent is Page page)
        {
            _currentPage = page;
            UpdateTag();
        }
        else if (parent != null)
        {
            FindContainerPageAndUpdateTag(parent);
        }
        else if (_currentPage == null)
        {
            _currentElement.PropertyChanged += CurrentElementOnPropertyChanged;
        }
    }

    protected void CurrentElementOnPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "Parent")
        {
            _currentElement.PropertyChanged -= CurrentElementOnPropertyChanged;
            FindContainerPageAndUpdateTag(_currentElement);
        }
    }

    protected override void OnDetached()
    {
        if (Element is View element)
            Transition.RemoveTransition(element, _currentPage);
    }

    protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
    {
        if (args.PropertyName == Transition.NameProperty.PropertyName ||
            args.PropertyName == Transition.GroupProperty.PropertyName)
            UpdateTag();

        base.OnElementPropertyChanged(args);
    }

    /// <summary>
    /// Update the shared transition name and/or group
    /// </summary>
    private void UpdateTag()
    {
        if (Element is View element && Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop && _currentPage != null)
        {
            var transitionName = Transition.GetName(element);
            var transitionGroup = Transition.GetGroup(element);

            //TODO: Rethink this mess... it works but is superduper ugly 
            if (transitionGroup != null)
                transitionName += "_" + transitionGroup;

            if (Container != null)
            {
                Transition.RegisterTransition(element, Container, _currentPage);
                ViewCompat.SetTransitionName(Container, _currentPage.Id + "_" + transitionName);
                // Container.TransitionName = _currentPage.Id + "_" + transitionName;
                // Container.TransitionName = transitionName;
            }
            else if (Control != null)
            {
                //TransitionName needs to be unique for page to enable transitions between more than 2 pages
                Transition.RegisterTransition(element, Control, _currentPage);
                ViewCompat.SetTransitionName(Control, _currentPage.Id + "_" + transitionName);
                // Control.TransitionName = _currentPage.Id + "_" + transitionName;
                // Control.TransitionName = transitionName;
            }
        }
    }
}
#endif