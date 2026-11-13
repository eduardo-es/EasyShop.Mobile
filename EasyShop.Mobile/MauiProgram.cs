using EasyShop.Mobile.Data;
using EasyShop.Mobile.Interfaces;
using EasyShop.Mobile.Services;
using EasyShop.Mobile.ViewModels;
using EasyShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasyShop.Mobile
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

            // Camada de dados (NoSQL local - LiteDB).
            // Singleton para manter uma única conexão compartilhada com products.db.
            builder.Services.AddSingleton<IProductRepository, ProductRepository>();

            // Camada de negócio.
            builder.Services.AddSingleton<IProductService, ProductService>();

            // Camada de apresentação (MVVM).
            builder.Services.AddTransient<ProductsViewModel>();
            builder.Services.AddTransient<ProductsPage>();

            // Shell da aplicação (uma instância por janela).
            builder.Services.AddTransient<AppShell>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
