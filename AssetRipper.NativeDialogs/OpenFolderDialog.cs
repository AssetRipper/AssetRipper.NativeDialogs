using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class OpenFolderDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<string?> OpenFolderAsync()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFolderAsyncWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFolderAsyncMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFolderAsyncLinux();
		}
		else
		{
			return Task.FromResult<string?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string?> OpenFolderAsyncWindows()
	{
		return Task.FromResult<string?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static Task<string?> OpenFolderAsyncMacOS()
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose folder)");
	}

	[SupportedOSPlatform("linux")]
	private static Task<string?> OpenFolderAsyncLinux()
	{
		return Task.FromResult<string?>(null);
	}

	public static Task<string[]?> OpenFoldersAsync()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFoldersAsyncWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFoldersAsyncMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFoldersAsyncLinux();
		}
		else
		{
			return Task.FromResult<string[]?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private static async Task<string[]?> OpenFoldersAsyncWindows()
	{
		// Todo: proper Windows implementation
		string? path = await OpenFolderAsync();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}

	[SupportedOSPlatform("macos")]
	private static async Task<string[]?> OpenFoldersAsyncMacOS()
	{
		// Todo: proper Mac implementation
		string? path = await OpenFolderAsync();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}

	[SupportedOSPlatform("linux")]
	private static async Task<string[]?> OpenFoldersAsyncLinux()
	{
		// Todo: proper Linux implementation
		string? path = await OpenFolderAsync();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}
}
