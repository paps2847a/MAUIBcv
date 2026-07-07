using BcvExchangeApp.Views;

namespace BcvExchangeApp;

public partial class AppShell : Shell
{
	public AppShell()
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
					ContentTemplate = new DataTemplate(typeof(MainPage)),
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
					ContentTemplate = new DataTemplate(typeof(PagoMovilPage)),
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
					ContentTemplate = new DataTemplate(typeof(Compras)),
					Route = "compras"
				}
			}
		});
	}
}
