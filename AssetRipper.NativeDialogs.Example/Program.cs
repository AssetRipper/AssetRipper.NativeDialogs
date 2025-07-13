namespace AssetRipper.NativeDialogs.Example;

internal static class Program
{
	static async Task Main(string[] args)
	{
		Arguments? arguments = Arguments.Parse(args);
		if (arguments is null)
		{
			return;
		}

		if (arguments.OpenFile)
		{
			if (arguments.AllowMultiple)
			{
				string[]? files = await OpenFileDialog.OpenFilesAsync();
				if (files is null || files.Length == 0)
				{
					Console.WriteLine("No files selected.");
				}
				else
				{
					foreach (string file in files)
					{
						Print(file);
					}
				}
			}
			else
			{
				string? file = await OpenFileDialog.OpenFileAsync();
				Print(file);
			}
		}
		else if (arguments.OpenFolder)
		{
			if (arguments.AllowMultiple)
			{
				string[]? folders = await OpenFolderDialog.OpenFoldersAsync();
				if (folders is null || folders.Length == 0)
				{
					Console.WriteLine("No folders selected.");
				}
				else
				{
					foreach (string file in folders)
					{
						Print(file);
					}
				}
			}
			else
			{
				string? folder = await OpenFolderDialog.OpenFolderAsync();
				Print(folder);
			}
		}
		else if (arguments.SaveFile)
		{
			string? file = await SaveFileDialog.SaveFileAsync();
			Print(file);
		}
		else if (arguments.Message)
		{
			throw new NotImplementedException("Message dialog is not implemented in this example.");
		}
		else
		{
			Console.WriteLine("No action specified. Use --help for usage information.");
		}
	}

	private static void Print(string? value)
	{
		Console.WriteLine(value ?? "null");
	}
}
