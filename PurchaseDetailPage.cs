using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp;

public class PurchaseDetailPage : ContentPage
{
    private readonly PurchaseRecord _record;
    private List<ShoppingItem> _items = new();

    public PurchaseDetailPage(PurchaseRecord record)
    {
        _record = record;
        BackgroundColor = Color.FromArgb("#F8FAFC"); // slate-50
        Shell.SetNavBarIsVisible(this, false);

        // Deserializar productos
        try
        {
            if (!string.IsNullOrWhiteSpace(_record.ItemsJson))
            {
                var list = JsonSerializer.Deserialize<List<ShoppingItem>>(_record.ItemsJson);
                if (list != null)
                {
                    _items = list;
                }
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
                    CreateHeader(),
                    CreateTotalsCard(),
                    CreateItemsListSection()
                }
            }
        };
    }

    private View CreateHeader()
    {
        return new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star),
            ColumnSpacing = 12,
            Children =
            {
                new Button
                {
                    Text = "←",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#0F172A"),
                    BackgroundColor = Colors.Transparent,
                    BorderWidth = 0,
                    Padding = 0,
                    HeightRequest = 40,
                    WidthRequest = 40
                }
                .Invoke(btn => btn.Clicked += async (s, e) => 
                {
                    // Animación táctil
                    await btn.ScaleToAsync(0.92, 70, Easing.CubicOut);
                    await btn.ScaleToAsync(1.0, 70, Easing.CubicIn);
                    await Navigation.PopAsync();
                })
                .Column(0)
                .CenterVertical(),

                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = "Detalle de Compra",
                            FontSize = 22,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A")
                        },
                        new Label
                        {
                            Text = _record.PurchaseDate.ToString("dd 'de' MMMM, yyyy - hh:mm tt", new System.Globalization.CultureInfo("es-ES")),
                            FontSize = 13,
                            TextColor = Color.FromArgb("#64748B")
                        }
                    }
                }
                .Column(1)
                .CenterVertical()
            }
        };
    }

    private View CreateTotalsCard()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = 16,
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new Label { Text = "Resumen de Totales Pagados", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") },
                    
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Star, Star),
                        ColumnSpacing = 8,
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 2,
                                HorizontalOptions = LayoutOptions.Center,
                                Children =
                                {
                                    new Label { Text = "TOTAL VES", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") },
                                    new Label { Text = $"{_record.TotalVes:N2} Bs", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                }
                            }
                            .Column(0),

                            new VerticalStackLayout
                            {
                                Spacing = 2,
                                HorizontalOptions = LayoutOptions.Center,
                                Children =
                                {
                                    new Label { Text = "TOTAL USD", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") },
                                    new Label { Text = $"${_record.TotalUsd:N2}", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                }
                            }
                            .Column(1),

                            new VerticalStackLayout
                            {
                                Spacing = 2,
                                HorizontalOptions = LayoutOptions.Center,
                                Children =
                                {
                                    new Label { Text = "TOTAL EUR", FontSize = 9, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") },
                                    new Label { Text = $"€{_record.TotalEur:N2}", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                }
                            }
                            .Column(2)
                        }
                    },

                    new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E2E8F0") },

                    // Tasas de cambio aplicadas
                    new HorizontalStackLayout
                    {
                        Spacing = 12,
                        HorizontalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label { Text = $"Tasa USD: {_record.UsdRate:N4} Bs", FontSize = 11, TextColor = Color.FromArgb("#64748B"), FontAttributes = FontAttributes.Italic },
                            new Label { Text = "|", FontSize = 11, TextColor = Color.FromArgb("#CBD5E1") },
                            new Label { Text = $"Tasa EUR: {_record.EurRate:N4} Bs", FontSize = 11, TextColor = Color.FromArgb("#64748B"), FontAttributes = FontAttributes.Italic }
                        }
                    }
                }
            }
        };
    }

    private View CreateItemsListSection()
    {
        var layout = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label { Text = "Detalle de Artículos", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
            }
        };

        if (_items.Count == 0)
        {
            layout.Children.Add(new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Padding = new Thickness(24, 30),
                Content = new Label
                {
                    Text = "No hay detalles individuales registrados para esta compra.",
                    TextColor = Color.FromArgb("#64748B"),
                    FontSize = 12,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            });
            return layout;
        }

        foreach (var item in _items)
        {
            layout.Children.Add(CreateItemDetailCard(item));
        }

        return layout;
    }

    private View CreateItemDetailCard(ShoppingItem item)
    {
        double itemTotal = item.Price * item.Quantity;
        double vesVal = 0;
        double usdVal = 0;
        double eurVal = 0;

        double usdRate = _record.UsdRate;
        double eurRate = _record.EurRate;

        // Calcular conversiones para el item basados en la tasa histórica de la compra
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
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = new Thickness(14, 12),
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto),
                RowDefinitions = Rows.Define(Auto, Auto),
                RowSpacing = 8,
                Children =
                {
                    // Fila 0 Izquierda: Nombre y cantidad/precio unitario
                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        Children =
                        {
                            new Label
                            {
                                Text = item.Name,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#0F172A"),
                                FontSize = 13,
                                LineBreakMode = LineBreakMode.TailTruncation
                            },
                            new Label
                            {
                                Text = $"{item.Quantity} x {item.Price:N2} {symbol}",
                                TextColor = Color.FromArgb("#64748B"),
                                FontSize = 11
                            }
                        }
                    }
                    .Row(0).Column(0),

                    // Fila 0 Derecha: Total original
                    new Label
                    {
                        Text = $"{itemTotal:N2} {symbol}",
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#0F172A"),
                        FontSize = 13,
                        VerticalOptions = LayoutOptions.Center
                    }
                    .Row(0).Column(1),

                    // Fila 1 (Span completo): Conversiones en las monedas principales
                    new Border
                    {
                        StrokeShape = new RoundRectangle { CornerRadius = 4 },
                        BackgroundColor = Color.FromArgb("#F8FAFC"), // slate-50
                        Padding = new Thickness(10, 6),
                        Content = new Grid
                        {
                            ColumnDefinitions = Columns.Define(Star, Star, Star),
                            Children =
                            {
                                new VerticalStackLayout
                                {
                                    HorizontalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        new Label { Text = "VES (Bs)", FontSize = 8, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center },
                                        new Label { Text = $"{vesVal:N2}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                    }
                                }.Column(0),

                                new VerticalStackLayout
                                {
                                    HorizontalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        new Label { Text = "USD ($)", FontSize = 8, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center },
                                        new Label { Text = $"{usdVal:N2}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                    }
                                }.Column(1),

                                new VerticalStackLayout
                                {
                                    HorizontalOptions = LayoutOptions.Center,
                                    Children =
                                    {
                                        new Label { Text = "EUR (€)", FontSize = 8, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center },
                                        new Label { Text = $"{eurVal:N2}", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                    }
                                }.Column(2)
                            }
                        }
                    }
                    .Row(1).ColumnSpan(2)
                }
            }
        };
    }
}
