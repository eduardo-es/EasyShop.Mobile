using EasyShop.Mobile.Views;

namespace EasyShop.Mobile;

public partial class AppShell : Shell
{
    public AppShell(ProductsPage productsPage)
    {
        InitializeComponent();

        Items.Add(new ShellContent
        {
            Title = "Catálogo",
            Route = nameof(ProductsPage),
            Content = productsPage ?? throw new ArgumentNullException(nameof(productsPage))
        });
    }
}
