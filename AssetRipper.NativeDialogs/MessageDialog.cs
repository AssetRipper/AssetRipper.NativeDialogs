using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class MessageDialog
{
	public readonly struct Options
	{
		public required string Message { get; init; }

		public Options()
		{
		}

		[SetsRequiredMembers]
		public Options(string message)
		{
			Message = message;
		}
	}

	public static Task Message(string message) => Message(new Options(message));

	public static Task Message(Options options)
	{
		if (OperatingSystem.IsWindows())
		{
			return MessageWindows(options);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return MessageMacOS(options);
		}
		else if (OperatingSystem.IsLinux())
		{
			return MessageLinux(options);
		}
		else
		{
			return Task.CompletedTask;
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task MessageWindows(Options options)
	{
		int statusCode;
		fixed (char* messagePtr = options.Message)
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
	private static Task MessageMacOS(Options options)
	{
		string escapedMessage = ProcessExecutor.EscapeString(options.Message);
		return ProcessExecutor.TryRun("osascript", "-e", $"display dialog \"{escapedMessage}\" buttons {{\"OK\"}}");
	}

	[SupportedOSPlatform("linux")]
	private static async Task MessageLinux(Options options)
	{
		string escapedMessage = ProcessExecutor.EscapeString(options.Message);
		if (await LinuxHelper.HasZenity())
		{
			await ProcessExecutor.TryRun("zenity", "--info", "--text", escapedMessage);
		}
		else if (await LinuxHelper.HasKDialog())
		{
			await ProcessExecutor.TryRun("kdialog", "--msgbox", escapedMessage);
		}
		else
		{
			// Fallback: do nothing
		}
	}
}
