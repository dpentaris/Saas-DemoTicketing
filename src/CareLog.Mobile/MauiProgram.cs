using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CareLog.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"));

        builder.Services.AddSingleton<Services.ApiClient>();
        builder.Services.AddSingleton<Data.LocalStore>();
        builder.Services.AddSingleton<Services.SyncService>();
        builder.Services.AddTransient<ViewModels.TodayViewModel>();
        builder.Services.AddTransient<Views.TodayPage>();
        return builder.Build();
    }
}
