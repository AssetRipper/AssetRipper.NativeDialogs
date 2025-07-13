using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class MessageDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task MessageAsync(string message, string label)
	{
		if (OperatingSystem.IsWindows())
		{
			return MessageAsyncWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return MessageAsyncMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return MessageAsyncLinux();
		}
		else
		{
			return Task.CompletedTask;
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task MessageAsyncWindows()
	{
		return Task.CompletedTask;
	}

	[SupportedOSPlatform("macos")]
	private static Task MessageAsyncMacOS()
	{
		return Task.CompletedTask;
	}

	[SupportedOSPlatform("linux")]
	private static Task MessageAsyncLinux()
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
