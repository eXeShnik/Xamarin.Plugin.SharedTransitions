using AndroidX.Fragment.App;
using SupportTransitions = AndroidX.Transitions;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers;

public interface ITransitionRenderer
{
    FragmentManager SupportFragmentManager { get; set; }
    string SelectedGroup { get; set; }
    bool IsInTabbedPage { get; set; }
    BackgroundAnimation BackgroundAnimation { get; set; }
    Page PropertiesContainer { get; set; }
    Page LastPageInStack { get; set; }
    ITransitionMapper TransitionMap { get; set; }
    SupportTransitions.Transition InflateTransitionInContext();
    void SharedTransitionStarted();
    void SharedTransitionEnded();
    void SharedTransitionCancelled();
}