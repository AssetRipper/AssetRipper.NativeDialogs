using System.Runtime.Versioning;

namespace AssetRipper.NativeDialogs;

[SupportedOSPlatform("linux")]
internal static class LinuxHelper
{
	private static bool? hasZenity;
	private static bool? hasKDialog;

	public static async Task<bool> HasSupportedBackend()
	{
		if (hasZenity is null)
		{
			await HasZenity();
		}
		if (hasKDialog is null)
		{
			await HasKDialog();
		}
		return hasZenity == true || hasKDialog == true;
	}

	public static async Task<bool> HasZenity()
	{
		return hasZenity ??= await ProcessExecutor.HasCommand("zenity");
	}

	public static async Task<bool> HasKDialog()
	{
		return hasKDialog ??= await ProcessExecutor.HasCommand("kdialog");
	}
}
