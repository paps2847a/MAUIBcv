using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views;

public class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        this.BackgroundColor(Color.FromArgb("#F8FAFC")); // slate-50

        // Ocultar la barra de navegación del Shell
        Shell.SetNavBarIsVisible(this, false);

        // Layout principal que superpone el contenido y la capa de carga
        Content = new Grid
        {
            Children =
            {
                // 1. CONTENIDO PRINCIPAL
                new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Spacing = 24,
                        Padding = new Thickness(24, 48, 24, 24),
                        Children =
                        {
                            CreateHeader(),
                            CreateStatusBanner(),
                            CreateRatesGrid(),
                            CreateDatePickerSection(),
                            CreateConverterSection(),
                            CreateHistorySection()
                        }
                    }
                }
                .Bind(ScrollView.OpacityProperty, nameof(MainViewModel.IsBusy), convert: (bool isBusy) => isBusy ? 0.35 : 1.0)
                .Bind(ScrollView.IsEnabledProperty, nameof(MainViewModel.IsBusy), convert: (bool isBusy) => !isBusy),

                // 2. CAPA DE CARGA (DIFUMINADO + SPINNER)
                CreateLoadingOverlay()
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Datos pre-cargados en el arranque: no se necesita inicializar aquí
    }

    private View CreateHeader()
    {
        return new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star, Auto),
            ColumnSpacing = 12,
            Children =
            {
                // Botón del Menú Desplegable (Flyout)
                new Button
                    {
                        BackgroundColor = Colors.Transparent,
                        BorderWidth = 0,
                        Padding = 0,
                        HeightRequest = 40,
                        WidthRequest = 40
                    }
                    .Text("☰")
                    .FontSize(20)
                    .Bold()
                    .TextColor(Color.FromArgb("#0F172A"))
                    .Invoke(btn => btn.Command = new Command(() => Shell.Current.FlyoutIsPresented = true))
                    .Column(0)
                    .CenterVertical(),

                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label()
                            .Text("BCV Tasas de Cambio")
                            .FontSize(22)
                            .Bold()
                            .TextColor(Color.FromArgb("#0F172A")), // slate-900
                        new Label()
                            .FontSize(13)
                            .TextColor(Color.FromArgb("#64748B")) // slate-500
                            .Bind(Label.TextProperty, nameof(MainViewModel.FormattedDate), stringFormat: "Fecha Valor: {0}")
                    }
                }
                .Column(1)
                .CenterVertical(),

                new Button
                    {
                        TextColor = Colors.White,
                        BackgroundColor = Color.FromArgb("#0F172A"),
                        CornerRadius = 15,
                        Padding = new Thickness(14, 0),
                        HeightRequest = 30
                    }
                    .Text("Actualizar")
                    .Bold()
                    .FontSize(11)
                    .Bind(Button.CommandProperty, nameof(MainViewModel.FetchLatestRatesCommand))
                    .Column(2)
                    .CenterVertical()
            }
        };
    }

    private View CreateStatusBanner()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"), // slate-200
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Content = new Label { LineBreakMode = LineBreakMode.WordWrap }
                .TextColor(Color.FromArgb("#475569")) // slate-600
                .FontSize(12)
                .Bind(Label.TextProperty, nameof(MainViewModel.StatusMessage))
        }
        .Padding(new Thickness(16, 12))
        .Bind(Border.IsVisibleProperty, nameof(MainViewModel.StatusMessage), 
            convert: (string? msg) => !string.IsNullOrEmpty(msg));
    }

    private View CreateRatesGrid()
    {
        return new Grid
        {
            ColumnDefinitions = Columns.Define(Star, Star),
            ColumnSpacing = 16,
            Children =
            {
                // Tarjeta Dolar
                new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Stroke = Color.FromArgb("#E2E8F0"),
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label().Text("DOLAR (USD)").FontSize(11).Bold().TextColor(Color.FromArgb("#64748B")),
                            new Label().FontSize(22).Bold().TextColor(Color.FromArgb("#0F172A"))
                                .Bind(Label.TextProperty, nameof(MainViewModel.UsdRate), stringFormat: "{0:N4} VES"),
                            new Label().Text("Banco Central de Venezuela").FontSize(9).TextColor(Color.FromArgb("#94A3B8"))
                        }
                    }
                }
                .Padding(16)
                .Column(0),

                // Tarjeta Euro
                new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Stroke = Color.FromArgb("#E2E8F0"),
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label().Text("EURO (EUR)").FontSize(11).Bold().TextColor(Color.FromArgb("#64748B")),
                            new Label().FontSize(22).Bold().TextColor(Color.FromArgb("#0F172A"))
                                .Bind(Label.TextProperty, nameof(MainViewModel.EurRate), stringFormat: "{0:N4} VES"),
                            new Label().Text("Banco Central de Venezuela").FontSize(9).TextColor(Color.FromArgb("#94A3B8"))
                        }
                    }
                }
                .Padding(16)
                .Column(1)
            }
        };
    }

    private View CreateDatePickerSection()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            BackgroundColor = Colors.White,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label()
                        .Text("Consultar fecha anterior")
                        .FontSize(13)
                        .Bold()
                        .TextColor(Color.FromArgb("#0F172A")),
                    new DatePicker { Format = "dd/MM/yyyy", MaximumDate = DateTime.Today }
                        .TextColor(Color.FromArgb("#0F172A"))
                        .BackgroundColor(Color.FromArgb("#F1F5F9")) // slate-100
                        .Bind(DatePicker.DateProperty, nameof(MainViewModel.SelectedDate), BindingMode.TwoWay)
                }
            }
        }
        .Padding(16);
    }

    private View CreateConverterSection()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            BackgroundColor = Colors.White,
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new Label()
                        .Text("Conversor de monedas")
                        .FontSize(14)
                        .Bold()
                        .TextColor(Color.FromArgb("#0F172A")),

                    // Selector de Moneda (USD / EUR)
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Star),
                        ColumnSpacing = 10,
                        Children =
                        {
                            new Button
                                {
                                    CornerRadius = 6,
                                    HeightRequest = 40,
                                    Command = _viewModel.SelectCurrencyCommand,
                                    CommandParameter = "USD"
                                }
                                .Text("Dolar (USD)")
                                .Bind(Button.BackgroundColorProperty, nameof(MainViewModel.SelectedCurrency),
                                    convert: (string? curr) => curr == "USD" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                .Bind(Button.TextColorProperty, nameof(MainViewModel.SelectedCurrency),
                                    convert: (string? curr) => curr == "USD" ? Colors.White : Color.FromArgb("#475569"))
                                .Column(0),

                            new Button
                                {
                                    CornerRadius = 6,
                                    HeightRequest = 40,
                                    Command = _viewModel.SelectCurrencyCommand,
                                    CommandParameter = "EUR"
                                }
                                .Text("Euro (EUR)")
                                .Bind(Button.BackgroundColorProperty, nameof(MainViewModel.SelectedCurrency),
                                    convert: (string? curr) => curr == "EUR" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                .Bind(Button.TextColorProperty, nameof(MainViewModel.SelectedCurrency),
                                    convert: (string? curr) => curr == "EUR" ? Colors.White : Color.FromArgb("#475569"))
                                .Column(1)
                        }
                    },

                    // Entrada de Monto
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Auto, Auto),
                        ColumnSpacing = 10,
                        Children =
                        {
                            new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 42 }
                                .Placeholder("Ingrese monto")
                                .PlaceholderColor(Color.FromArgb("#94A3B8"))
                                .TextColor(Color.FromArgb("#0F172A"))
                                .BackgroundColor(Color.FromArgb("#F1F5F9"))
                                .Bind(Entry.TextProperty, nameof(MainViewModel.AmountText), BindingMode.TwoWay)
                                .Column(0),

                            new Button { CornerRadius = 6, HeightRequest = 42, WidthRequest = 42, Command = _viewModel.CopyAmountToConvertCommand }
                                .Text("⎘")
                                .FontSize(14)
                                .TextColor(Color.FromArgb("#475569"))
                                .BackgroundColor(Color.FromArgb("#F1F5F9"))
                                .Column(1),

                            new Button { CornerRadius = 6, HeightRequest = 42, Command = _viewModel.ToggleDirectionCommand }
                                .TextColor(Colors.White)
                                .BackgroundColor(Color.FromArgb("#0F172A"))
                                .Bold()
                                .Bind(Button.TextProperty, nameof(MainViewModel.IsToVes),
                                    convert: (bool toVes) => toVes ? "Divisa a VES" : "VES a Divisa")
                                .Column(2)
                        }
                    },

                    // Resultado
                    new Border
                    {
                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                        BackgroundColor = Color.FromArgb("#F1F5F9"),
                        Content = new Grid
                        {
                            ColumnDefinitions = Columns.Define(Star, Auto),
                            Children =
                            {
                                new VerticalStackLayout
                                {
                                    HorizontalOptions = LayoutOptions.Center,
                                    Spacing = 4,
                                    Children =
                                    {
                                        new Label().Text("RESULTADO ESTIMADO").FontSize(9).TextColor(Color.FromArgb("#64748B")).TextCenterHorizontal(),
                                        new Label()
                                            .FontSize(24)
                                            .Bold()
                                            .TextColor(Color.FromArgb("#0F172A"))
                                            .Bind(Label.TextProperty, nameof(MainViewModel.ConversionResult))
                                    }
                                }
                                .Column(0),

                                new Button
                                    {
                                        BackgroundColor = Colors.Transparent,
                                        HeightRequest = 40,
                                        WidthRequest = 40,
                                        Command = _viewModel.CopyResultCommand
                                    }
                                    .Text("⎘")
                                    .FontSize(16)
                                    .TextColor(Color.FromArgb("#475569"))
                                    .Column(1)
                                    .CenterVertical()
                            }
                        }
                    }
                    .Padding(12)
                }
            }
        }
        .Padding(20);
    }

    private View CreateHistorySection()
    {
        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label()
                    .Text("Historial reciente")
                    .FontSize(14)
                    .Bold()
                    .TextColor(Color.FromArgb("#0F172A"))
                    .Margin(new Thickness(0, 8, 0, 0)),

                new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Stroke = Color.FromArgb("#E2E8F0"),
                    BackgroundColor = Colors.White,
                    Content = new VerticalStackLayout
                    {
                        Children =
                        {
                            // Encabezado de Tabla
                            new Grid
                            {
                                BackgroundColor = Color.FromArgb("#F1F5F9"),
                                Padding = new Thickness(12, 8),
                                ColumnDefinitions = Columns.Define(Star, Star, Star),
                                Children =
                                {
                                    new Label().Text("Fecha").Bold().TextColor(Color.FromArgb("#0F172A")).FontSize(12).Column(0),
                                    new Label().Text("USD (VES)").Bold().TextColor(Color.FromArgb("#0F172A")).FontSize(12).TextEnd().Column(1),
                                    new Label().Text("EUR (VES)").Bold().TextColor(Color.FromArgb("#0F172A")).FontSize(12).TextEnd().Column(2)
                                }
                            },

                            // Lista de Registros
                            new CollectionView
                            {
                                HeightRequest = 200,
                                ItemTemplate = new DataTemplate(() =>
                                {
                                    return new Grid
                                    {
                                        Padding = new Thickness(12, 10),
                                        ColumnDefinitions = Columns.Define(Star, Star, Star),
                                        Children =
                                        {
                                            new Label { TextColor = Color.FromArgb("#475569"), FontSize = 12 }
                                                .Bind(Label.TextProperty, nameof(ExchangeRate.Date), 
                                                    convert: (DateTime date) => date.ToString("dd/MM/yyyy"))
                                                .Column(0),

                                            new Label { TextColor = Color.FromArgb("#0F172A"), FontSize = 12 }
                                                .TextEnd()
                                                .Bind(Label.TextProperty, nameof(ExchangeRate.UsdRate), stringFormat: "{0:N2}")
                                                .Column(1),

                                            new Label { TextColor = Color.FromArgb("#0F172A"), FontSize = 12 }
                                                .TextEnd()
                                                .Bind(Label.TextProperty, nameof(ExchangeRate.EurRate), stringFormat: "{0:N2}")
                                                .Column(2)
                                        }
                                    };
                                })
                            }
                            .Bind(CollectionView.ItemsSourceProperty, nameof(MainViewModel.History))
                        }
                    }
                }
                .Padding(0)
            }
        };
    }

    private View CreateLoadingOverlay()
    {
        return new Grid
        {
            // Fondo semitransparente que simula un difuminado sutil
            BackgroundColor = Color.FromRgba(248, 250, 252, 160), // slate-50 con opacidad del 63%
            InputTransparent = false, // Bloquea clics en los elementos de fondo
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new ActivityIndicator
                            {
                                Color = Color.FromArgb("#0F172A"),
                                HeightRequest = 44,
                                WidthRequest = 44
                            }
                            .CenterHorizontal()
                            .Bind(ActivityIndicator.IsRunningProperty, nameof(MainViewModel.IsBusy)),

                        new Label()
                            .TextColor(Color.FromArgb("#334155"))
                            .FontSize(13)
                            .Bold()
                            .TextCenterHorizontal()
                            .Bind(Label.TextProperty, nameof(MainViewModel.StatusMessage))
                    }
                }
            }
        }
        .Bind(Grid.IsVisibleProperty, nameof(MainViewModel.IsBusy));
    }
}
