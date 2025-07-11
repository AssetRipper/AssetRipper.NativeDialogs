using System.Diagnostics;

namespace AssetRipper.NativeDialogs;

internal static class ProcessExecutor
{
	public static string? TryRun(string command, string arguments)
	{
		Process p = new()
		{
			StartInfo = new()
			{
				FileName = command,
				Arguments = arguments,
				RedirectStandardOutput = true,
			},
		};
		if (!p.Start())
		{
			return null;
		}

		string? path = p.StandardOutput.ReadLine();
		p.WaitForExit();
		return p.ExitCode == 0 ? path : null;
	}
}
