using CafePos.Data;
using CafePos.Services.Implementations;
using CafePos.Services.Interfaces;
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
            builder.Services.AddTransient<IDbInitializer, DbInitializer>();

            // Dependency Injection - ViewModels

            // Dependency Injection - Views / Pages

            return builder.Build();
        }
    }
}
