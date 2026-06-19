using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp;

public class PagoMovilPage : ContentPage
{
    private readonly PagoMovilViewModel _viewModel;

    public PagoMovilPage(PagoMovilViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        BackgroundColor = Color.FromArgb("#F8FAFC"); // slate-50

        // Ocultar barra de navegación
        Shell.SetNavBarIsVisible(this, false);

        Content = new Grid
        {
            Children =
            {
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
                            CreateSearchBar(),
                            CreateRecordsList()
                        }
                    }
                }
                .Bind(ScrollView.OpacityProperty, nameof(PagoMovilViewModel.IsLoading), convert: (bool loading) => loading ? 0.35 : 1.0)
                .Bind(ScrollView.IsEnabledProperty, nameof(PagoMovilViewModel.IsLoading), convert: (bool loading) => !loading),

                CreateLoadingOverlay()
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Cargar registros al entrar
        Task.Run(async () => await _viewModel.InitializeAsync());
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
                    Text = "☰",
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#0F172A"),
                    BackgroundColor = Colors.Transparent,
                    BorderWidth = 0,
                    Padding = 0,
                    HeightRequest = 40,
                    WidthRequest = 40,
                    Command = new Command(() => Shell.Current.FlyoutIsPresented = true)
                }
                .Column(0)
                .CenterVertical(),

                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label
                        {
                            Text = "Pago Móvil",
                            FontSize = 22,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A")
                        },
                        new Label
                        {
                            Text = "Datos de Pago Móvil",
                            FontSize = 13,
                            TextColor = Color.FromArgb("#64748B")
                        }
                    }
                }
                .Column(1)
                .CenterVertical(),

                // Botón de Registro
                new Button
                {
                    Text = "+ Agregar",
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    BackgroundColor = Color.FromArgb("#0F172A"),
                    CornerRadius = 15,
                    Padding = new Thickness(14, 0),
                    HeightRequest = 30,
                    FontSize = 11,
                    Command = new Command(async () => await Navigation.PushAsync(new PagoMovilFormPage(_viewModel)))
                }
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
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 12),
            Content = new Label
            {
                TextColor = Color.FromArgb("#475569"),
                FontSize = 12,
                LineBreakMode = LineBreakMode.WordWrap
            }
            .Bind(Label.TextProperty, nameof(PagoMovilViewModel.StatusMessage))
        }
        .Bind(Border.IsVisibleProperty, nameof(PagoMovilViewModel.StatusMessage), 
            convert: (string? msg) => !string.IsNullOrEmpty(msg));
    }

    private View CreateSearchBar()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"), // slate-200
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = new Thickness(12, 4),
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Auto, Star),
                ColumnSpacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = "🔍",
                        FontSize = 14,
                        VerticalOptions = LayoutOptions.Center,
                        TextColor = Color.FromArgb("#94A3B8") // slate-400
                    }
                    .Column(0),

                    new Entry
                    {
                        Placeholder = "Buscar por banco, cédula o teléfono...",
                        PlaceholderColor = Color.FromArgb("#94A3B8"),
                        TextColor = Color.FromArgb("#0F172A"),
                        BackgroundColor = Colors.Transparent,
                        HeightRequest = 40,
                        FontSize = 13,
                        ClearButtonVisibility = ClearButtonVisibility.WhileEditing
                    }
                    .Column(1)
                    .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.SearchQuery), BindingMode.TwoWay)
                }
            }
        };
    }

    private View CreateLoadingOverlay()
    {
        return new Grid
        {
            BackgroundColor = Color.FromArgb("#80FFFFFF"), // Blanco con 50% de opacidad para difuminar
            Children =
            {
                new ActivityIndicator
                {
                    IsRunning = true,
                    Color = Color.FromArgb("#0F172A"),
                    HeightRequest = 50,
                    WidthRequest = 50,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        }
        .Bind(Grid.IsVisibleProperty, nameof(PagoMovilViewModel.IsLoading));
    }

    private View CreateRecordsList()
    {
        return new CollectionView
        {
            ItemTemplate = new DataTemplate(() => CreateRecordCard()),
            EmptyView = new Border
            {
                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                Padding = new Thickness(24, 40),
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label
                        {
                            Text = "No hay registros guardados",
                            FontSize = 15,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A"),
                            HorizontalTextAlignment = TextAlignment.Center
                        },
                        new Label
                        {
                            Text = "Pulse '+ Agregar' en la esquina superior derecha para registrar sus datos de Pago Móvil.",
                            FontSize = 12,
                            TextColor = Color.FromArgb("#64748B"),
                            HorizontalTextAlignment = TextAlignment.Center,
                            LineBreakMode = LineBreakMode.WordWrap
                        }
                    }
                }
            }
        }
        .Bind(CollectionView.ItemsSourceProperty, nameof(PagoMovilViewModel.Records));
    }

    private View CreateRecordCard()
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"), // slate-200
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 12),
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    new Label
                    {
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#0F172A"),
                        FontSize = 13,
                        VerticalOptions = LayoutOptions.Center,
                        LineBreakMode = LineBreakMode.TailTruncation
                    }
                    .Bind(Label.TextProperty, nameof(PagoMovilRecord.DisplayName)),

                    new Label
                    {
                        Text = "→",
                        TextColor = Color.FromArgb("#94A3B8"),
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        VerticalOptions = LayoutOptions.Center
                    }
                    .Column(1)
                }
            }
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            if (border.BindingContext is PagoMovilRecord record)
            {
                await Navigation.PushAsync(new PagoMovilDetailPage(record, _viewModel));
            }
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }
}
