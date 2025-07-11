using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class OpenFolderDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<string?> OpenFolderAsync(OpenFileDialogOptions? options = null)
	{
		options ??= OpenFileDialogOptions.Default;
		if (OperatingSystem.IsWindows())
		{
			return OpenFolderAsyncWindows(options);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFolderAsyncMacOS(options);
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFolderAsyncLinux(options);
		}
		else
		{
			return Task.FromResult<string?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string?> OpenFolderAsyncWindows(OpenFileDialogOptions options)
	{
		return Task.FromResult<string?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static Task<string?> OpenFolderAsyncMacOS(OpenFileDialogOptions options)
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose folder)");
	}

	[SupportedOSPlatform("linux")]
	private static Task<string?> OpenFolderAsyncLinux(OpenFileDialogOptions options)
	{
		return Task.FromResult<string?>(null);
	}
}
