using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Views;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp;

public partial class AppShell : Shell
{
    public AppShell(MainPage mainPage, PagoMovilPage pagoMovilPage, Compras compras)
    {
        FlyoutBehavior = FlyoutBehavior.Flyout;
        FlyoutWidth = 290;
        FlyoutBackgroundColor = Colors.White;

        // Configuración de colores del Shell NavBar y TabBar
        Shell.SetTitleColor(this, AppStyle.Slate900);
        Shell.SetBackgroundColor(this, AppStyle.Slate50);
        Shell.SetForegroundColor(this, AppStyle.Slate900);
        Shell.SetTabBarBackgroundColor(this, Colors.White);
        Shell.SetTabBarTitleColor(this, AppStyle.Slate900);
        Shell.SetTabBarUnselectedColor(this, AppStyle.Slate500);

        // Encabezado del Menú Lateral (Flyout Header)
        FlyoutHeader = new Border
        {
            BackgroundColor = AppStyle.Slate400,
            Padding = new Thickness(24, 52, 24, 24),
            StrokeThickness = 0,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            new Label().Text("🇻🇪").FontSize(24).CenterVertical(),
                            new VerticalStackLayout
                            {
                                Spacing = 2,
                                Children =
                                {
                                    new Label()
                                        .Text("BCV Monitor & Pagos")
                                        .FontSize(17)
                                        .Bold()
                                        .TextColor(Colors.White),
                                    new Label()
                                        .Text("Vida Diaria en Venezuela")
                                        .FontSize(11)
                                        .TextColor(Color.FromArgb("#CBD5E1"))
                                }
                            }
                        }
                    },
                    new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#334155"), Margin = new Thickness(0, 10, 0, 0) }
                }
            }
        };

        // Pie de página del Menú Lateral (Flyout Footer)
        FlyoutFooter = new VerticalStackLayout
        {
            Padding = new Thickness(20, 16),
            Children =
            {
                new Label()
                    .Text("v1.2 • C# 14 / .NET 10 MAUI")
                    .FontSize(11)
                    .TextColor(AppStyle.Slate500)
                    .TextCenterHorizontal()
            }
        };

        // Plantilla visual para las pestañas/opciones del menú (ItemTemplate)
        ItemTemplate = new DataTemplate(() =>
        {
            var iconLabel = new Label
            {
                FontSize = 20,
                VerticalOptions = LayoutOptions.Center
            }.Column(0);

            iconLabel.SetBinding(Label.TextProperty, new Binding("Icon", converter: new ImageSourceToStringConverter()));

            var titleLabel = new Label
            {
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0F172A"), // Color azul oscuro/negro pizarra perfectamente legible
                VerticalOptions = LayoutOptions.Center
            }.Column(1);

            titleLabel.SetBinding(Label.TextProperty, "Title");

            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 10 },
                StrokeThickness = 0,
                BackgroundColor = Colors.Transparent,
                Padding = new Thickness(16, 14),
                Margin = new Thickness(12, 4),
                Content = new Grid
                {
                    ColumnDefinitions = Columns.Define(36, Star),
                    Children =
                    {
                        iconLabel,
                        titleLabel
                    }
                }
            };

            return border;
        });

        // 1. Pestaña Página Principal
        Items.Add(new FlyoutItem()
        {
            Title = "Página Principal",
            Icon = "🏠",
            Items =
            {
                new ShellContent
                {
                    Content = mainPage,
                    Route = "mainpage"
                }
            }
        });

        // 2. Pestaña Gestión de Pago Móvil
        Items.Add(new FlyoutItem()
        {
            Title = "Gestión de Pago Móvil",
            Icon = "💸",
            Items =
            {
                new ShellContent()
                {
                    Content = pagoMovilPage,
                    Route = "pagomovil"
                }
            }
        });

        // 3. Pestaña Compras Generales
        Items.Add(new FlyoutItem()
        {
            Title = "Compras Generales",
            Icon = "🛒",
            Items =
            {
                new ShellContent()
                {
                    Content = compras,
                    Route = "compras"
                }
            }
        });
    }

    private class ImageSourceToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FileImageSource fileImageSource)
            {
                return fileImageSource.File;
            }
            if (value is FontImageSource fontImageSource)
            {
                return fontImageSource.Glyph;
            }
            return value?.ToString() ?? "🔹";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
