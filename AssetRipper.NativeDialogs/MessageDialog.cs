using System.ComponentModel;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

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
		int statusCode;
		fixed (char* messagePtr = message)
		{
			// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messageboxw
			statusCode = Windows.MessageBoxW(
				default, // No owner window.
				messagePtr, // Message text.
				null, // Title.
				MB.MB_OK | MB.MB_ICONINFORMATION); // OK button and information icon.
		}

		if (statusCode == 0)
		{
			int errorCode = unchecked((int)Windows.GetLastError());
			throw new Win32Exception(errorCode, "Failed to show message dialog.");
		}
		else if (statusCode != Windows.IDOK)
		{
			throw new($"Unexpected status code {statusCode} from {nameof(Windows.MessageBoxW)}.");
		}

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
				using Gtk.MessageDialog md = new(
					null,
					Gtk.DialogFlags.Modal,
					Gtk.MessageType.Info,
					Gtk.ButtonsType.Ok,
					message
				);
				md.Run();
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
