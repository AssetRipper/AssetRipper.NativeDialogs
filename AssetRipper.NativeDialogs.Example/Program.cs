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
				string[]? files = await OpenFileDialog.OpenFiles();
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
				string? file = await OpenFileDialog.OpenFile();
				Print(file);
			}
		}
		else if (arguments.OpenFolder)
		{
			if (arguments.AllowMultiple)
			{
				string[]? folders = await OpenFolderDialog.OpenFolders();
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
				string? folder = await OpenFolderDialog.OpenFolder();
				Print(folder);
			}
		}
		else if (arguments.SaveFile)
		{
			string? file = await SaveFileDialog.SaveFile();
			Print(file);
		}
		else if (arguments.Confirmation)
		{
			bool? result = await ConfirmationDialog.Confirm("Are you sure you want to proceed?", "Yes", "No");
			switch (result)
			{
				case true:
					Console.WriteLine("Confirmed.");
					break;
				case false:
					Console.WriteLine("Cancelled.");
					break;
				case null:
					Console.WriteLine("No response.");
					break;
			}
		}
		else if (arguments.Message)
		{
			await MessageDialog.Message("This is a message dialog.", "OK");
			Console.WriteLine("Message dialog displayed.");
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
