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
		if (OperatingSystem.IsWindows())
		{
			return ConfirmWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return ConfirmMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return ConfirmLinux();
		}
		else
		{
			return Task.FromResult<bool?>(false);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<bool?> ConfirmWindows()
	{
		return Task.FromResult<bool?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static Task<bool?> ConfirmMacOS()
	{
		return Task.FromResult<bool?>(null);
	}

	[SupportedOSPlatform("linux")]
	private static Task<bool?> ConfirmLinux()
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
