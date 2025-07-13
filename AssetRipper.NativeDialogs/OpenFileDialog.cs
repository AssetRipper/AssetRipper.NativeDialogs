using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class OpenFileDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<string?> OpenFileAsync()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFileAsyncWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFileAsyncMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFileAsyncLinux();
		}
		else
		{
			return Task.FromResult<string?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string?> OpenFileAsyncWindows()
	{
		// https://learn.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamew

		char[] buffer = ArrayPool<char>.Shared.Rent(ushort.MaxValue + 1); // Should be enough for the overwhelming majority of cases.
		new Span<char>(buffer).Clear();

		string filter = "All Files\0*.*\0\0";

		fixed (char* bufferPtr = buffer)
		fixed (char* filterPtr = filter)
		{
			OPENFILENAMEW ofn = default;
			ofn.lStructSize = (uint)Unsafe.SizeOf<OPENFILENAMEW>();
			ofn.hwndOwner = default; // No owner window.
			ofn.lpstrFile = bufferPtr;
			ofn.nMaxFile = (uint)buffer.Length;
			ofn.lpstrFilter = filterPtr;
			ofn.nFilterIndex = 1; // The first pair of strings has an index value of 1.
			ofn.Flags = OFN.OFN_PATHMUSTEXIST | OFN.OFN_FILEMUSTEXIST;
			if (Windows.GetOpenFileNameW(&ofn))
			{
				int length = Array.IndexOf(buffer, '\0');
				if (length > 0)
				{
					string result = new(buffer, 0, length);
					ArrayPool<char>.Shared.Return(buffer);
					return Task.FromResult<string?>(result);
				}
			}
		}

		ArrayPool<char>.Shared.Return(buffer);
		return Task.FromResult<string?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static Task<string?> OpenFileAsyncMacOS()
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose file)");
	}

	[SupportedOSPlatform("linux")]
	private static Task<string?> OpenFileAsyncLinux()
	{
		if (Gtk.Global.IsSupported)
		{
			string? result;
			Gtk.Application.Init(); // spins a main loop
			try
			{
				using Gtk.FileChooserNative dlg = new(
					"Open a file", null,
					Gtk.FileChooserAction.Open, "Open", "Cancel");

				if (dlg.Run() == (int)Gtk.ResponseType.Accept)
				{
					result = dlg.File?.Path;
				}
				else
				{
					result = null; // User canceled the dialog
				}
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
			return Task.FromResult<string?>(null);
		}
	}
}
