using Microsoft.Extensions.Logging;
using Plugin.Maui.Audio;
using SpotiClone.Services;
using SpotiClone.ViewModels;
using SpotiClone.Views;

namespace SpotiClone;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.AddAudio()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Services
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<IApiService, ApiService>();
		builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
		builder.Services.AddSingleton<IPlayerService, PlayerService>();

		// ViewModels
		builder.Services.AddSingleton<PlayerViewModel>();
		builder.Services.AddSingleton<MiniPlayerViewModel>();
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<SearchViewModel>();
		builder.Services.AddTransient<LibraryViewModel>();
		builder.Services.AddTransient<HistoryViewModel>();

		// Views
		builder.Services.AddTransient<HomePage>();
		builder.Services.AddTransient<SearchPage>();
		builder.Services.AddTransient<LibraryPage>();
		builder.Services.AddTransient<PlayerPage>();
		builder.Services.AddTransient<HistoryPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
