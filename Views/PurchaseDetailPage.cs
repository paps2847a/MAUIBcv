using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.Views.Components;
using BcvExchangeApp.Views.Components.Compras;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views;

public class PurchaseDetailPage : ContentPage
{
    public PurchaseDetailPage(PurchaseRecord record)
    {
        BackgroundColor = AppStyle.PageBackground;
        Shell.SetNavBarIsVisible(this, false);

        List<ShoppingItem> items = new();
        try
        {
            if (!string.IsNullOrWhiteSpace(record.ItemsJson))
            {
                var list = JsonSerializer.Deserialize<List<ShoppingItem>>(record.ItemsJson);
                if (list != null) items = list;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al deserializar items de compra: {ex.Message}");
        }

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
                        Title = "Detalle de Compra",
                        Subtitle = record.PurchaseDate.ToString("dd 'de' MMMM, yyyy - hh:mm tt", new CultureInfo("es-ES")),
                        IsBackButton = true
                    },

                    CreateTotalsCard(record),

                    PurchaseDetailItemsSection.CreateSection(record, items)
                }
            }
        };
    }

    private View CreateTotalsCard(PurchaseRecord record)
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Padding = 16,
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new Label().Text("Resumen de Totales Pagados").FontSize(14).Bold().TextColor(AppStyle.TextPrimary),
                    
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Star, Star),
                        ColumnSpacing = 8,
                        Children =
                        {
                            CreateTotalCell("TOTAL VES", $"{record.TotalVes:N2} Bs", 0),
                            CreateTotalCell("TOTAL USD", $"${record.TotalUsd:N2}", 1),
                            CreateTotalCell("TOTAL EUR", $"€{record.TotalEur:N2}", 2)
                        }
                    },

                    new BoxView { HeightRequest = 1, BackgroundColor = AppStyle.BorderColor },

                    new HorizontalStackLayout
                    {
                        Spacing = 12,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label().Text($"Tasa USD: {record.UsdRate:N4} Bs").FontSize(11).TextColor(AppStyle.TextSecondary).Italic(),
                            new Label().Text("|").FontSize(11).TextColor(AppStyle.Slate300),
                            new Label().Text($"Tasa EUR: {record.EurRate:N4} Bs").FontSize(11).TextColor(AppStyle.TextSecondary).Italic()
                        }
                    }
                }
            }
        };
    }

    private VerticalStackLayout CreateTotalCell(string title, string valueStr, int col)
    {
        return new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label().Text(title).FontSize(9).Bold().TextColor(AppStyle.TextSecondary),
                new Label().Text(valueStr).FontSize(15).Bold().TextColor(AppStyle.TextPrimary)
            }
        }.Column(col);
    }
}
