using Android.Content;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Plugin.SharedTransitions.Platforms.Android.Renderers;

public class SharedTransitionShellRenderer : ShellRenderer
{
    public SharedTransitionShellRenderer(Context context) : base(context)
    {
    }

    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new SharedTransitionShellItemRenderer(this);
    }

}