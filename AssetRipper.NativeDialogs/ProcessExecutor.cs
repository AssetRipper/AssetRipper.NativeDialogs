using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

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
			StringBuilder output = new();
			process.OutputDataReceived += (sender, e) =>
			{
				if (e.Data != null)
				{
					output.AppendLine(e.Data);
				}
			};

			if (!process.Start())
			{
				return null;
			}

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			await process.WaitForExitAsync();

			string result = output.ToString().Trim();

			return process.ExitCode == 0 && result.Length > 0 ? result : null;
		}
	}

	[SupportedOSPlatform("linux")]
	public static async Task<bool> HasCommand(string command)
	{
		string? result = await TryRun("/bin/bash", "-c", $"command -v {EscapeString(command)}");
		return !string.IsNullOrEmpty(result);
	}
}
