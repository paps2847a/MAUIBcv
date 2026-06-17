using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp;

public class PagoMovilFormPage : ContentPage
{
    private readonly PagoMovilViewModel _viewModel;

    public PagoMovilFormPage(PagoMovilViewModel viewModel)
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
                            CreateFormCard()
                        }
                    }
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
                // Botón Atrás
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
                    WidthRequest = 40,
                    Command = new Command(async () => await Navigation.PopAsync())
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
                            Text = "Registro",
                            FontSize = 22,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A")
                        },
                        new Label
                        {
                            Text = "Nuevo Pago Móvil",
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

    private View CreateFormCard()
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 6 },
            Stroke = Color.FromArgb("#E2E8F0"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            Padding = 20,
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    // Mensaje de Error
                    new Label
                    {
                        TextColor = Color.FromArgb("#EF4444"),
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                    .Bind(Label.TextProperty, nameof(PagoMovilViewModel.FormErrorMessage))
                    .Bind(Label.IsVisibleProperty, nameof(PagoMovilViewModel.FormErrorMessage),
                        convert: (string? err) => !string.IsNullOrEmpty(err)),

                    // Campo Cédula
                    new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = "Cédula de Identidad", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#475569") },
                            new Entry
                            {
                                Placeholder = "Ej: 28127336",
                                PlaceholderColor = Color.FromArgb("#94A3B8"),
                                TextColor = Color.FromArgb("#0F172A"),
                                Keyboard = Keyboard.Numeric,
                                BackgroundColor = Color.FromArgb("#F1F5F9"),
                                HeightRequest = 42
                            }
                            .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.FormCedula), BindingMode.TwoWay)
                        }
                    },

                    // Campo Teléfono
                    new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = "Número de Teléfono", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#475569") },
                            new Entry
                            {
                                Placeholder = "Ej: 04121234567",
                                PlaceholderColor = Color.FromArgb("#94A3B8"),
                                TextColor = Color.FromArgb("#0F172A"),
                                Keyboard = Keyboard.Telephone,
                                BackgroundColor = Color.FromArgb("#F1F5F9"),
                                HeightRequest = 42
                            }
                            .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.FormPhone), BindingMode.TwoWay)
                        }
                    },

                    // Campo Banco
                    new VerticalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new Label { Text = "Banco Receptor", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#475569") },
                            new Border
                            {
                                StrokeShape = new RoundRectangle { CornerRadius = 6 },
                                BackgroundColor = Color.FromArgb("#F1F5F9"),
                                Padding = new Thickness(10, 0),
                                Content = new Picker
                                {
                                    Title = "Seleccione un banco",
                                    TitleColor = Color.FromArgb("#94A3B8"),
                                    TextColor = Color.FromArgb("#0F172A"),
                                    ItemDisplayBinding = new Binding(nameof(Bank.DisplayName))
                                }
                                .Bind(Picker.ItemsSourceProperty, nameof(PagoMovilViewModel.Banks))
                                .Bind(Picker.SelectedItemProperty, nameof(PagoMovilViewModel.FormSelectedBank), BindingMode.TwoWay)
                            }
                        }
                    },

                    // Botón Guardar
                    new Button
                    {
                        Text = "Guardar Registro",
                        TextColor = Colors.White,
                        BackgroundColor = Color.FromArgb("#0F172A"),
                        FontAttributes = FontAttributes.Bold,
                        HeightRequest = 45,
                        CornerRadius = 6,
                        Command = _viewModel.AddRecordCommand,
                        CommandParameter = Navigation
                    }
                }
            }
        };
    }
}
