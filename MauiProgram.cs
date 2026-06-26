using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Data;
using BcvExchangeApp.Services;
using BcvExchangeApp.ViewModels;

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
		builder.Services.AddSingleton<PagoMovilPage>();
		builder.Services.AddTransient<PagoMovilFormPage>();
		builder.Services.AddTransient<PagoMovilDetailPage>();
		builder.Services.AddSingleton<ComprasViewModel>();
		builder.Services.AddSingleton<Compras>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

