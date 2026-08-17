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

		// ViewModels como Singleton: persisten en memoria y no se recrean al navegar
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<PagoMovilViewModel>();
		builder.Services.AddSingleton<ComprasViewModel>();

		// Paginas como Singleton: el árbol visual se construye una sola vez en el arranque
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<PagoMovilPage>();
		builder.Services.AddSingleton<Compras>();

		// AppShell en DI para recibir las páginas pre-construidas
		builder.Services.AddSingleton<AppShell>();

		var app = builder.Build();

		// Pre-calentar SOLO los ViewModels en background para que los datos estén listos
		// antes de que el usuario haga clic en cualquier pestaña.
		// Las páginas (árbol visual) las construye Shell la primera vez que se navega a ellas.
		_ = Task.Run(async () =>
		{
			//await Task.Delay(400); // Dar tiempo al splash screen
			var mainVm = app.Services.GetRequiredService<MainViewModel>();
			var pagoMovilVm = app.Services.GetRequiredService<PagoMovilViewModel>();
			var comprasVm = app.Services.GetRequiredService<ComprasViewModel>();

			await InitializeDatabase(app.Services);
			
			await Task.WhenAll(
				mainVm.InitializeAsync(),
				pagoMovilVm.InitializeAsync(),
				comprasVm.InitializeAsync()
			);
		});

		return app;
	}

	private async static Task InitializeDatabase(IServiceProvider services)
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
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"ExchangeRates\" ADD COLUMN \"UsdtRate\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"ItemsJson\" TEXT NOT NULL DEFAULT '';"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"UsdRate\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"EurRate\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"UsdtRate\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
			try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"TotalUsdt\" REAL NOT NULL DEFAULT 0.0;"); } catch { }
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"Error al inicializar la base de datos en arranque: {ex.Message}");
		}
	}
}

