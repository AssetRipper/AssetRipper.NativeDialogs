using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class ConfirmationDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

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
		return Task.FromResult<bool?>(null);
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
				result = null;
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
