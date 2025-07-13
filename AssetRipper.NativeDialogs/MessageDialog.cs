using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class MessageDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task Message(string message, string label)
	{
		if (OperatingSystem.IsWindows())
		{
			return MessageWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return MessageMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return MessageLinux();
		}
		else
		{
			return Task.CompletedTask;
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task MessageWindows()
	{
		return Task.CompletedTask;
	}

	[SupportedOSPlatform("macos")]
	private static Task MessageMacOS()
	{
		return Task.CompletedTask;
	}

	[SupportedOSPlatform("linux")]
	private static Task MessageLinux()
	{
		if (Gtk.Global.IsSupported)
		{
			Gtk.Application.Init(); // spins a main loop
			try
			{
			}
			finally
			{
				Gtk.Application.Quit(); // stops the main loop
			}

			return Task.CompletedTask;
		}
		else
		{
			// Fallback
			return Task.CompletedTask;
		}
	}
}
