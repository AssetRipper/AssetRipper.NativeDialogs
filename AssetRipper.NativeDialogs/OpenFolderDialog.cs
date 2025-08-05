using System.Diagnostics;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class OpenFolderDialog
{
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
	private static Task<string?> OpenFolderWindows()
	{
		TaskCompletionSource<string?> tcs = new();

		Thread thread = new(() =>
		{
			try
			{
				// Run the STA work
				Debug.Assert(OperatingSystem.IsWindows());
				string? result = OpenFolderWindowsInternal();

				// Mark task complete
				tcs.SetResult(result);
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();

		return tcs.Task;
	}

	[SupportedOSPlatform("windows")]
	private unsafe static string? OpenFolderWindowsInternal()
	{
		string? result = null;

		HRESULT hr = Windows.CoInitializeEx(null, (uint)COINIT.COINIT_APARTMENTTHREADED);
		switch (hr.Value)
		{
			case S.S_OK:
				Windows.CoUninitialize();
				throw new InvalidOperationException("CoInitializeEx failed with S_OK, which should never happen because .NET is supposed to initialize the thread.");
			case S.S_FALSE:
				// The thread is already initialized, which is expected.
				break;
			case RPC.RPC_E_CHANGED_MODE:
				// The thread is already initialized with a different mode, which is unexpected.
				throw new InvalidOperationException("CoInitializeEx failed with RPC_E_CHANGED_MODE, which should never happen because we only call this method in STA threads.");
		}

		IFileOpenDialog* pFileDialog = null;

		// Assign the CLSID and IID for the FileOpenDialog.
		Guid CLSID_FileOpenDialog = new("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7");
		Guid IID_IFileOpenDialog = new("D57C7288-D4AD-4768-BE02-9D969532D960");

		// Create the FileOpenDialog object.
		hr = Windows.CoCreateInstance(&CLSID_FileOpenDialog, null, (uint)CLSCTX.CLSCTX_INPROC_SERVER, &IID_IFileOpenDialog, (void**)&pFileDialog);
		if (!Windows.SUCCEEDED(hr))
		{
			return null;
		}

		// Set the options on the dialog.
		uint dwOptions;
		pFileDialog->GetOptions(&dwOptions);
		pFileDialog->SetOptions(dwOptions | (uint)FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS | (uint)FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);

		// Show the dialog
		hr = pFileDialog->Show(default);
		if (Windows.SUCCEEDED(hr))
		{
			IShellItem* pItem;
			hr = pFileDialog->GetResult(&pItem);
			if (Windows.SUCCEEDED(hr))
			{
				char* pszFilePath = null;
				hr = pItem->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &pszFilePath);
				if (Windows.SUCCEEDED(hr))
				{
					result = new string(pszFilePath);
					Windows.CoTaskMemFree(pszFilePath);
				}
				pItem->Release();
			}
		}
		pFileDialog->Release();

		return result;
	}

	[SupportedOSPlatform("macos")]
	private static Task<string?> OpenFolderMacOS()
	{
		return ProcessExecutor.TryRun("osascript", "-e", "POSIX path of (choose folder)");
	}

	[SupportedOSPlatform("linux")]
	private static Task<string?> OpenFolderLinux()
	{
		if (Gtk.Global.IsSupported)
		{
			string? result;
			GtkHelper.EnsureInitialized();

			using Gtk.FileChooserNative dlg = new(
				"Open a folder", null,
				Gtk.FileChooserAction.SelectFolder, "Open", "Cancel");

			if (dlg.Run() == (int)Gtk.ResponseType.Accept)
			{
				result = dlg.Filename;
			}
			else
			{
				result = null; // User canceled the dialog
			}

			return Task.FromResult(result);
		}
		else
		{
			// Fallback
			return Task.FromResult<string?>(null);
		}
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
	private static Task<string[]?> OpenFoldersWindows()
	{
		TaskCompletionSource<string[]?> tcs = new();

		Thread thread = new(() =>
		{
			try
			{
				// Run the STA work
				Debug.Assert(OperatingSystem.IsWindows());
				string[]? result = OpenFoldersWindowsInternal();

				// Mark task complete
				tcs.SetResult(result);
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();

		return tcs.Task;
	}

	[SupportedOSPlatform("windows")]
	private unsafe static string[]? OpenFoldersWindowsInternal()
	{
		HRESULT hr = Windows.CoInitializeEx(null, (uint)COINIT.COINIT_APARTMENTTHREADED);
		switch (hr.Value)
		{
			case S.S_OK:
				Windows.CoUninitialize();
				throw new InvalidOperationException("CoInitializeEx failed with S_OK, which should never happen because .NET is supposed to initialize the thread.");
			case S.S_FALSE:
				// The thread is already initialized, which is expected.
				break;
			case RPC.RPC_E_CHANGED_MODE:
				// The thread is already initialized with a different mode, which is unexpected.
				throw new InvalidOperationException("CoInitializeEx failed with RPC_E_CHANGED_MODE, which should never happen because we only call this method in STA threads.");
		}

		IFileOpenDialog* pFileDialog = null;

		// Assign the CLSID and IID for the FileOpenDialog.
		Guid CLSID_FileOpenDialog = new("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7");
		Guid IID_IFileOpenDialog = new("D57C7288-D4AD-4768-BE02-9D969532D960");

		// Create the FileOpenDialog object.
		hr = Windows.CoCreateInstance(&CLSID_FileOpenDialog, null, (uint)CLSCTX.CLSCTX_INPROC_SERVER, &IID_IFileOpenDialog, (void**)&pFileDialog);
		if (!Windows.SUCCEEDED(hr))
		{
			return null;
		}

		// Set the options on the dialog.
		uint dwOptions = default;
		pFileDialog->GetOptions(&dwOptions);
		pFileDialog->SetOptions(dwOptions | (uint)FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS | (uint)FILEOPENDIALOGOPTIONS.FOS_ALLOWMULTISELECT | (uint)FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);

		string[]? result = null;

		// Show the dialog
		hr = pFileDialog->Show(default);
		if (Windows.SUCCEEDED(hr))
		{
			IShellItemArray* pItemArray;
			hr = pFileDialog->GetResults(&pItemArray);
			if (Windows.SUCCEEDED(hr))
			{
				uint itemCount = default;
				pItemArray->GetCount(&itemCount);
				if (itemCount > 0)
				{
					result = new string[itemCount];
					for (uint i = 0; i < itemCount; i++)
					{
						IShellItem* pItem;
						hr = pItemArray->GetItemAt(i, &pItem);
						if (Windows.SUCCEEDED(hr))
						{
							char* pszFilePath = null;
							hr = pItem->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &pszFilePath);
							if (Windows.SUCCEEDED(hr))
							{
								result[i] = new string(pszFilePath);
								Windows.CoTaskMemFree(pszFilePath);
							}
							pItem->Release();
						}
					}
				}
				pItemArray->Release();
			}
		}
		pFileDialog->Release();

		return result;
	}

	[SupportedOSPlatform("macos")]
	private static async Task<string[]?> OpenFoldersMacOS()
	{
		ReadOnlySpan<string> arguments =
		[
			"-e", "set theFolders to choose folder with multiple selections allowed",
			"-e", "set folderPaths to {}",
			"-e", "repeat with aFolder in theFolders",
			"-e", "set end of folderPaths to POSIX path of aFolder",
			"-e", "end repeat",
			"-e", "set text item delimiters to \":\"",
			"-e", "return folderPaths as string",
		];
		string? output = await ProcessExecutor.TryRun("osascript", arguments);
		if (string.IsNullOrEmpty(output))
		{
			return null; // User canceled the dialog
		}
		return output.Split(':');
	}

	[SupportedOSPlatform("linux")]
	private static Task<string[]?> OpenFoldersLinux()
	{
		if (Gtk.Global.IsSupported)
		{
			string[]? result;
			GtkHelper.EnsureInitialized();

			using Gtk.FileChooserNative dlg = new(
				"Open folders", null,
				Gtk.FileChooserAction.SelectFolder, "Open", "Cancel");

			dlg.SelectMultiple = true; // Allow multiple folder selection

			if (dlg.Run() == (int)Gtk.ResponseType.Accept)
			{
				result = dlg.Filenames;
			}
			else
			{
				result = null; // User canceled the dialog
			}

			return Task.FromResult(result);
		}
		else
		{
			// Fallback
			return Task.FromResult<string[]?>(null);
		}
	}
}
