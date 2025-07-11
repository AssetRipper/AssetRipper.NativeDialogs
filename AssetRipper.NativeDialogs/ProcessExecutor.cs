using System.Diagnostics;

namespace AssetRipper.NativeDialogs;

internal static class ProcessExecutor
{
	public static async Task<string?> TryRun(string command, params ReadOnlySpan<string> arguments)
	{
		Process p = new()
		{
			StartInfo = new()
			{
				FileName = command,
				RedirectStandardOutput = true,
			},
		};
		foreach (string arg in arguments)
		{
			p.StartInfo.ArgumentList.Add(arg);
		}
		if (!p.Start())
		{
			return null;
		}

		string? path = p.StandardOutput.ReadLine();
		await p.WaitForExitAsync();
		return p.ExitCode == 0 ? path : null;
	}
}
