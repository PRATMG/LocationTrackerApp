using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using LocationTrackerApp;               // App, MainPage
using LocationTrackerApp.Services;      // LocationDatabase


namespace LocationTrackerApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// --- SQLite service registration ---
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
		builder.Services.AddSingleton(new LocationDatabase(dbPath));

		// --- Pages via DI ---
		builder.Services.AddSingleton<MainPage>();

		var app = builder.Build();

		// Ensure DB table exists (fire-and-forget)
		var db = app.Services.GetRequiredService<LocationDatabase>();
		_ = db.InitializeAsync();

		return app;
	}
}
