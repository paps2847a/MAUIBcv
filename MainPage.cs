using System;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Store;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp;

public class MainPage : ContentPage
{
    private readonly BcvStore _store;

    public MainPage(BcvStore store)
    {
        _store = store;
        BindingContext = _store;
        BackgroundColor = Color.FromArgb("#F8FAFC"); // Fondo claro y limpio slate-50

        // Configuración de la barra de navegación en Shell
        Shell.SetNavBarIsVisible(this, false);

        // Grid principal para permitir superponer la animación de carga (blur/overlay)
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
                            // ENCABEZADO
                            CreateHeader(),

                            // BANNER DE ESTADO HISTÓRICO
                            CreateStatusBanner(),

                            // TARJETAS DE TASAS ACTUALES
                            CreateRatesGrid(),

                            // CONSULTA HISTÓRICA (DATE PICKER)
                            CreateDatePickerSection(),

                            // CONVERSOR DE MONEDAS
                            CreateConverterSection(),

                            // HISTORIAL DE TASAS (TABLA / LISTA)
                            CreateHistorySection()
                        }
                    }
                }
                .Bind(ScrollView.OpacityProperty, "State.IsBusy", convert: (bool isBusy) => isBusy ? 0.35 : 1.0)
                .Bind(ScrollView.IsEnabledProperty, "State.IsBusy", convert: (bool isBusy) => !isBusy),

                // 2. OVERLAY DE CARGA (DIFUMINACIÓN Y SPINNER CENTRAL)
                CreateLoadingOverlay()
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Deferir la inicialización para permitir que la UI se dibuje instantáneamente al iniciar
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
        {
            Task.Run(async () => await _store.InitializeAsync());
        });
    }

    // --- Componentes Visuales ---

    private View CreateHeader()
    {
        return new Grid
        {
            ColumnDefinitions = Columns.Define(Star, Auto),
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = "BCV Tasas de Cambio",
                            FontSize = 22,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A") // slate-900
                        },
                        new Label
                        {
                            FontSize = 13,
                            TextColor = Color.FromArgb("#64748B") // slate-500
                        }
                        .Bind(Label.TextProperty, "State.FormattedDate", stringFormat: "Fecha Valor: {0}")
                    }
                }
                .Column(0),

                new Button
                {
                    Text = "Actualizar",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    BackgroundColor = Color.FromArgb("#0F172A"), // Contraste oscuro minimalista
                    CornerRadius = 15, // Totalmente ovalado
                    Padding = new Thickness(14, 0),
                    HeightRequest = 30, // Reducido
                    FontSize = 11,
                    Margin = new Thickness(16, 0, 0, 0), // Separado para evitar amontonamiento
                    Command = new Command(() => _store.Dispatch(new FetchLatestRates()))
                }
                .Column(1)
                .CenterVertical()
            }
        };
    }

    private View CreateStatusBanner()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"), // Gris slate-200
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 12),
            Content = new Label
            {
                TextColor = Color.FromArgb("#475569"), // slate-600
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap
            }
            .Bind(Label.TextProperty, "State.StatusMessage")
        }
        .Bind(Border.IsVisibleProperty, "State.StatusMessage", 
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
                    Padding = 16,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = "DOLAR (USD)", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") },
                            new Label { FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                .Bind(Label.TextProperty, "State.UsdRate", stringFormat: "{0:N4} VES"),
                            new Label { Text = "Banco Central de Venezuela", FontSize = 9, TextColor = Color.FromArgb("#94A3B8") }
                        }
                    }
                }
                .Column(0),

                // Tarjeta Euro
                new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Stroke = Color.FromArgb("#E2E8F0"),
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    Padding = 16,
                    Content = new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = "EURO (EUR)", FontSize = 11, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#64748B") },
                            new Label { FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                .Bind(Label.TextProperty, "State.EurRate", stringFormat: "{0:N4} VES"),
                            new Label { Text = "Banco Central de Venezuela", FontSize = 9, TextColor = Color.FromArgb("#94A3B8") }
                        }
                    }
                }
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
            Padding = 16,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = "Consultar fecha anterior",
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#0F172A")
                    },
                    new DatePicker
                    {
                        Format = "dd/MM/yyyy",
                        MaximumDate = DateTime.Today,
                        TextColor = Color.FromArgb("#0F172A"),
                        BackgroundColor = Color.FromArgb("#F1F5F9") // slate-100
                    }
                    .Bind(DatePicker.DateProperty, "State.SelectedDate", BindingMode.OneWay)
                    .Invoke(picker => picker.DateSelected += (s, e) => 
                    {
                        var newDate = e.NewDate ?? DateTime.Today;
                        if (_store.State.SelectedDate.Date != newDate.Date)
                        {
                            _store.Dispatch(new SelectDate(newDate));
                        }
                    })
                }
            }
        };
    }

    private View CreateConverterSection()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            BackgroundColor = Colors.White,
            Padding = 20,
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new Label
                    {
                        Text = "Conversor de monedas",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#0F172A")
                    },

                    // Selector de Moneda (USD / EUR)
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Star),
                        ColumnSpacing = 10,
                        Children =
                        {
                            new Button { Text = "Dolar (USD)", CornerRadius = 6, HeightRequest = 40, Command = new Command(() => _store.Dispatch(new SelectCurrency("USD"))) }
                                .Bind(Button.BackgroundColorProperty, "State.SelectedCurrency",
                                    convert: (string? curr) => curr == "USD" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                .Bind(Button.TextColorProperty, "State.SelectedCurrency",
                                    convert: (string? curr) => curr == "USD" ? Colors.White : Color.FromArgb("#475569"))
                                .Column(0),

                            new Button { Text = "Euro (EUR)", CornerRadius = 6, HeightRequest = 40, Command = new Command(() => _store.Dispatch(new SelectCurrency("EUR"))) }
                                .Bind(Button.BackgroundColorProperty, "State.SelectedCurrency",
                                    convert: (string? curr) => curr == "EUR" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                .Bind(Button.TextColorProperty, "State.SelectedCurrency",
                                    convert: (string? curr) => curr == "EUR" ? Colors.White : Color.FromArgb("#475569"))
                                .Column(1)
                        }
                    },

                    // Entrada de Monto
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Auto),
                        ColumnSpacing = 10,
                        Children =
                        {
                            new Entry
                            {
                                Placeholder = "Ingrese monto",
                                PlaceholderColor = Color.FromArgb("#94A3B8"),
                                TextColor = Color.FromArgb("#0F172A"),
                                Keyboard = Keyboard.Numeric,
                                BackgroundColor = Color.FromArgb("#F1F5F9"),
                                HeightRequest = 42
                            }
                            .Bind(Entry.TextProperty, "State.AmountText", BindingMode.OneWay)
                            .Invoke(entry => entry.TextChanged += (s, e) => 
                            {
                                if (_store.State.AmountText != e.NewTextValue)
                                {
                                    _store.Dispatch(new ChangeAmount(e.NewTextValue));
                                }
                            })
                            .Column(0),

                            new Button
                            {
                                CornerRadius = 6,
                                TextColor = Colors.White,
                                BackgroundColor = Color.FromArgb("#0F172A"),
                                HeightRequest = 42,
                                FontAttributes = FontAttributes.Bold,
                                Command = new Command(() => _store.Dispatch(new ToggleDirection()))
                            }
                            .Bind(Button.TextProperty, "State.IsToVes",
                                convert: (bool toVes) => toVes ? "Divisa a VES" : "VES a Divisa")
                            .Column(1)
                        }
                    },

                    // Resultado
                    new Border
                    {
                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                        BackgroundColor = Color.FromArgb("#F1F5F9"),
                        Padding = 12,
                        Content = new VerticalStackLayout
                        {
                            HorizontalOptions = LayoutOptions.Center,
                            Spacing = 4,
                            Children =
                            {
                                new Label { Text = "RESULTADO ESTIMADO", FontSize = 9, TextColor = Color.FromArgb("#64748B"), HorizontalTextAlignment = TextAlignment.Center },
                                new Label
                                {
                                    FontSize = 24,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Color.FromArgb("#0F172A")
                                }
                                .Bind(Label.TextProperty, "State.ConversionResult")
                            }
                        }
                    }
                }
            }
        };
    }

    private View CreateHistorySection()
    {
        return new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label
                {
                    Text = "Historial reciente",
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#0F172A"),
                    Margin = new Thickness(0, 8, 0, 0)
                },

                new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Stroke = Color.FromArgb("#E2E8F0"),
                    BackgroundColor = Colors.White,
                    Padding = 0,
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
                                    new Label { Text = "Fecha", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), FontSize = 12 }.Column(0),
                                    new Label { Text = "USD (VES)", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), FontSize = 12, HorizontalTextAlignment = TextAlignment.End }.Column(1),
                                    new Label { Text = "EUR (VES)", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), FontSize = 12, HorizontalTextAlignment = TextAlignment.End }.Column(2)
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

                                            new Label { TextColor = Color.FromArgb("#0F172A"), FontSize = 12, HorizontalTextAlignment = TextAlignment.End }
                                                .Bind(Label.TextProperty, nameof(ExchangeRate.UsdRate), stringFormat: "{0:N2}")
                                                .Column(1),

                                            new Label { TextColor = Color.FromArgb("#0F172A"), FontSize = 12, HorizontalTextAlignment = TextAlignment.End }
                                                .Bind(Label.TextProperty, nameof(ExchangeRate.EurRate), stringFormat: "{0:N2}")
                                                .Column(2)
                                        }
                                    };
                                })
                            }
                            .Bind(CollectionView.ItemsSourceProperty, "State.History")
                        }
                    }
                }
            }
        };
    }

    // Capa de carga que difumine levemente el contenido y muestra el spinner central
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
                            WidthRequest = 44,
                            HorizontalOptions = LayoutOptions.Center
                        }
                        .Bind(ActivityIndicator.IsRunningProperty, "State.IsBusy"),

                        new Label
                        {
                            TextColor = Color.FromArgb("#334155"),
                            FontSize = 13,
                            FontAttributes = FontAttributes.Bold,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                        .Bind(Label.TextProperty, "State.StatusMessage")
                    }
                }
            }
        }
        .Bind(Grid.IsVisibleProperty, "State.IsBusy");
    }
}
