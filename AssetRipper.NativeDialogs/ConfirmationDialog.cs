using System.ComponentModel;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class ConfirmationDialog
{
	public static Task<bool?> Confirm(string message, string trueLabel, string falseLabel)
	{
		ArgumentException.ThrowIfNullOrEmpty(message);
		ArgumentException.ThrowIfNullOrEmpty(trueLabel);
		ArgumentException.ThrowIfNullOrEmpty(falseLabel);
		ArgumentOutOfRangeException.ThrowIfEqual(trueLabel, falseLabel);

		if (OperatingSystem.IsWindows())
		{
			return ConfirmWindows(message, trueLabel, falseLabel);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return ConfirmMacOS(message, trueLabel, falseLabel);
		}
		else if (OperatingSystem.IsLinux())
		{
			return ConfirmLinux(message, trueLabel, falseLabel);
		}
		else
		{
			return Task.FromResult<bool?>(false);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<bool?> ConfirmWindows(string message, string trueLabel, string falseLabel)
	{
		int statusCode;
		fixed (char* messagePtr = message)
		{
			// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messageboxw
			statusCode = Windows.MessageBoxW(
				default, // No owner window.
				messagePtr, // Message text.
				null, // Title.
				MB.MB_OKCANCEL | MB.MB_ICONQUESTION); // OK button and information icon.
		}

		if (statusCode == 0)
		{
			int errorCode = unchecked((int)Windows.GetLastError());
			throw new Win32Exception(errorCode, "Failed to show message dialog.");
		}
		else if (statusCode is Windows.IDYES or Windows.IDOK)
		{
			return Task.FromResult<bool?>(true);
		}
		else if (statusCode is Windows.IDNO or Windows.IDCANCEL)
		{
			return Task.FromResult<bool?>(false);
		}
		else
		{
			throw new($"Unexpected status code {statusCode} from {nameof(Windows.MessageBoxW)}.");
		}
	}

	[SupportedOSPlatform("macos")]
	private static async Task<bool?> ConfirmMacOS(string message, string trueLabel, string falseLabel)
	{
		string escapedMessage = ProcessExecutor.EscapeString(message);
		string escapedTrueLabel = ProcessExecutor.EscapeString(trueLabel);
		string escapedFalseLabel = ProcessExecutor.EscapeString(falseLabel);
		string? result = await ProcessExecutor.TryRun("osascript",
			"-e", $"display dialog \"{escapedMessage}\" buttons {{\"{escapedTrueLabel}\", \"{escapedFalseLabel}\"}} default button \"{escapedTrueLabel}\"",
			"-e", "button returned of result");
		if (result == trueLabel)
		{
			return true;
		}
		else if (result == falseLabel)
		{
			return false;
		}
		else
		{
			return null; // An error occurred
		}
	}

	[SupportedOSPlatform("linux")]
	private static Task<bool?> ConfirmLinux(string message, string trueLabel, string falseLabel)
	{
		if (Gtk.Global.IsSupported)
		{
			bool? result;
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
				int response = md.Run();
				result = response switch
				{
					(int)Gtk.ResponseType.Ok or (int)Gtk.ResponseType.Yes => true,
					(int)Gtk.ResponseType.Cancel or (int)Gtk.ResponseType.No => false,
					_ => throw new($"Unexpected response type: {response}"),
				};
			}
			finally
			{
				Gtk.Application.Quit(); // stops the main loop
			}

			return Task.FromResult(result);
		}
		else
		{
			// Fallback
			return Task.FromResult<bool?>(null);
		}
	}
}
