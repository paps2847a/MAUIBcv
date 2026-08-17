using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Compras;

public static class PurchaseDetailItemsSection
{
    public static View CreateSection(PurchaseRecord record, List<ShoppingItem> items)
    {
        var layout = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label().Text("Detalle de Artículos").FontSize(14).Bold().TextColor(AppStyle.TextPrimary)
            }
        };

        if (items.Count == 0)
        {
            layout.Children.Add(new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Stroke = AppStyle.BorderColor,
                StrokeThickness = 1,
                BackgroundColor = AppStyle.CardBackground,
                Content = new Label()
                    .Text("No hay detalles individuales registrados para esta compra.")
                    .TextColor(AppStyle.TextSecondary)
                    .FontSize(12)
                    .TextCenterHorizontal()
            }
            .Padding(new Thickness(24, 30)));

            return layout;
        }

        foreach (var item in items)
        {
            layout.Children.Add(CreateItemDetailCard(record, item));
        }

        return layout;
    }

    private static View CreateItemDetailCard(PurchaseRecord record, ShoppingItem item)
    {
        double itemTotal = item.Price * item.Quantity;
        double vesVal = 0, usdVal = 0, eurVal = 0;
        double usdRate = record.UsdRate;
        double eurRate = record.EurRate;

        if (item.Currency == "VES")
        {
            vesVal = itemTotal;
            usdVal = usdRate > 0 ? itemTotal / usdRate : 0;
            eurVal = eurRate > 0 ? itemTotal / eurRate : 0;
        }
        else if (item.Currency == "USD")
        {
            vesVal = usdRate > 0 ? itemTotal * usdRate : 0;
            usdVal = itemTotal;
            eurVal = (usdRate > 0 && eurRate > 0) ? (itemTotal * usdRate) / eurRate : 0;
        }
        else if (item.Currency == "EUR")
        {
            vesVal = eurRate > 0 ? itemTotal * eurRate : 0;
            usdVal = (usdRate > 0 && eurRate > 0) ? (itemTotal * eurRate) / usdRate : 0;
            eurVal = itemTotal;
        }

        string symbol = item.Currency == "USD" ? "$" : item.Currency == "EUR" ? "€" : "Bs";

        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto),
                RowDefinitions = Rows.Define(Auto, Auto),
                RowSpacing = 8,
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        Children =
                        {
                            new Label { LineBreakMode = LineBreakMode.TailTruncation }
                                .Text(item.Name)
                                .Bold()
                                .TextColor(AppStyle.TextPrimary)
                                .FontSize(13),
                            new Label()
                                .Text($"{item.Quantity} x {item.Price:N2} {symbol}")
                                .TextColor(AppStyle.TextSecondary)
                                .FontSize(11)
                        }
                    }.Row(0).Column(0),

                    new Label()
                        .Text($"{itemTotal:N2} {symbol}")
                        .Bold()
                        .TextColor(AppStyle.TextPrimary)
                        .FontSize(13)
                        .CenterVertical()
                        .Row(0).Column(1),

                    new Border
                    {
                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                        BackgroundColor = AppStyle.InputBackground,
                        Content = new Grid
                        {
                            ColumnDefinitions = Columns.Define(Star, Star, Star),
                            Children =
                            {
                                CreateCurrencySubCell("VES (Bs)", $"{vesVal:N2}", 0),
                                CreateCurrencySubCell("USD ($)", $"{usdVal:N2}", 1),
                                CreateCurrencySubCell("EUR (€)", $"{eurVal:N2}", 2)
                            }
                        }
                    }
                    .Padding(new Thickness(10, 6))
                    .Row(1).ColumnSpan(2)
                }
            }
        }
        .Padding(new Thickness(14, 12));
    }

    private static VerticalStackLayout CreateCurrencySubCell(string labelText, string valText, int col)
    {
        return new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label().Text(labelText).FontSize(8).TextColor(AppStyle.TextSecondary).TextCenterHorizontal(),
                new Label().Text(valText).FontSize(11).Bold().TextColor(AppStyle.TextPrimary)
            }
        }.Column(col);
    }
}
