﻿using System;
using System.Linq;
using Xamarin.Forms;

namespace Plugin.SharedTransitions
{
    /// <summary>
    /// Transition effect used to specify tags to views
    /// </summary>
    /// <seealso cref="Xamarin.Forms.RoutingEffect" />
    public class TransitionEffect : RoutingEffect
    {
        public TransitionEffect() : base(Transition.FullName)
        {
        }
    }

    /// <summary>
    /// Add specific information to View to activate the Shared Transitions
    /// </summary>
    public static class Transition
    {
        public const string ResolutionGroupName = "Plugin.SharedTransitions";
        public const string EffectName = nameof(Transition);
        public const string FullName = ResolutionGroupName + "." + EffectName;

        /// <summary>
        /// Transition name to associate views animation between pages
        /// </summary>
        public static readonly BindableProperty TransitionNameProperty = BindableProperty.CreateAttached(
            "TransitionName", 
            typeof(string), 
            typeof(Transition), 
            0, 
            propertyChanged: 
            OnPropertyChanged);

        /// <summary>
        /// Gets the shared transition name for the element
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        public static string GetTransitionName(BindableObject bindable)
        {
            return (string)bindable.GetValue(TransitionNameProperty);
        }

        /// <summary>
        /// Sets the shared transition name for the element
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="value">The shared transition name.</param>
        public static void SetTransitionName(BindableObject bindable, int value)
        {
            bindable.SetValue(TransitionNameProperty, value);
        }

        /// <summary>
        /// Registers the transition element in the TransitionStack
        /// when the native View does not already have a unique Id
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <returns>The unique Id of the native View</returns>
        public static int RegisterTransition(BindableObject bindable)
        {
            return RegisterTransition(bindable, 0);
        }

        /// <summary>
        /// Registers the transition element in the TransitionStack
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="nativeViewId">The platform View identifier</param>
        /// <returns>The unique Id of the native View</returns>
        public static int RegisterTransition(BindableObject bindable, int nativeViewId)
        {
            if (bindable is View element)
            {
                var transitionName = GetTransitionName(element);
                if (!(element.Navigation?.NavigationStack.Count > 0) || string.IsNullOrEmpty(transitionName)) return 0;

                var currentPage = element.Navigation.NavigationStack.Last();
                if (currentPage.Parent is SharedTransitionNavigationPage navPage)
                {
                    return navPage.TransitionMap.Add(currentPage, transitionName,element.Id, nativeViewId);
                }
            }

            return 0;
        }

        /// <summary>
        /// Called when a property is changed.
        /// </summary>
        /// <param name="bindable">Xamarin Forms Element</param>
        /// <param name="oldValue">The old value</param>
        /// <param name="newValue">The new value</param>
        static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable == null)
                return;

            var element = (View)bindable;
            var existing = element.Effects.FirstOrDefault(x => x is TransitionEffect);

            if (existing == null && newValue != null && (int)newValue > 0)
            {
                element.Effects.Add(new TransitionEffect());
            }
            else if (existing != null)
            {
                element.Effects.Remove(existing);
            }
        }
    }
}
