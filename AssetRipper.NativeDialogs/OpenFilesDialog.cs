﻿using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class OpenFilesDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);

	public static Task<string[]?> OpenFilesAsync(OpenFileDialogOptions? options = null)
	{
		options ??= OpenFileDialogOptions.Default;
		if (OperatingSystem.IsWindows())
		{
			return OpenFilesAsyncWindows(options);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFilesAsyncMacOS(options);
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFilesAsyncLinux(options);
		}
		else
		{
			return Task.FromResult<string[]?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string[]?> OpenFilesAsyncWindows(OpenFileDialogOptions options)
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
			ofn.Flags = OFN.OFN_PATHMUSTEXIST | OFN.OFN_FILEMUSTEXIST | OFN.OFN_ALLOWMULTISELECT | OFN.OFN_EXPLORER;
			if (Windows.GetOpenFileNameW(&ofn) && buffer[^1] == 0)
			{
				List<string> files = [];

				int directoryLength = Array.IndexOf(buffer, '\0');
				string directory = new(buffer, 0, directoryLength);

				int startIndex = directoryLength + 1;
				while (startIndex < buffer.Length && buffer[startIndex] != '\0')
				{
					int endIndex = Array.IndexOf(buffer, '\0', startIndex);
					string fileName = new(buffer, startIndex, endIndex - startIndex);
					files.Add(Path.Combine(directory, fileName));
					startIndex = endIndex + 1; // Move to the next file name
				}

				ArrayPool<char>.Shared.Return(buffer);
				if (files.Count > 0)
				{
					return Task.FromResult<string[]?>(files.ToArray());
				}
				else
				{
					// If a single file was selected, the system appends it to the directory path.
					return Task.FromResult<string[]?>([directory]);
				}
			}
		}

		ArrayPool<char>.Shared.Return(buffer);
		return Task.FromResult<string[]?>(null);
	}

	[SupportedOSPlatform("macos")]
	private static async Task<string[]?> OpenFilesAsyncMacOS(OpenFileDialogOptions options)
	{
		ReadOnlySpan<string> arguments =
		[
			"-e", "set theFiles to choose file with multiple selections allowed",
			"-e", "set filePaths to {}",
			"-e", "repeat with aFile in theFiles",
			"-e", "set end of filePaths to POSIX path of aFile",
			"-e", "end repeat",
			"-e", "set text item delimiters to \":\"",
			"-e", "return filePaths as string",
		];
		string? output = await ProcessExecutor.TryRun("osascript", arguments);
		if (string.IsNullOrEmpty(output))
		{
			return null; // User canceled the dialog
		}
		return output.Split(':');
	}

	[SupportedOSPlatform("linux")]
	private static async Task<string[]?> OpenFilesAsyncLinux(OpenFileDialogOptions options)
	{
		// Todo: proper Linux implementation
		string? path = await OpenFileDialog.OpenFileAsync();
		if (string.IsNullOrEmpty(path))
		{
			return null; // User canceled the dialog
		}
		return [path];
	}
}
