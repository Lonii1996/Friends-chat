using ChatClient.Handlers;
using Microsoft.Extensions.Logging;

namespace ChatClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            
            builder.Services.AddTransient<AuthHttpMessageHandler>();

            
            builder.Services.AddSingleton(sp =>
            {
                var handler = sp.GetRequiredService<AuthHttpMessageHandler>();

                
                var httpClient = new HttpClient(handler)
                {
                    BaseAddress = new Uri(DeviceInfo.Platform == DevicePlatform.Android
                        ? "http://10.0.2.2:8080"
                        : "http://localhost:8080")
                };

                return httpClient;
            });




            return builder.Build();
        }
    }
}
