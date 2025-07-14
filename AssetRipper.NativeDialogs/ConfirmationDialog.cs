using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class ConfirmationDialog
{
	public enum Type
	{
		OkCancel,
		YesNo,
	}

	public readonly struct Options
	{
		public required string Message { get; init; }
		public Type Type { get; init; } = Type.OkCancel;
		internal string TrueLabel => Type == Type.OkCancel ? "OK" : "Yes";
		internal string FalseLabel => Type == Type.OkCancel ? "Cancel" : "No";

		public Options()
		{
		}

		[SetsRequiredMembers]
		public Options(string message, Type type = Type.OkCancel)
		{
			Message = message;
			Type = type;
		}
	}

	public static Task<bool?> Confirm(Options options)
	{
		ArgumentException.ThrowIfNullOrEmpty(options.Message);

		if (OperatingSystem.IsWindows())
		{
			return ConfirmWindows(options);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return ConfirmMacOS(options);
		}
		else if (OperatingSystem.IsLinux())
		{
			return ConfirmLinux(options);
		}
		else
		{
			return Task.FromResult<bool?>(false);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<bool?> ConfirmWindows(Options options)
	{
		int statusCode;
		fixed (char* messagePtr = options.Message)
		{
			// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messageboxw
			statusCode = Windows.MessageBoxW(
				default, // No owner window.
				messagePtr, // Message text.
				null, // Title.
				(options.Type == Type.OkCancel ? (uint)MB.MB_OKCANCEL : MB.MB_YESNO) | MB.MB_ICONQUESTION); // OK button and information icon.
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
	private static async Task<bool?> ConfirmMacOS(Options options)
	{
		string escapedMessage = ProcessExecutor.EscapeString(options.Message);
		string escapedTrueLabel = ProcessExecutor.EscapeString(options.TrueLabel);
		string escapedFalseLabel = ProcessExecutor.EscapeString(options.FalseLabel);
		string? result = await ProcessExecutor.TryRun("osascript",
			"-e", $"display dialog \"{escapedMessage}\" buttons {{\"{escapedFalseLabel}\", \"{escapedTrueLabel}\"}} default button \"{escapedTrueLabel}\"",
			"-e", "button returned of result");
		if (result == options.TrueLabel)
		{
			return true;
		}
		else if (result == options.FalseLabel)
		{
			return false;
		}
		else
		{
			return null; // An error occurred
		}
	}

	[SupportedOSPlatform("linux")]
	private static Task<bool?> ConfirmLinux(Options options)
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
					options.Type == Type.OkCancel ? Gtk.ButtonsType.OkCancel : Gtk.ButtonsType.YesNo,
					options.Message
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
				//Gtk.Application.Quit(); // stops the main loop
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
