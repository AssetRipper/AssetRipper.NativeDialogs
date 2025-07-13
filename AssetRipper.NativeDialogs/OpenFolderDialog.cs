using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

public static class OpenFolderDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<string?> OpenFolder()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFolderWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFolderMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFolderLinux();
		}
		else
		{
			return Task.FromResult<string?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string?> OpenFolderWindows()
	{
		return Task.FromResult<string?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static Task<string?> OpenFolderMacOS()
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose folder)");
	}

	[SupportedOSPlatform("linux")]
	private static Task<string?> OpenFolderLinux()
	{
		return Task.FromResult<string?>(null);
	}

	public static Task<string[]?> OpenFolders()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFoldersWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFoldersMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFoldersLinux();
		}
		else
		{
			return Task.FromResult<string[]?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private static async Task<string[]?> OpenFoldersWindows()
	{
		// Todo: proper Windows implementation
		string? path = await OpenFolder();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}

	[SupportedOSPlatform("macos")]
	private static async Task<string[]?> OpenFoldersMacOS()
	{
		// Todo: proper Mac implementation
		string? path = await OpenFolder();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}

	[SupportedOSPlatform("linux")]
	private static async Task<string[]?> OpenFoldersLinux()
	{
		// Todo: proper Linux implementation
		string? path = await OpenFolder();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}
}
