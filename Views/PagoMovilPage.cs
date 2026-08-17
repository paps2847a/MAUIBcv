using Microsoft.Maui;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Components;
using BcvExchangeApp.Views.Components.PagoMovil;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views;

public class PagoMovilPage : ContentPage
{
    public PagoMovilPage(PagoMovilViewModel viewModel)
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
                                Title = "Pago Móvil",
                                Subtitle = "Datos de Pago Móvil",
                                ActionText = "+ Agregar",
                                ActionCommand = new Command(async () => await Navigation.PushAsync(new PagoMovilFormPage(viewModel)))
                            },

                            new StatusBannerView()
                                .Bind(StatusBannerView.MessageProperty, nameof(PagoMovilViewModel.StatusMessage)),

                            new PagoMovilSearchBarView(),

                            new CollectionView
                            {
                                ItemTemplate = new DataTemplate(() => PagoMovilRecordCardView.CreateCard(viewModel)),
                                EmptyView = new Border
                                {
                                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                                    Stroke = AppStyle.BorderColor,
                                    StrokeThickness = 1,
                                    BackgroundColor = AppStyle.CardBackground,
                                    Content = new VerticalStackLayout
                                    {
                                        Spacing = 12,
                                        HorizontalOptions = LayoutOptions.Center,
                                        Children =
                                        {
                                            new Label()
                                                .Text("No hay registros guardados")
                                                .FontSize(15)
                                                .Bold()
                                                .TextColor(AppStyle.TextPrimary)
                                                .TextCenterHorizontal(),
                                            new Label { LineBreakMode = LineBreakMode.WordWrap }
                                                .Text("Pulse '+ Agregar' en la esquina superior derecha para registrar sus datos de Pago Móvil.")
                                                .FontSize(12)
                                                .TextColor(AppStyle.TextSecondary)
                                                .TextCenterHorizontal()
                                        }
                                    }
                                }
                                .Padding(new Thickness(24, 40))
                            }
                            .Bind(CollectionView.ItemsSourceProperty, nameof(PagoMovilViewModel.Records))
                        }
                    }
                }
                .Bind(ScrollView.OpacityProperty, nameof(PagoMovilViewModel.IsLoading), convert: (bool loading) => loading ? 0.35 : 1.0)
                .Bind(ScrollView.IsEnabledProperty, nameof(PagoMovilViewModel.IsLoading), convert: (bool loading) => !loading),

                new LoadingOverlayView()
                    .Bind(LoadingOverlayView.IsBusyProperty, nameof(PagoMovilViewModel.IsLoading))
            }
        };
    }
}
