using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class ConfirmationDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<bool?> ConfirmAsync(string message, string trueLabel, string falseLabel)
	{
		if (OperatingSystem.IsWindows())
		{
			return ConfirmAsyncWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return ConfirmAsyncMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return ConfirmAsyncLinux();
		}
		else
		{
			return Task.FromResult<bool?>(false);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<bool?> ConfirmAsyncWindows()
	{
		return Task.FromResult<bool?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static Task<bool?> ConfirmAsyncMacOS()
	{
		return Task.FromResult<bool?>(null);
	}

	[SupportedOSPlatform("linux")]
	private static Task<bool?> ConfirmAsyncLinux()
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
