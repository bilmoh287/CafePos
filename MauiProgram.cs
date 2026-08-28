using CafePos.Data;
using CafePos.Services.Implementations;
using CafePos.Services.Interfaces;
using CafePos.ViewModels;
using CafePos.Views;
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
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddSingleton<ICartService, CartService>();
            builder.Services.AddTransient<IOrderService, OrderService>();

            // Dependency Injection - ViewModels
            builder.Services.AddTransient<ProductsViewModel>();
            builder.Services.AddTransient<CartViewModel>();
            builder.Services.AddTransient<CheckoutViewModel>();

            // Dependency Injection - Views / Pages / Modals
            builder.Services.AddTransient<ProductsPage>();
            builder.Services.AddTransient<CartPage>();
            builder.Services.AddTransient<CheckoutModal>();

            return builder.Build();
        }
    }
}
