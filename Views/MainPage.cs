using Microsoft.Maui;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Components;
using BcvExchangeApp.Views.Components.Main;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views;

public class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = AppStyle.PageBackground;

        Shell.SetNavBarIsVisible(this, false);

        Content = new Grid
        {
            Children =
            {
                new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Spacing = 24,
                        Padding = new Thickness(24, 48, 24, 24),
                        Children =
                        {
                            new HeaderView
                            {
                                Title = "BCV Tasas de Cambio",
                                ActionText = "Actualizar",
                                ActionCommand = viewModel.FetchLatestRatesCommand
                            }
                            .Bind(HeaderView.SubtitleProperty, nameof(MainViewModel.FormattedDate), stringFormat: "Fecha Valor: {0}"),

                            new StatusBannerView()
                                .Bind(StatusBannerView.MessageProperty, nameof(MainViewModel.StatusMessage)),

                            new AutoFetchToggleView(),

                            new RatesCardView(),

                            new DatePickerCardView(),

                            new CurrencyConverterCardView(viewModel),

                            new RateHistoryTableView()
                        }
                    }
                }
                .Bind(ScrollView.OpacityProperty, nameof(MainViewModel.IsBusy), convert: (bool isBusy) => isBusy ? 0.35 : 1.0)
                .Bind(ScrollView.IsEnabledProperty, nameof(MainViewModel.IsBusy), convert: (bool isBusy) => !isBusy),

                new LoadingOverlayView()
                    .Bind(LoadingOverlayView.IsBusyProperty, nameof(MainViewModel.IsBusy))
                    .Bind(LoadingOverlayView.MessageProperty, nameof(MainViewModel.StatusMessage))
            }
        };
    }
}
