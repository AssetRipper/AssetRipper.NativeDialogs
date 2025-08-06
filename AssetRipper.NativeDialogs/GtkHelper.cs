using System.Diagnostics;

namespace AssetRipper.NativeDialogs;

internal static class GtkHelper
{
	private static bool isInitialized;

	public static bool TryInitialize()
	{
		bool wasInitialized = Interlocked.CompareExchange(ref isInitialized, true, false);
		if (wasInitialized)
		{
			return false; // Already initialized
		}

		Gtk.Application.Init();
		return true; // Successfully initialized
	}

	public static Task Delay()
	{
		return Task.Delay(100); // Wait for 100 milliseconds
	}

	public static void Shutdown()
	{
		Debug.Assert(isInitialized);
		Gtk.Application.Quit();
		isInitialized = false;
	}
}
