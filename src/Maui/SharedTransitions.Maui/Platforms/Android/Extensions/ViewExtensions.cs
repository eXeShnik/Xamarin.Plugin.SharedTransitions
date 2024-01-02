﻿using View = Android.Views.View;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace Plugin.SharedTransitions.Platforms.Android.Extensions;

public static class ViewExtensions
{
    /// <summary>
    /// Detect the Fragment containing this View by traversing all the ChildFragmenManagers
    /// </summary>
    /// <param name="view">The current view</param>
    /// <param name="fragmentManager">The current FragmentManager</param>
    /// <returns></returns>
    internal static Fragment ParentFragment(this View view, FragmentManager fragmentManager)
    {
        //This is a bit clunky but is needed because in Forms EVERYTHING is internal!
        //We could use FragmentContainer or PageContainer b ut we would need reflection wich is slow
        foreach (var fragment in fragmentManager.Fragments)
        {
            if (fragment.View?.FindViewById(view.Id) != null && fragment.ChildFragmentManager?.Fragments.Count > 0)
            {

                var childManager = fragment.ChildFragmentManager.Fragments[0].ChildFragmentManager;

                return childManager?.Fragments?.Count > 0
                    ? view.ParentFragment(childManager)
                    : fragment.ChildFragmentManager.Fragments.Last();
            }
        }

        return null;
    }
}