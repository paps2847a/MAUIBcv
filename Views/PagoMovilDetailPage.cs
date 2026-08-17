using Microsoft.Maui;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Components;
using BcvExchangeApp.Views.Components.PagoMovil;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views;

public class PagoMovilDetailPage : ContentPage
{
    public PagoMovilDetailPage(PagoMovilRecord record, PagoMovilViewModel viewModel)
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
                        Title = "Detalles",
                        Subtitle = "Información de Pago Móvil",
                        IsBackButton = true
                    },

                    new StatusBannerView()
                        .Bind(StatusBannerView.MessageProperty, nameof(PagoMovilViewModel.StatusMessage)),

                    new PagoMovilDetailsCardView(record, viewModel, this)
                }
            }
        };
    }
}
