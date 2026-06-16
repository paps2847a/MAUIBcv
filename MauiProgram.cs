using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Data;
using BcvExchangeApp.Services;
using BcvExchangeApp.Store;

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

		// Registro de base de datos, servicios, Store y Vista
		builder.Services.AddDbContext<BcvDbContext>();
		builder.Services.AddSingleton<BcvScraperService>();
		builder.Services.AddSingleton<BcvStore>();
		builder.Services.AddSingleton<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

