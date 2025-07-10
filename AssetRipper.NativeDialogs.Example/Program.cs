namespace AssetRipper.NativeDialogs.Example;

internal static class Program
{
	static async Task Main(string[] args)
	{
		/*Arguments? arguments = Arguments.Parse(args);
		if (arguments is null)
		{
			return;
		}

		if (arguments.OpenFile)
		{
			string? file = await OpenFileDialog.OpenFileAsync();
			Print(file);
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
		}*/
		string? file = await OpenFileDialog.OpenFileAsync();
		Print(file);
	}

	private static void Print(string? value)
	{
		Console.WriteLine(value ?? "null");
	}
}
