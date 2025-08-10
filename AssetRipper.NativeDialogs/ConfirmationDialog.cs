using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace AssetRipper.NativeDialogs;

public static class ConfirmationDialog
{
	public enum Type
	{
		OkCancel,
		YesNo,
	}

	public readonly struct Options
	{
		public required string Message { get; init; }
		public Type Type { get; init; } = Type.OkCancel;
		internal string TrueLabel => Type == Type.OkCancel ? "OK" : "Yes";
		internal string FalseLabel => Type == Type.OkCancel ? "Cancel" : "No";

		public Options()
		{
		}

		[SetsRequiredMembers]
		public Options(string message, Type type = Type.OkCancel)
		{
			Message = message;
			Type = type;
		}
	}

	public static Task<bool?> Confirm(Options options)
	{
		ArgumentException.ThrowIfNullOrEmpty(options.Message);

		if (OperatingSystem.IsWindows())
		{
			return ConfirmWindows(options);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return ConfirmMacOS(options);
		}
		else if (OperatingSystem.IsLinux())
		{
			return ConfirmLinux(options);
		}
		else
		{
			return Task.FromResult<bool?>(false);
		}
	}

	[SupportedOSPlatform("windows")]
	private unsafe static Task<bool?> ConfirmWindows(Options options)
	{
		int statusCode;
		fixed (char* messagePtr = options.Message)
		{
			// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-messageboxw
			statusCode = Windows.MessageBoxW(
				default, // No owner window.
				messagePtr, // Message text.
				null, // Title.
				(options.Type == Type.OkCancel ? (uint)MB.MB_OKCANCEL : MB.MB_YESNO) | MB.MB_ICONQUESTION); // OK button and information icon.
		}

		if (statusCode == 0)
		{
			int errorCode = unchecked((int)Windows.GetLastError());
			throw new Win32Exception(errorCode, "Failed to show message dialog.");
		}
		else if (statusCode is Windows.IDYES or Windows.IDOK)
		{
			return Task.FromResult<bool?>(true);
		}
		else if (statusCode is Windows.IDNO or Windows.IDCANCEL)
		{
			return Task.FromResult<bool?>(false);
		}
		else
		{
			throw new($"Unexpected status code {statusCode} from {nameof(Windows.MessageBoxW)}.");
		}
	}

	[SupportedOSPlatform("macos")]
	private static async Task<bool?> ConfirmMacOS(Options options)
	{
		string escapedMessage = ProcessExecutor.EscapeString(options.Message);
		string escapedTrueLabel = ProcessExecutor.EscapeString(options.TrueLabel);
		string escapedFalseLabel = ProcessExecutor.EscapeString(options.FalseLabel);
		string? result = await ProcessExecutor.TryRun("osascript",
			"-e", $"display dialog \"{escapedMessage}\" buttons {{\"{escapedFalseLabel}\", \"{escapedTrueLabel}\"}} default button \"{escapedTrueLabel}\"",
			"-e", "button returned of result");
		if (result == options.TrueLabel)
		{
			return true;
		}
		else if (result == options.FalseLabel)
		{
			return false;
		}
		else
		{
			return null; // An error occurred
		}
	}

	[SupportedOSPlatform("linux")]
	private static async Task<bool?> ConfirmLinux(Options options)
	{
		string escapedMessage = ProcessExecutor.EscapeString(options.Message);
		if (await LinuxHelper.HasZenity())
		{
			Process process = new()
			{
				StartInfo = new()
				{
					FileName = "zenity",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				},
			};

			process.StartInfo.ArgumentList.Add("--question");
			process.StartInfo.ArgumentList.Add("--text");
			process.StartInfo.ArgumentList.Add(escapedMessage);
			if (options.Type != Type.YesNo)
			{
				string escapedTrueLabel = ProcessExecutor.EscapeString(options.TrueLabel);
				string escapedFalseLabel = ProcessExecutor.EscapeString(options.FalseLabel);

				process.StartInfo.ArgumentList.Add("--ok-label");
				process.StartInfo.ArgumentList.Add(escapedTrueLabel);
				process.StartInfo.ArgumentList.Add("--cancel-label");
				process.StartInfo.ArgumentList.Add(escapedFalseLabel);
			}

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			if (!process.Start())
			{
				return null; // Failed to start the process
			}

			await process.WaitForExitAsync();

			return process.ExitCode switch
			{
				0 => true, // User clicked OK or Yes
				1 => false, // User clicked Cancel or No
				_ => null, // An error occurred
			};
		}
		else if (await LinuxHelper.HasKDialog())
		{
			Process process = new()
			{
				StartInfo = new()
				{
					FileName = "kdialog",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				},
			};

			process.StartInfo.ArgumentList.Add(options.Type is Type.YesNo ? "--yesno" : "--okcancel");
			process.StartInfo.ArgumentList.Add(escapedMessage);

			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			if (!process.Start())
			{
				return null; // Failed to start the process
			}

			await process.WaitForExitAsync();

			return process.ExitCode switch
			{
				0 => true, // User clicked OK or Yes
				1 => false, // User clicked Cancel or No
				_ => null, // An error occurred
			};
		}
		else
		{
			// Fallback
			return null;
		}
	}
}
