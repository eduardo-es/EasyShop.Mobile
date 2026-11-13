using Microsoft.Extensions.DependencyInjection;

namespace EasyShop.Mobile
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            // InitializeComponent precisa ser executado antes de qualquer página ser
            // resolvida, pois os recursos globais (cores/estilos) vivem em App.xaml.
            InitializeComponent();

            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var appShell = _services.GetRequiredService<AppShell>();
            return new Window(appShell);
        }
    }
}
