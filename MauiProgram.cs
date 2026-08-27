using CafePos.Data;
using Microsoft.Extensions.Logging;

namespace CafePos
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

            // Dependency Injection - Data Context
            builder.Services.AddDbContext<AppDbContext>();

            // Dependency Injection - Services

            // Dependency Injection - ViewModels

            // Dependency Injection - Views / Pages

            return builder.Build();
        }
    }
}
