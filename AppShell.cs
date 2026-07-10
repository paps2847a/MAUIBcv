using BcvExchangeApp.Views;

namespace BcvExchangeApp;

public partial class AppShell : Shell
{
    // Las páginas se inyectan pre-construidas desde DI para evitar
    // que MAUI Shell las instancie en el hilo de UI durante la navegación.
    public AppShell(MainPage mainPage, PagoMovilPage pagoMovilPage, Compras compras)
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;

        Shell.SetTitleColor(this, Colors.Black);
        Shell.SetBackgroundColor(this, Colors.White);
        Shell.SetForegroundColor(this, Colors.Black);
        Shell.SetTabBarBackgroundColor(this, Colors.White);
        Shell.SetTabBarTitleColor(this, Colors.Black);
        Shell.SetTabBarUnselectedColor(this, Color.FromArgb("#94A3B8"));

        Items.Add(new FlyoutItem()
        {
            Title = "Pagina Principal",
            Items =
            {
                new ShellContent
                {
                    Content = mainPage,
                    Route = "mainpage"
                }
            }
        });

        Items.Add(new FlyoutItem()
        {
            Title = "Gestion de Pago Movil",
            Items =
            {
                new ShellContent()
                {
                    Content = pagoMovilPage,
                    Route = "pagomovil"
                }
            }
        });

        Items.Add(new FlyoutItem()
        {
            Title = "Compras Generales",
            Items =
            {
                new ShellContent()
                {
                    Content = compras,
                    Route = "compras"
                }
            }
        });
    }
}

