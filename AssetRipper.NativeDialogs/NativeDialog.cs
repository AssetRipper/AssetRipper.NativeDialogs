namespace AssetRipper.NativeDialogs;

public static class NativeDialog
{
	private static bool SupportedLinux { get; } = OperatingSystem.IsLinux() && LinuxHelper.HasSupportedBackend().WaitForResult();
	public static bool Supported => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || (OperatingSystem.IsLinux() && SupportedLinux);

	private static T WaitForResult<T>(this Task<T> task)
	{
		task.Wait();
		return task.Result;
	}
}
