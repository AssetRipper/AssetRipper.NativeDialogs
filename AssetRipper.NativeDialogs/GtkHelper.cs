namespace AssetRipper.NativeDialogs;

internal static class GtkHelper
{
	private static bool isInitialized;

	public static void EnsureInitialized()
	{
		if (!isInitialized)
		{
			Gtk.Application.Init();
			isInitialized = true;
		}
	}
}
