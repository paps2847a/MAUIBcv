using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views;

public class PagoMovilPage : ContentPage
{
    private readonly PagoMovilViewModel _viewModel;

    public PagoMovilPage(PagoMovilViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        this.BackgroundColor(Color.FromArgb("#F8FAFC")); // slate-50

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
        // // Cargar registros al entrar con retraso para dar tiempo a que termine la animación de transición
        // Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
        // {
        //     Task.Run(async () => await _viewModel.InitializeAsync());
        // });
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
                            .Text("Pago Móvil")
                            .FontSize(22)
                            .Bold()
                            .TextColor(Color.FromArgb("#0F172A")),
                        new Label()
                            .Text("Datos de Pago Móvil")
                            .FontSize(13)
                            .TextColor(Color.FromArgb("#64748B"))
                    }
                }
                .Column(1)
                .CenterVertical(),

                // Botón de Registro
                new Button
                    {
                        TextColor = Colors.White,
                        BackgroundColor = Color.FromArgb("#0F172A"),
                        CornerRadius = 15,
                        Padding = new Thickness(14, 0),
                        HeightRequest = 30
                    }
                    .Text("+ Agregar")
                    .Bold()
                    .FontSize(11)
                    .Invoke(btn => btn.Clicked += async (s, e) => await Navigation.PushAsync(new PagoMovilFormPage(_viewModel)))
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
            Content = new Label { LineBreakMode = LineBreakMode.WordWrap }
                .TextColor(Color.FromArgb("#475569"))
                .FontSize(12)
                .Bind(Label.TextProperty, nameof(PagoMovilViewModel.StatusMessage))
        }
        .Padding(new Thickness(16, 12))
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
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Auto, Star),
                ColumnSpacing = 8,
                Children =
                {
                    new Label()
                        .Text("🔍")
                        .FontSize(14)
                        .TextColor(Color.FromArgb("#94A3B8")) // slate-400
                        .CenterVertical()
                        .Column(0),

                    new Entry { ClearButtonVisibility = ClearButtonVisibility.WhileEditing, HeightRequest = 40 }
                        .Placeholder("Buscar por banco, cédula o teléfono...")
                        .PlaceholderColor(Color.FromArgb("#94A3B8"))
                        .TextColor(Color.FromArgb("#0F172A"))
                        .BackgroundColor(Colors.Transparent)
                        .FontSize(13)
                        .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.SearchQuery), BindingMode.TwoWay)
                        .Column(1)
                }
            }
        }
        .Padding(new Thickness(12, 4));
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
                    Color = Color.FromArgb("#0F172A"),
                    HeightRequest = 50,
                    WidthRequest = 50,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
                .Bind(ActivityIndicator.IsRunningProperty, nameof(PagoMovilViewModel.IsLoading))
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
                Content = new VerticalStackLayout
                {
                    Spacing = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label()
                            .Text("No hay registros guardados")
                            .FontSize(15)
                            .Bold()
                            .TextColor(Color.FromArgb("#0F172A"))
                            .TextCenterHorizontal(),
                        new Label { LineBreakMode = LineBreakMode.WordWrap }
                            .Text("Pulse '+ Agregar' en la esquina superior derecha para registrar sus datos de Pago Móvil.")
                            .FontSize(12)
                            .TextColor(Color.FromArgb("#64748B"))
                            .TextCenterHorizontal()
                    }
                }
            }
            .Padding(new Thickness(24, 40))
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
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    new Label { LineBreakMode = LineBreakMode.TailTruncation }
                        .Bold()
                        .TextColor(Color.FromArgb("#0F172A"))
                        .FontSize(13)
                        .CenterVertical()
                        .Bind(Label.TextProperty, nameof(PagoMovilRecord.DisplayName)),

                    new Label()
                        .Text("→")
                        .TextColor(Color.FromArgb("#94A3B8"))
                        .FontSize(16)
                        .Bold()
                        .CenterVertical()
                        .Column(1)
                }
            }
        }
        .Padding(new Thickness(16, 14))
        .Margin(new Thickness(0, 0, 0, 12));

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
