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
			string? file = await OpenFileDialog.OpenFileAsync();
			Print(file);
		}
		else if (arguments.OpenFiles)
		{
			string[]? files = await OpenFilesDialog.OpenFilesAsync();
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
		else if (arguments.OpenFolder)
		{
			string? folder = null;
			Print(folder);
		}
		else if (arguments.SaveFile)
		{
			string? file = null;
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
