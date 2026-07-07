using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views;

public class Compras : ContentPage
{
    private readonly ComprasViewModel _viewModel;
    private Border? _totalsCard;

    private async Task AnimateButton(Button? button)
    {
        if (button == null) return;
        await button.ScaleToAsync(0.92, 70, Easing.CubicOut);
        await button.ScaleToAsync(1.0, 70, Easing.CubicIn);
    }

    private async Task FlashTotalsCard()
    {
        if (_totalsCard == null) return;
        await _totalsCard.ScaleToAsync(1.03, 100, Easing.CubicOut);
        _totalsCard.BackgroundColor = Color.FromArgb("#F1F5F9");
        await _totalsCard.ScaleToAsync(1.0, 100, Easing.CubicIn);
        _totalsCard.BackgroundColor = Colors.White;
    }

    public Compras(ComprasViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        this.BackgroundColor(Color.FromArgb("#F8FAFC")); // slate-50

        // Ocultar barra de navegación
        Shell.SetNavBarIsVisible(this, false);

        Content = new Grid
        {
            RowDefinitions = Rows.Define(Auto, Star),
            Padding = new Thickness(24, 48, 24, 24),
            RowSpacing = 16,
            Children =
            {
                // Top Header + Status Banner + Tab Selector (Fijos arriba)
                new VerticalStackLayout
                {
                    Spacing = 16,
                    Children =
                    {
                        CreateHeader(),
                        CreateStatusBanner(),
                        CreateTabSelector()
                    }
                }
                .Row(0),

                // Area de Contenido Principal
                new Grid
                {
                    Children =
                    {
                        // Pestaña 1: Lista Activa (CollectionView auto-scrolleable)
                        CreateActiveListCollectionView()
                            .Bind(View.IsVisibleProperty, nameof(ComprasViewModel.ActiveTab), convert: (string? tab) => tab == "List"),

                        // Pestaña 2: Historial (CollectionView + paginación fija abajo)
                        CreateHistoryGrid()
                            .Bind(View.IsVisibleProperty, nameof(ComprasViewModel.ActiveTab), convert: (string? tab) => tab == "History")
                    }
                }
                .Row(1)
                .Bind(Grid.OpacityProperty, nameof(ComprasViewModel.IsLoading), convert: (bool loading) => loading ? 0.35 : 1.0)
                .Bind(Grid.IsEnabledProperty, nameof(ComprasViewModel.IsLoading), convert: (bool loading) => !loading),

                CreateLoadingOverlay()
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Inicializar de forma diferida tras pintar la UI inicial con retraso para dar tiempo a la transición
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
        {
            Task.Run(async () => await _viewModel.InitializeAsync());
        });
    }

    private View CreateHeader()
    {
        return new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star),
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
                            .Text("Gestor de Compras")
                            .FontSize(22)
                            .Bold()
                            .TextColor(Color.FromArgb("#0F172A")),
                        new Label()
                            .Text("Cálculo y control de gastos cotidianos")
                            .FontSize(13)
                            .TextColor(Color.FromArgb("#64748B"))
                    }
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
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Content = new Label { LineBreakMode = LineBreakMode.WordWrap }
                    .TextColor(Color.FromArgb("#475569"))
                    .FontSize(12)
                    .Bind(Label.TextProperty, nameof(ComprasViewModel.StatusMessage))
            }
            .Padding(new Thickness(16, 12))
            .Bind(Border.IsVisibleProperty, nameof(ComprasViewModel.StatusMessage), 
                convert: (string? msg) => !string.IsNullOrEmpty(msg));
    }

    private View CreateTabSelector()
    {
        return new Grid
        {
            ColumnDefinitions = Columns.Define(Star, Star),
            ColumnSpacing = 10,
            Children =
            {
                new Button
                    {
                        CornerRadius = 6,
                        HeightRequest = 40,
                        Command = _viewModel.SwitchTabCommand,
                        CommandParameter = "List"
                    }
                    .Text("Lista Activa")
                    .Bold()
                    .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.ActiveTab),
                        convert: (string? tab) => tab == "List" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                    .Bind(Button.TextColorProperty, nameof(ComprasViewModel.ActiveTab),
                        convert: (string? tab) => tab == "List" ? Colors.White : Color.FromArgb("#475569"))
                    .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                    .Column(0),

                new Button
                    {
                        CornerRadius = 6,
                        HeightRequest = 40,
                        Command = _viewModel.SwitchTabCommand,
                        CommandParameter = "History"
                    }
                    .Text("Historial")
                    .Bold()
                    .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.ActiveTab),
                        convert: (string? tab) => tab == "History" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                    .Bind(Button.TextColorProperty, nameof(ComprasViewModel.ActiveTab),
                        convert: (string? tab) => tab == "History" ? Colors.White : Color.FromArgb("#475569"))
                    .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                    .Column(1)
            }
        };
    }

    private View CreateActiveListCollectionView()
    {
        return new CollectionView
        {
            Header = new VerticalStackLayout
            {
                Spacing = 20,
                Margin = new Thickness(0, 0, 0, 16),
                Children =
                {
                    CreateAddProductCard(),
                    CreateTotalsCard(),
                    new Label()
                        .Text("Productos en la Lista")
                        .FontSize(14)
                        .Bold()
                        .TextColor(Color.FromArgb("#0F172A"))
                        .Margin(new Thickness(0, 4, 0, 0))
                }
            },
            ItemTemplate = new DataTemplate(() => CreateShoppingItemCard()),
            EmptyView = new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                    Stroke = Color.FromArgb("#E2E8F0"),
                    StrokeThickness = 1,
                    BackgroundColor = Colors.White,
                    Content = new Label()
                        .Text("No hay productos en la lista activa.")
                        .TextColor(Color.FromArgb("#64748B"))
                        .FontSize(12)
                        .TextCenterHorizontal()
                }
                .Padding(new Thickness(24, 30))
        }
        .Bind(CollectionView.ItemsSourceProperty, nameof(ComprasViewModel.ShoppingItems));
    }

    private View CreateAddProductCard()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Label().Text("Agregar Producto").FontSize(14).Bold().TextColor(Color.FromArgb("#0F172A")),
                    
                    // Nombre del Producto (Opcional)
                    new VerticalStackLayout
                    {
                        Spacing = 4,
                        Children =
                        {
                            new Label().Text("Nombre del Producto (Opcional)").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                            new Entry()
                                .Placeholder("Ej: Harina PAN, Leche (o dejar vacío)")
                                .PlaceholderColor(Color.FromArgb("#94A3B8"))
                                .TextColor(Color.FromArgb("#0F172A"))
                                .BackgroundColor(Color.FromArgb("#F1F5F9"))
                                .Height(40)
                                .FontSize(13)
                                .Bind(Entry.TextProperty, nameof(ComprasViewModel.FormName), BindingMode.TwoWay)
                        }
                    },

                    // Grid para Precio y Cantidad
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Star),
                        ColumnSpacing = 12,
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    new Label().Text("Precio").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                                    new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 40 }
                                        .Placeholder("Ej: 1.50 o 60")
                                        .PlaceholderColor(Color.FromArgb("#94A3B8"))
                                        .TextColor(Color.FromArgb("#0F172A"))
                                        .BackgroundColor(Color.FromArgb("#F1F5F9"))
                                        .FontSize(13)
                                        .Bind(Entry.TextProperty, nameof(ComprasViewModel.FormPrice), BindingMode.TwoWay)
                                }
                            }
                            .Column(0),

                            new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    new Label().Text("Cantidad").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                                    new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 40 }
                                        .Placeholder("1")
                                        .PlaceholderColor(Color.FromArgb("#94A3B8"))
                                        .TextColor(Color.FromArgb("#0F172A"))
                                        .BackgroundColor(Color.FromArgb("#F1F5F9"))
                                        .FontSize(13)
                                        .Bind(Entry.TextProperty, nameof(ComprasViewModel.FormQuantityText), BindingMode.TwoWay)
                                }
                            }
                            .Column(1)
                        }
                    },

                    // Selector de Moneda
                    new VerticalStackLayout
                    {
                        Spacing = 4,
                        Children =
                        {
                            new Label().Text("Moneda del Precio").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                            new Grid
                            {
                                ColumnDefinitions = Columns.Define(Star, Star, Star),
                                ColumnSpacing = 8,
                                Children =
                                {
                                    new Button
                                        {
                                            CornerRadius = 6,
                                            HeightRequest = 36,
                                            Command = _viewModel.SelectFormCurrencyCommand,
                                            CommandParameter = "USD"
                                        }
                                        .Text("USD ($)")
                                        .FontSize(12)
                                        .Bold()
                                        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.FormCurrency),
                                            convert: (string? c) => c == "USD" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                        .Bind(Button.TextColorProperty, nameof(ComprasViewModel.FormCurrency),
                                            convert: (string? c) => c == "USD" ? Colors.White : Color.FromArgb("#475569"))
                                        .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                                        .Column(0),

                                    new Button
                                        {
                                            CornerRadius = 6,
                                            HeightRequest = 36,
                                            Command = _viewModel.SelectFormCurrencyCommand,
                                            CommandParameter = "VES"
                                        }
                                        .Text("VES (Bs)")
                                        .FontSize(12)
                                        .Bold()
                                        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.FormCurrency),
                                            convert: (string? c) => c == "VES" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                        .Bind(Button.TextColorProperty, nameof(ComprasViewModel.FormCurrency),
                                            convert: (string? c) => c == "VES" ? Colors.White : Color.FromArgb("#475569"))
                                        .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                                        .Column(1),

                                    new Button
                                        {
                                            CornerRadius = 6,
                                            HeightRequest = 36,
                                            Command = _viewModel.SelectFormCurrencyCommand,
                                            CommandParameter = "EUR"
                                        }
                                        .Text("EUR (€)")
                                        .FontSize(12)
                                        .Bold()
                                        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.FormCurrency),
                                            convert: (string? c) => c == "EUR" ? Color.FromArgb("#0F172A") : Color.FromArgb("#F1F5F9"))
                                        .Bind(Button.TextColorProperty, nameof(ComprasViewModel.FormCurrency),
                                            convert: (string? c) => c == "EUR" ? Colors.White : Color.FromArgb("#475569"))
                                        .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                                        .Column(2)
                                }
                            }
                        }
                    },

                    // Botón de Registrar
                    new Button
                        {
                            CornerRadius = 6,
                            HeightRequest = 40,
                            Command = _viewModel.AddItemCommand
                        }
                        .Text("+ Agregar a la Lista")
                        .TextColor(Colors.White)
                        .BackgroundColor(Color.FromArgb("#0F172A"))
                        .Bold()
                        .Margin(new Thickness(0, 4, 0, 0))
                        .Invoke(btn => btn.Clicked += async (s, e) => 
                        {
                            await AnimateButton(btn);
                            await Task.Delay(250);
                            await FlashTotalsCard();
                        })
                }
            }
        }
        .Padding(16);
    }

    private View CreateTotalsCard()
    {
        _totalsCard = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new Label().Text("Totales Estimados").FontSize(14).Bold().TextColor(Color.FromArgb("#0F172A")),
                    
                    // Fila de Totales
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
                                    new Label().Text("TOTAL VES").FontSize(9).Bold().TextColor(Color.FromArgb("#64748B")),
                                    new Label().FontSize(15).Bold().TextColor(Color.FromArgb("#0F172A"))
                                        .Bind(Label.TextProperty, nameof(ComprasViewModel.TotalVes), stringFormat: "{0:N2} Bs")
                                }
                            }
                            .Column(0),

                            new VerticalStackLayout
                            {
                                Spacing = 2,
                                HorizontalOptions = LayoutOptions.Center,
                                Children =
                                {
                                    new Label().Text("TOTAL USD").FontSize(9).Bold().TextColor(Color.FromArgb("#64748B")),
                                    new Label().FontSize(15).Bold().TextColor(Color.FromArgb("#0F172A"))
                                        .Bind(Label.TextProperty, nameof(ComprasViewModel.TotalUsd), stringFormat: "${0:N2}")
                                }
                            }
                            .Column(1),

                            new VerticalStackLayout
                            {
                                Spacing = 2,
                                HorizontalOptions = LayoutOptions.Center,
                                Children =
                                {
                                    new Label().Text("TOTAL EUR").FontSize(9).Bold().TextColor(Color.FromArgb("#64748B")),
                                    new Label().FontSize(15).Bold().TextColor(Color.FromArgb("#0F172A"))
                                        .Bind(Label.TextProperty, nameof(ComprasViewModel.TotalEur), stringFormat: "€{0:N2}")
                                }
                            }
                            .Column(2)
                        }
                    },

                    // Integración Pago Móvil
                    new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label().Text("Copiar Pago Móvil rápido").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                            new Grid
                            {
                                ColumnDefinitions = Columns.Define(Star, Auto),
                                ColumnSpacing = 8,
                                Children =
                                {
                                    new Border
                                    {
                                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                                        BackgroundColor = Color.FromArgb("#F1F5F9"),
                                        Content = new Picker
                                        {
                                            Title = "Seleccione contacto",
                                            TitleColor = Color.FromArgb("#94A3B8"),
                                            TextColor = Color.FromArgb("#0F172A"),
                                            ItemDisplayBinding = new Binding(nameof(PagoMovilRecord.DisplayName)),
                                            HeightRequest = 36
                                        }
                                        .Bind(Picker.ItemsSourceProperty, nameof(ComprasViewModel.PagoMovilList))
                                        .Bind(Picker.SelectedItemProperty, nameof(ComprasViewModel.SelectedPagoMovil), BindingMode.TwoWay)
                                    }
                                    .Padding(new Thickness(8, 0)),

                                    new Button
                                        {
                                            CornerRadius = 6,
                                            HeightRequest = 36,
                                            Command = _viewModel.CopyPagoMovilCommand
                                        }
                                        .Text("Copiar")
                                        .TextColor(Colors.White)
                                        .BackgroundColor(Color.FromArgb("#0F172A"))
                                        .Bold()
                                        .FontSize(11)
                                        .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                                        .Column(1)
                                }
                            }
                        }
                    },
 
                    // Botón Completar Compra
                    new Button { CornerRadius = 6, HeightRequest = 42 }
                        .Text("Guardar y Completar Compra")
                        .TextColor(Colors.White)
                        .BackgroundColor(Color.FromArgb("#16A34A")) // Verde success
                        .Bold()
                        .Invoke(btn => btn.Clicked += async (s, e) =>
                        {
                            await AnimateButton(btn);
                            if (_viewModel.ShoppingItems.Count == 0)
                            {
                                await DisplayAlertAsync("Vacío", "No hay productos en la lista.", "Aceptar");
                                return;
                            }

                            bool confirm = await DisplayAlertAsync("Confirmar", "¿Desea registrar esta compra y vaciar la lista activa?", "Registrar", "Cancelar");
                            if (confirm)
                            {
                                _viewModel.CompletePurchaseCommand.Execute(null);
                            }
                        })
                }
            }
        }
        .Padding(16);
        return _totalsCard;
    }

    private View CreateShoppingItemCard()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    // Nombre y cantidad
                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label { LineBreakMode = LineBreakMode.TailTruncation }
                                .Bold()
                                .TextColor(Color.FromArgb("#0F172A"))
                                .FontSize(13)
                                .Bind(Label.TextProperty, nameof(ShoppingItem.Name)),

                            new Label()
                                .TextColor(Color.FromArgb("#64748B"))
                                .FontSize(11)
                                .Bind(Label.TextProperty, nameof(ShoppingItem.Quantity), stringFormat: "Cantidad: {0}")
                        }
                    }
                    .Column(0),

                    // Precio total en divisa nativa
                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        Children =
                        {
                            new Label()
                                .Bold()
                                .TextColor(Color.FromArgb("#0F172A"))
                                .FontSize(13)
                                .Bind(Label.TextProperty, convert: (ShoppingItem? item) => 
                                {
                                    if (item == null) return string.Empty;
                                    string symbol = item.Currency == "USD" ? "$" : item.Currency == "EUR" ? "€" : "Bs";
                                    return $"{item.TotalPrice:N2} {symbol}";
                                }),

                            new Label()
                                .TextColor(Color.FromArgb("#94A3B8"))
                                .FontSize(10)
                                .Bind(Label.TextProperty, convert: (ShoppingItem? item) =>
                                {
                                    if (item == null) return string.Empty;
                                    string symbol = item.Currency == "USD" ? "$" : item.Currency == "EUR" ? "€" : "Bs";
                                    return $"{item.Quantity}x {item.Price:N2} {symbol}";
                                })
                        }
                    }
                    .Column(1)
                    .Margin(new Thickness(0, 0, 8, 0)),

                    // Botón Eliminar
                    new Button
                        {
                            TextColor = Color.FromArgb("#EF4444"),
                            BackgroundColor = Colors.Transparent,
                            HeightRequest = 32,
                            WidthRequest = 32,
                            Padding = 0
                        }
                        .Text("✕")
                        .Bold()
                        .FontSize(13)
                        .Invoke(btn => btn.Clicked += async (s, e) => 
                        {
                            await AnimateButton(btn);
                            if (btn.BindingContext is ShoppingItem item)
                            {
                                _viewModel.DeleteItemCommand.Execute(item);
                                await Task.Delay(250);
                                await FlashTotalsCard();
                            }
                        })
                        .Column(2)
                        .CenterVertical()
                }
            }
        }
        .Padding(new Thickness(12, 10))
        .Margin(new Thickness(0, 0, 0, 8));
    }

    private View CreateHistoryGrid()
    {
        return new Grid
        {
            RowDefinitions = Rows.Define(Star, Auto),
            RowSpacing = 8,
            Children =
            {
                new CollectionView
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
                                .TextColor(Color.FromArgb("#0F172A"))
                        }
                    },
                    ItemTemplate = new DataTemplate(() => CreateHistoryRecordCard()),
                    EmptyView = new Border
                    {
                        StrokeShape = new RoundRectangle { CornerRadius = 6 },
                        Stroke = Color.FromArgb("#E2E8F0"),
                        StrokeThickness = 1,
                        BackgroundColor = Colors.White,
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
                                    .TextColor(Color.FromArgb("#0F172A"))
                                    .TextCenterHorizontal(),
                                new Label { LineBreakMode = LineBreakMode.WordWrap }
                                    .Text("Tus compras completadas aparecerán aquí una vez que registres tu lista de compras activa.")
                                    .FontSize(12)
                                    .TextColor(Color.FromArgb("#64748B"))
                                    .TextCenterHorizontal()
                            }
                        }
                    }
                    .Padding(new Thickness(24, 40))
                }
                .Row(0)
                .Bind(CollectionView.ItemsSourceProperty, nameof(ComprasViewModel.PurchaseHistory)),

                // Controles de Paginación sticky al fondo
                new Grid
                {
                    ColumnDefinitions = Columns.Define(Auto, Star, Auto),
                    Padding = new Thickness(0, 8),
                    Children =
                    {
                        new Button
                            {
                                CornerRadius = 6,
                                HeightRequest = 36,
                                Command = _viewModel.PreviousHistoryPageCommand
                            }
                            .Text("◀ Anterior")
                            .FontSize(12)
                            .Bold()
                            .TextColor(Colors.White)
                            .Bind(Button.IsEnabledProperty, nameof(ComprasViewModel.HasPreviousHistoryPage))
                            .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.HasPreviousHistoryPage),
                                convert: (bool enabled) => enabled ? Color.FromArgb("#0F172A") : Color.FromArgb("#CBD5E1"))
                            .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                            .Column(0),

                        new Label()
                            .TextColor(Color.FromArgb("#0F172A"))
                            .Bold()
                            .FontSize(13)
                            .TextCenterHorizontal()
                            .CenterVertical()
                            .Bind(Label.TextProperty, nameof(ComprasViewModel.HistoryPageText))
                            .Column(1),

                        new Button
                            {
                                CornerRadius = 6,
                                HeightRequest = 36,
                                Command = _viewModel.NextHistoryPageCommand
                            }
                            .Text("Siguiente ▶")
                            .FontSize(12)
                            .Bold()
                            .TextColor(Colors.White)
                            .Bind(Button.IsEnabledProperty, nameof(ComprasViewModel.HasNextHistoryPage))
                            .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.HasNextHistoryPage),
                                convert: (bool enabled) => enabled ? Color.FromArgb("#0F172A") : Color.FromArgb("#CBD5E1"))
                            .Invoke(btn => btn.Clicked += async (s, e) => await AnimateButton(btn))
                            .Column(2)
                    }
                }
                .Row(1)
                .Bind(Grid.IsVisibleProperty, nameof(ComprasViewModel.TotalHistoryPages), convert: (int totalPages) => totalPages > 1)
            }
        };
    }

    private View CreateHistoryRecordCard()
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    // Izquierda: Fecha y detalle
                    new VerticalStackLayout
                    {
                        Spacing = 4,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label()
                                .Bold()
                                .TextColor(Color.FromArgb("#0F172A"))
                                .FontSize(12)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.PurchaseDate),
                                    convert: (DateTime dt) => dt.ToString("dd/MM/yyyy - hh:mm tt")),

                            new Label { LineBreakMode = LineBreakMode.WordWrap }
                                .TextColor(Color.FromArgb("#64748B"))
                                .FontSize(11)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.ItemSummary))
                        }
                    }
                    .Column(0),

                    // Centro: Totales convertidos
                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        Children =
                        {
                            new Label()
                                .Bold()
                                .TextColor(Color.FromArgb("#0F172A"))
                                .FontSize(12)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.TotalVes), stringFormat: "{0:N2} Bs"),

                            new Label()
                                .TextColor(Color.FromArgb("#475569"))
                                .FontSize(11)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.TotalUsd), stringFormat: "${0:N2}"),

                            new Label()
                                .TextColor(Color.FromArgb("#94A3B8"))
                                .FontSize(10)
                                .Bind(Label.TextProperty, nameof(PurchaseRecord.TotalEur), stringFormat: "€{0:N2}")
                        }
                    }
                    .Column(1)
                    .Margin(new Thickness(0, 0, 8, 0)),

                    // Derecha: Botón Eliminar del Historial
                    new Button
                        {
                            TextColor = Color.FromArgb("#EF4444"),
                            BackgroundColor = Colors.Transparent,
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
                                bool confirm = await DisplayAlertAsync("Eliminar Compra", "¿Desea eliminar esta compra del historial?", "Eliminar", "Cancelar");
                                if (confirm)
                                {
                                    _viewModel.DeletePurchaseCommand.Execute(rec);
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
                await Navigation.PushAsync(new PurchaseDetailPage(rec));
            }
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    private View CreateLoadingOverlay()
    {
        return new Grid
        {
            BackgroundColor = Color.FromArgb("#80FFFFFF"),
            Children =
            {
                new ActivityIndicator
                {
                    Color = Color.FromArgb("#0F172A"),
                    HeightRequest = 50,
                    WidthRequest = 50,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
                .Bind(ActivityIndicator.IsRunningProperty, nameof(ComprasViewModel.IsLoading))
            }
        }
        .Bind(Grid.IsVisibleProperty, nameof(ComprasViewModel.IsLoading));
    }
}