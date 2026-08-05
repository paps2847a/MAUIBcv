using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Compras;

public class PurchaseHistoryCardView : Grid
{
    public PurchaseHistoryCardView(ComprasViewModel viewModel, Page page)
    {
        RowDefinitions = Rows.Define(Star, Auto);
        RowSpacing = 8;

        var prevBtn = new Button
        {
            CornerRadius = 6,
            HeightRequest = 36,
            Command = viewModel.PreviousHistoryPageCommand
        }
        .Text("◀ Anterior")
        .FontSize(12)
        .Bold()
        .TextColor(AppStyle.White)
        .Bind(Button.IsEnabledProperty, nameof(ComprasViewModel.HasPreviousHistoryPage))
        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.HasPreviousHistoryPage),
            convert: (bool enabled) => enabled ? AppStyle.Slate900 : AppStyle.Slate300)
        .Column(0);

        prevBtn.Clicked += async (s, e) => await AppStyle.AnimateClickAsync(prevBtn);

        var nextBtn = new Button
        {
            CornerRadius = 6,
            HeightRequest = 36,
            Command = viewModel.NextHistoryPageCommand
        }
        .Text("Siguiente ▶")
        .FontSize(12)
        .Bold()
        .TextColor(AppStyle.White)
        .Bind(Button.IsEnabledProperty, nameof(ComprasViewModel.HasNextHistoryPage))
        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.HasNextHistoryPage),
            convert: (bool enabled) => enabled ? AppStyle.Slate900 : AppStyle.Slate300)
        .Column(2);

        nextBtn.Clicked += async (s, e) => await AppStyle.AnimateClickAsync(nextBtn);

        var collectionView = new CollectionView
        {
            Header = new VerticalStackLayout
            {
                Margin = new Thickness(0, 0, 0, 12),
                Children =
                {
                    new Label()
                        .Text("Historial de Compras")
                        .FontSize(14)
                        .Bold()
                        .TextColor(AppStyle.TextPrimary)
                }
            },
            ItemTemplate = new DataTemplate(() => CreateHistoryCard(viewModel, page)),
            EmptyView = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
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
                            .Text("No hay compras registradas")
                            .FontSize(15)
                            .Bold()
                            .TextColor(AppStyle.TextPrimary)
                            .TextCenterHorizontal(),
                        new Label { LineBreakMode = LineBreakMode.WordWrap }
                            .Text("Tus compras completadas aparecerán aquí una vez que registres tu lista de compras activa.")
                            .FontSize(12)
                            .TextColor(AppStyle.TextSecondary)
                            .TextCenterHorizontal()
                    }
                }
            }
            .Padding(new Thickness(24, 40))
        }
        .Row(0)
        .Bind(CollectionView.ItemsSourceProperty, nameof(ComprasViewModel.PurchaseHistory));

        var paginationGrid = new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star, Auto),
            Padding = new Thickness(0, 8),
            Children =
            {
                prevBtn,
                new Label()
                    .TextColor(AppStyle.TextPrimary)
                    .Bold()
                    .FontSize(13)
                    .TextCenterHorizontal()
                    .CenterVertical()
                    .Bind(Label.TextProperty, nameof(ComprasViewModel.HistoryPageText))
                    .Column(1),
                nextBtn
            }
        }
        .Row(1)
        .Bind(Grid.IsVisibleProperty, nameof(ComprasViewModel.TotalHistoryPages), convert: (int totalPages) => totalPages > 1);

        Children.Add(collectionView);
        Children.Add(paginationGrid);
    }

    private View CreateHistoryCard(ComprasViewModel viewModel, Page page)
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 4,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label()
                                .Bold()
                                .TextColor(AppStyle.TextPrimary)
                                .FontSize(12)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.PurchaseDate),
                                    convert: (DateTime dt) => dt.ToString("dd/MM/yyyy - hh:mm tt")),

                            new Label { LineBreakMode = LineBreakMode.WordWrap }
                                .TextColor(AppStyle.TextSecondary)
                                .FontSize(11)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.ItemSummary))
                        }
                    }.Column(0),

                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        Children =
                        {
                            new Label().Bold().TextColor(AppStyle.TextPrimary).FontSize(12)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.TotalVes), stringFormat: "{0:N2} Bs"),
                            new Label().TextColor(AppStyle.TextSecondary).FontSize(11)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.TotalUsd), stringFormat: "${0:N2}"),
                            new Label().TextColor(AppStyle.Slate400).FontSize(10)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.TotalEur), stringFormat: "€{0:N2}")
                        }
                    }
                    .Column(1)
                    .Margin(new Thickness(0, 0, 8, 0)),

                    new Button
                    {
                        TextColor = AppStyle.DangerRed,
                        BackgroundColor = AppStyle.Transparent,
                        HeightRequest = 32,
                        WidthRequest = 32,
                        Padding = 0
                    }
                    .Text("✕")
                    .Bold()
                    .FontSize(13)
                    .Invoke(btn => btn.Clicked += async (s, e) =>
                    {
                        if (btn.BindingContext is PurchaseRecord rec)
                        {
                            bool confirm = await page.DisplayAlertAsync("Eliminar Compra", "¿Desea eliminar esta compra del historial?", "Eliminar", "Cancelar");
                            if (confirm)
                            {
                                viewModel.DeletePurchaseCommand.Execute(rec);
                            }
                        }
                    })
                    .Column(2)
                    .CenterVertical()
                }
            }
        }
        .Padding(new Thickness(14, 12))
        .Margin(new Thickness(0, 0, 0, 10));

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            if (border.BindingContext is PurchaseRecord rec)
            {
                await page.Navigation.PushAsync(new PurchaseDetailPage(rec));
            }
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }
}
