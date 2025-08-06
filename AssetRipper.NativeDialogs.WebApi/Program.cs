using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.NativeDialogs.WebApi;

public static class Program
{
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

		builder.Services.ConfigureHttpJsonOptions(options =>
		{
			options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
		});

		WebApplication app = builder.Build();

		app.MapGet("/", () => Task.FromResult<string?>("Welcome to the AssetRipper Native Dialogs Web API!"));
		app.MapGet("/open-file", OpenFileDialog.OpenFile);
		app.MapGet("/open-files", OpenFileDialog.OpenFiles);
		app.MapGet("/open-folder", OpenFolderDialog.OpenFolder);
		app.MapGet("/open-folders", OpenFolderDialog.OpenFolders);
		app.MapGet("/save-file", SaveFileDialog.SaveFile);
		app.MapGet("/confirm", static async Task<string?> () =>
		{
			bool? result = await ConfirmationDialog.Confirm(new() { Message = "Do you acknowledge?", Type = ConfirmationDialog.Type.YesNo });
			return result switch
			{
				true => "User acknowledged.",
				false => "User did not acknowledge.",
				null => "User canceled the dialog."
			};
		});
		app.MapGet("/message", static async Task<string?> () =>
		{
			await MessageDialog.Message("Hello, World!");
			return "Message dialog shown.";
		});

		app.Run();
	}

	private static void DisableCaching(this HttpResponse response)
	{
		response.Headers.CacheControl = "no-store, max-age=0";
	}

	private static void MapGet(this WebApplication app, [StringSyntax("Route")] string path, Func<Task<string?>> func)
	{
		app.MapGet(path, async (context) =>
		{
			context.Response.DisableCaching();
			await JsonResult(await func.Invoke()).ExecuteAsync(context);
		});
	}

	private static void MapGet(this WebApplication app, [StringSyntax("Route")] string path, Func<Task<string[]?>> func)
	{
		app.MapGet(path, async (context) =>
		{
			context.Response.DisableCaching();
			await JsonResult(await func.Invoke()).ExecuteAsync(context);
		});
	}

	private static IResult JsonResult<T>(T? value) where T : class
	{
		if (value is null)
		{
			return Results.Text("null", "text/json");
		}
		else
		{
			return Results.Json(value, AppJsonSerializerContext.Default);
		}
	}
}
