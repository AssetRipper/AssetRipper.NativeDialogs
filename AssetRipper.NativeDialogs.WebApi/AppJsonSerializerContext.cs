using System.Text.Json.Serialization;

namespace AssetRipper.NativeDialogs.WebApi;

[JsonSerializable(typeof(string[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
