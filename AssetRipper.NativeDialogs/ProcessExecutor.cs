using System.Diagnostics;

namespace AssetRipper.NativeDialogs;

internal static class ProcessExecutor
{
	public static string EscapeString(string str)
	{
		return str
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("'", "\\'");
	}

	public static Task<string?> TryRun(string command, params ReadOnlySpan<string> arguments)
	{
		Process p = new()
		{
			StartInfo = new()
			{
				FileName = command,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			},
		};
		foreach (string arg in arguments)
		{
			p.StartInfo.ArgumentList.Add(arg);
		}
		return TryRunProcess(p);

		static async Task<string?> TryRunProcess(Process process)
		{
			if (!process.Start())
			{
				return null;
			}

			string? path = process.StandardOutput.ReadLine();

			await process.WaitForExitAsync();

			return process.ExitCode == 0 ? path : null;
		}
	}
}
