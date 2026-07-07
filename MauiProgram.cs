using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Data;
using BcvExchangeApp.Services;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views;

namespace BcvExchangeApp;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkitMarkup()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Registro de base de datos, servicios, ViewModel y Vista
		builder.Services.AddDbContext<BcvDbContext>();
		
		builder.Services.AddSingleton<BcvScraperService>();

		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<PagoMovilViewModel>();
		builder.Services.AddTransient<PagoMovilPage>();
		builder.Services.AddSingleton<ComprasViewModel>();
		builder.Services.AddSingleton<Compras>();

		var app = builder.Build();
		
		// Inicializar la base de datos en segundo plano para no bloquear el arranque de la app
		Task.Run(() => InitializeDatabase(app.Services));

		return app;
	}

	private static void InitializeDatabase(IServiceProvider services)
	{
		try
		{
			using var scope = services.CreateScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<BcvDbContext>();
			
			// 1. Asegurar creación física de base de datos
			dbContext.Database.EnsureCreated();
			
			// 2. Crear tablas de forma explícita si no existieran
			dbContext.Database.ExecuteSqlRaw(
				"CREATE TABLE IF NOT EXISTS \"PagoMovilRecords\" (" +
				"\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
				"\"Cedula\" TEXT NOT NULL, " +
				"\"Phone\" TEXT NOT NULL, " +
				"\"BankCode\" TEXT NOT NULL, " +
				"\"BankName\" TEXT NOT NULL, " +
				"\"CreatedAt\" TEXT NOT NULL" +
				");"
			);

			dbContext.Database.ExecuteSqlRaw(
				"CREATE TABLE IF NOT EXISTS \"ShoppingItems\" (" +
				"\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
				"\"Name\" TEXT NOT NULL, " +
				"\"Price\" REAL NOT NULL, " +
				"\"Currency\" TEXT NOT NULL, " +
				"\"Quantity\" INTEGER NOT NULL, " +
				"\"CreatedAt\" TEXT NOT NULL" +
				");"
			);

			dbContext.Database.ExecuteSqlRaw(
				"CREATE TABLE IF NOT EXISTS \"PurchaseRecords\" (" +
				"\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
				"\"PurchaseDate\" TEXT NOT NULL, " +
				"\"TotalVes\" REAL NOT NULL, " +
				"\"TotalUsd\" REAL NOT NULL, " +
				"\"TotalEur\" REAL NOT NULL, " +
				"\"ItemSummary\" TEXT NOT NULL, " +
				"\"ItemsJson\" TEXT NOT NULL DEFAULT '', " +
				"\"UsdRate\" REAL NOT NULL DEFAULT 0.0, " +
				"\"EurRate\" REAL NOT NULL DEFAULT 0.0, " +
				"\"CreatedAt\" TEXT NOT NULL" +
				");"
			);

			dbContext.Database.ExecuteSqlRaw(
				"CREATE INDEX IF NOT EXISTS \"IX_PurchaseRecords_PurchaseDate\" ON \"PurchaseRecords\" (\"PurchaseDate\");"
			);

			// Asegurar que las columnas agregadas existan en la base de datos local
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"ItemsJson\" TEXT NOT NULL DEFAULT '';"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"UsdRate\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"EurRate\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error al inicializar la base de datos en arranque: {ex.Message}");
		}
	}
}

