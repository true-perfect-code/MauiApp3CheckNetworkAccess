using Microsoft.Extensions.Logging;

namespace MauiApp3CheckNetworkAccess
{
    public static class Prop
    {
        public static bool InternetConnected = false;
        public static bool InternetConnected2 = false;
    }

    public static class MauiProgram
    {
        

        public static MauiApp CreateMauiApp()
        {
            Prop.InternetConnected = Connectivity.NetworkAccess == NetworkAccess.Internet;
            Task.Delay(5000);

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
