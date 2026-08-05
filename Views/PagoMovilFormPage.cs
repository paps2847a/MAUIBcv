using Microsoft.Maui;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Components;
using BcvExchangeApp.Views.Components.PagoMovil;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views;

public class PagoMovilFormPage : ContentPage
{
    public PagoMovilFormPage(PagoMovilViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = AppStyle.PageBackground;

        Shell.SetNavBarIsVisible(this, false);

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 24,
                Padding = new Thickness(24, 48, 24, 24),
                Children =
                {
                    new HeaderView
                    {
                        Title = "Registro",
                        Subtitle = "Nuevo Pago Móvil",
                        IsBackButton = true
                    },

                    new PagoMovilFormCardView(viewModel, Navigation)
                }
            }
        };
    }
}
