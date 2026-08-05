using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Main;

public class RateHistoryTableView : VerticalStackLayout
{
    public RateHistoryTableView()
    {
        Spacing = 12;

        Children.Add(new Label()
            .Text("Historial reciente")
            .FontSize(14)
            .Bold()
            .TextColor(AppStyle.TextPrimary)
            .Margin(new Thickness(0, 8, 0, 0)));

        var tableBorder = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Grid
                    {
                        BackgroundColor = AppStyle.InputBackground,
                        Padding = new Thickness(10, 10),
                        ColumnDefinitions = Columns.Define(80, Star, Star, Star),
                        Children =
                        {
                            new Label().Text("Fecha").Bold().TextColor(AppStyle.TextPrimary).FontSize(11).Column(0),
                            new Label().Text("USD").Bold().TextColor(AppStyle.TextPrimary).FontSize(11).TextEnd().Column(1),
                            new Label().Text("EUR").Bold().TextColor(AppStyle.TextPrimary).FontSize(11).TextEnd().Column(2),
                            new Label().Text("USDT").Bold().TextColor(AppStyle.TextPrimary).FontSize(11).TextEnd().Column(3)
                        }
                    },

                    new CollectionView
                    {
                        HeightRequest = 220,
                        ItemTemplate = new DataTemplate(() => new Grid
                        {
                            Padding = new Thickness(10, 10),
                            ColumnDefinitions = Columns.Define(80, Star, Star, Star),
                            Children =
                            {
                                new Label { TextColor = AppStyle.TextSecondary, FontSize = 11 }
                                    .Bind(Label.TextProperty, nameof(ExchangeRate.Date), 
                                        convert: (DateTime date) => date.ToString("dd/MM/yy"))
                                    .Column(0),

                                new Label { TextColor = AppStyle.TextPrimary, FontSize = 11 }
                                    .TextEnd()
                                    .Bind(Label.TextProperty, nameof(ExchangeRate.UsdRate), stringFormat: "{0:N2}")
                                    .Column(1),

                                new Label { TextColor = AppStyle.TextPrimary, FontSize = 11 }
                                    .TextEnd()
                                    .Bind(Label.TextProperty, nameof(ExchangeRate.EurRate), stringFormat: "{0:N2}")
                                    .Column(2),

                                new Label { TextColor = AppStyle.TextPrimary, FontSize = 11 }
                                    .TextEnd()
                                    .Bind(Label.TextProperty, nameof(ExchangeRate.UsdtRate), stringFormat: "{0:N2}")
                                    .Column(3)
                            }
                        })
                    }
                    .Bind(CollectionView.ItemsSourceProperty, nameof(MainViewModel.History))
                }
            }
        };

        Children.Add(tableBorder);
    }
}
