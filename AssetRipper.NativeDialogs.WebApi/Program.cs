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

		app.MapGetFunction("/", () => Task.FromResult<string?>("Welcome to the AssetRipper Native Dialogs Web API!"));
		app.MapGetFunction("/open-file", OpenFileDialog.OpenFile);
		app.MapGetFunction("/open-files", OpenFileDialog.OpenFiles);
		app.MapGetFunction("/open-folder", OpenFolderDialog.OpenFolder);
		app.MapGetFunction("/open-folders", OpenFolderDialog.OpenFolders);
		app.MapGetFunction("/save-file", SaveFileDialog.SaveFile);
		app.MapGetFunction("/confirm", static async Task<string?> () =>
		{
			bool? result = await ConfirmationDialog.Confirm(new() { Message = "Do you acknowledge?", Type = ConfirmationDialog.Type.YesNo });
			return result switch
			{
				true => "User acknowledged.",
				false => "User did not acknowledge.",
				null => "User canceled the dialog."
			};
		});
		app.MapGetFunction("/message", static async Task<string?> () =>
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

	private static void MapGetFunction(this WebApplication app, [StringSyntax("Route")] string path, Func<Task<string?>> func)
	{
		app.MapGet(path, async (context) =>
		{
			context.Response.DisableCaching();
			await JsonResult(await func.Invoke()).ExecuteAsync(context);
		});
	}

	private static void MapGetFunction(this WebApplication app, [StringSyntax("Route")] string path, Func<Task<string[]?>> func)
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
