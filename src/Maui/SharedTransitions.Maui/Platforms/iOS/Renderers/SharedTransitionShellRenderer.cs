using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Plugin.SharedTransitions.Platforms.iOS.Renderers
{
	public class SharedTransitionShellRenderer : ShellRenderer
	{
		protected override IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection)
		{
			return new SharedTransitionShellSectionRenderer(this);
		}
	}
}
