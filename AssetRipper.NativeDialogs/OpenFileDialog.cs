using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class OpenFileDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<string?> OpenFileAsync(OpenFileDialogOptions? options = null)
	{
		options ??= OpenFileDialogOptions.Default;
		if (OperatingSystem.IsWindows())
		{
			return OpenFileAsyncWindows(options);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFileAsyncMacOS(options);
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFileAsyncLinux(options);
		}
		else
		{
			return Task.FromResult<string?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string?> OpenFileAsyncWindows(OpenFileDialogOptions options)
	{
		// https://learn.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamew

		char[] buffer = ArrayPool<char>.Shared.Rent(ushort.MaxValue + 1); // Should be enough for the overwhelming majority of cases.
		new Span<char>(buffer).Clear();

		string filter;
		if (options.Filters.Count == 0)
		{
			filter = "All Files\0*.*\0\0";
		}
		else
		{
			StringBuilder filterBuilder = new();
			foreach (KeyValuePair<string, string> pair in options.Filters)
			{
				filterBuilder
					.Append(pair.Key).Append('\0')
					.Append("*.").Append(pair.Value).Append('\0');
			}
			filterBuilder.Append('\0'); // End of filter list
			filter = filterBuilder.ToString();
		}

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
	private static Task<string?> OpenFileAsyncMacOS(OpenFileDialogOptions options)
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose file)");
	}

	[SupportedOSPlatform("linux")]
	private static Task<string?> OpenFileAsyncLinux(OpenFileDialogOptions options)
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
