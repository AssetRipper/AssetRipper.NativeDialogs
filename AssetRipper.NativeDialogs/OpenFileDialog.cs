using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class OpenFileDialog
{
	public static Task<string?> OpenFile()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFileWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFileMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFileLinux();
		}
		else
		{
			return Task.FromResult<string?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string?> OpenFileWindows()
	{
		// https://learn.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamew

		char[] buffer = ArrayPool<char>.Shared.Rent(ushort.MaxValue + 1); // Should be enough for the overwhelming majority of cases.
		new Span<char>(buffer).Clear();

		fixed (char* bufferPtr = buffer)
		fixed (char* filterPtr = "All Files (*.*)\0*.*\0\0")
		{
			OPENFILENAMEW ofn = default;
			ofn.lStructSize = (uint)Unsafe.SizeOf<OPENFILENAMEW>();
			ofn.hwndOwner = default; // No owner window.
			ofn.lpstrFile = bufferPtr;
			ofn.nMaxFile = (uint)buffer.Length;
			ofn.lpstrFilter = filterPtr;
			ofn.nFilterIndex = 1; // The first pair of strings has an index value of 1.
			ofn.Flags = OFN.OFN_PATHMUSTEXIST | OFN.OFN_FILEMUSTEXIST | OFN.OFN_EXPLORER;
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
	private static Task<string?> OpenFileMacOS()
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose file)");
	}

	[SupportedOSPlatform("linux")]
	private static async Task<string?> OpenFileLinux()
	{
		if (await LinuxHelper.HasZenity())
		{
			return await ProcessExecutor.TryRun("zenity", "--file-selection");
		}
		else if (await LinuxHelper.HasKDialog())
		{
			return await ProcessExecutor.TryRun("kdialog", "--getopenfilename");
		}
		else
		{
			// Fallback
			return null;
		}
	}

	public static Task<string[]?> OpenFiles()
	{
		if (OperatingSystem.IsWindows())
		{
			return OpenFilesWindows();
		}
		else if (OperatingSystem.IsMacOS())
		{
			return OpenFilesMacOS();
		}
		else if (OperatingSystem.IsLinux())
		{
			return OpenFilesLinux();
		}
		else
		{
			return Task.FromResult<string[]?>(null);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<string[]?> OpenFilesWindows()
	{
		// https://learn.microsoft.com/en-us/windows/win32/api/commdlg/ns-commdlg-openfilenamew

		char[] buffer = ArrayPool<char>.Shared.Rent(ushort.MaxValue + 1); // Should be enough for the overwhelming majority of cases.
		new Span<char>(buffer).Clear();

		fixed (char* bufferPtr = buffer)
		fixed (char* filterPtr = "All Files (*.*)\0*.*\0\0")
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
	private static async Task<string[]?> OpenFilesMacOS()
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
	private static async Task<string[]?> OpenFilesLinux()
	{
		if (await LinuxHelper.HasZenity())
		{
			string? output = await ProcessExecutor.TryRun("zenity", "--file-selection", "--multiple");
			if (string.IsNullOrEmpty(output))
			{
				return null; // User canceled the dialog
			}
			return output.Split('|');
		}
		else if (await LinuxHelper.HasKDialog())
		{
			string? output = await ProcessExecutor.TryRun("kdialog", "--getopenfilenames");
			if (string.IsNullOrEmpty(output))
			{
				return null; // User canceled the dialog
			}
			return output.Split('\n');
		}
		else
		{
			// Fallback
			return null;
		}
	}
}
