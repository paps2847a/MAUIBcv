using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Components;
using BcvExchangeApp.Views.Components.Compras;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views;

public class Compras : ContentPage
{
    private Border? _totalsCard;

    public Compras(ComprasViewModel viewModel)
    {
        BindingContext = viewModel;
        BackgroundColor = AppStyle.PageBackground;

        Shell.SetNavBarIsVisible(this, false);

        _totalsCard = new TotalsSummaryCardView(viewModel, this);

        Content = new Grid
        {
            RowDefinitions = Rows.Define(Auto, Star),
            Padding = new Thickness(24, 48, 24, 24),
            RowSpacing = 16,
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 16,
                    Children =
                    {
                        new HeaderView
                        {
                            Title = "Gestor de Compras",
                            Subtitle = "Cálculo y control de gastos cotidianos"
                        },
                        new StatusBannerView()
                            .Bind(StatusBannerView.MessageProperty, nameof(ComprasViewModel.StatusMessage)),
                        new ComprasTabSelectorView(viewModel)
                    }
                }
                .Row(0),

                new Grid
                {
                    Children =
                    {
                        new CollectionView
                        {
                            Header = new VerticalStackLayout
                            {
                                Spacing = 20,
                                Margin = new Thickness(0, 0, 0, 16),
                                Children =
                                {
                                    new AddProductCardView(viewModel, async () =>
                                    {
                                        if (_totalsCard != null)
                                        {
                                            await Task.Delay(200);
                                            await AppStyle.FlashCardAsync(_totalsCard);
                                        }
                                    }),
                                    _totalsCard,
                                    new Label()
                                        .Text("Productos en la Lista")
                                        .FontSize(14)
                                        .Bold()
                                        .TextColor(AppStyle.TextPrimary)
                                        .Margin(new Thickness(0, 4, 0, 0))
                                }
                            },
                            ItemTemplate = new DataTemplate(() => ShoppingItemCardView.CreateCard(viewModel, async () =>
                            {
                                if (_totalsCard != null)
                                {
                                    await Task.Delay(200);
                                    await AppStyle.FlashCardAsync(_totalsCard);
                                }
                            })),
                            EmptyView = new Border
                            {
                                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                                Stroke = AppStyle.BorderColor,
                                StrokeThickness = 1,
                                BackgroundColor = AppStyle.CardBackground,
                                Content = new Label()
                                    .Text("No hay productos en la lista activa.")
                                    .TextColor(AppStyle.TextSecondary)
                                    .FontSize(12)
                                    .TextCenterHorizontal()
                            }
                            .Padding(new Thickness(24, 30))
                        }
                        .Bind(CollectionView.ItemsSourceProperty, nameof(ComprasViewModel.ShoppingItems))
                        .Bind(View.IsVisibleProperty, nameof(ComprasViewModel.ActiveTab), convert: (string? tab) => tab == "List"),

                        new PurchaseHistoryCardView(viewModel, this)
                            .Bind(View.IsVisibleProperty, nameof(ComprasViewModel.ActiveTab), convert: (string? tab) => tab == "History")
                    }
                }
                .Row(1)
                .Bind(Grid.OpacityProperty, nameof(ComprasViewModel.IsLoading), convert: (bool loading) => loading ? 0.35 : 1.0)
                .Bind(Grid.IsEnabledProperty, nameof(ComprasViewModel.IsLoading), convert: (bool loading) => !loading),

                new LoadingOverlayView()
                    .Bind(LoadingOverlayView.IsBusyProperty, nameof(ComprasViewModel.IsLoading))
            }
        };
    }
}