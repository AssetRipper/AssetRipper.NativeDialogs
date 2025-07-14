using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class MessageDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task Message(string message)
	{
		if (OperatingSystem.IsWindows())
		{
			return MessageWindows(message);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return MessageMacOS(message);
		}
		else if (OperatingSystem.IsLinux())
		{
			return MessageLinux(message);
		}
		else
		{
			return Task.CompletedTask;
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task MessageWindows(string message)
	{
		return Task.CompletedTask;
	}

	[SupportedOSPlatform("macos")]
	private static Task MessageMacOS(string message)
	{
		string escapedMessage = ProcessExecutor.EscapeString(message);
		return ProcessExecutor.TryRun("osascript", "-e", $"display dialog \"{escapedMessage}\"");
	}

	[SupportedOSPlatform("linux")]
	private static Task MessageLinux(string message)
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
