namespace AssetRipper.NativeDialogs;

public static class NativeDialog
{
	public static bool Supported =>
		OperatingSystem.IsWindows() ||
		OperatingSystem.IsMacOS() ||
		(OperatingSystem.IsLinux() && Gtk.Global.IsSupported);
}
