namespace AssetRipper.NativeDialogs;

public sealed class OpenFileDialogOptions
{
	internal static OpenFileDialogOptions Default { get; } = new();

	public IReadOnlyList<KeyValuePair<string, string>> Filters { get; init; } = [];
}
