using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp;

public class PagoMovilDetailPage : ContentPage
{
    private readonly PagoMovilRecord _record;
    private readonly PagoMovilViewModel _viewModel;

    public PagoMovilDetailPage(PagoMovilRecord record, PagoMovilViewModel viewModel)
    {
        _record = record;
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
                            CreateDetailsCard()
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
                            Text = "Detalles",
                            FontSize = 22,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#0F172A")
                        },
                        new Label
                        {
                            Text = "Información de Pago Móvil",
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

    private View CreateDetailsCard()
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
                Spacing = 20,
                Children =
                {
                    // Banco
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Auto),
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    new Label { Text = "Banco Receptor", FontSize = 11, TextColor = Color.FromArgb("#64748B") },
                                    new Label { Text = $"{_record.BankCode} - {_record.BankName}", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                }
                            }
                            .Column(0),

                            new Button
                            {
                                Text = "⎘",
                                FontSize = 14,
                                TextColor = Color.FromArgb("#475569"),
                                BackgroundColor = Colors.Transparent,
                                HeightRequest = 36,
                                WidthRequest = 36,
                                Padding = 0,
                                Command = _viewModel.CopyFieldCommand,
                                CommandParameter = _record.BankName
                            }
                            .Column(1)
                            .CenterVertical()
                        }
                    },

                    // Cédula
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Auto),
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    new Label { Text = "Cédula de Identidad", FontSize = 11, TextColor = Color.FromArgb("#64748B") },
                                    new Label { Text = _record.Cedula, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                }
                            }
                            .Column(0),

                            new Button
                            {
                                Text = "⎘",
                                FontSize = 14,
                                TextColor = Color.FromArgb("#475569"),
                                BackgroundColor = Colors.Transparent,
                                HeightRequest = 36,
                                WidthRequest = 36,
                                Padding = 0,
                                Command = _viewModel.CopyFieldCommand,
                                CommandParameter = _record.Cedula
                            }
                            .Column(1)
                            .CenterVertical()
                        }
                    },

                    // Teléfono
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Auto),
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                Spacing = 4,
                                Children =
                                {
                                    new Label { Text = "Número de Teléfono", FontSize = 11, TextColor = Color.FromArgb("#64748B") },
                                    new Label { Text = _record.Phone, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A") }
                                }
                            }
                            .Column(0),

                            new Button
                            {
                                Text = "⎘",
                                FontSize = 14,
                                TextColor = Color.FromArgb("#475569"),
                                BackgroundColor = Colors.Transparent,
                                HeightRequest = 36,
                                WidthRequest = 36,
                                Padding = 0,
                                Command = _viewModel.CopyFieldCommand,
                                CommandParameter = _record.Phone
                            }
                            .Column(1)
                            .CenterVertical()
                        }
                    },

                    new BoxView { HeightRequest = 1, BackgroundColor = Color.FromArgb("#E2E8F0") },

                    // Acciones globales (Copiar todo, Compartir)
                    new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Star),
                        ColumnSpacing = 10,
                        Children =
                        {
                            new Button
                            {
                                Text = "Copiar Todo",
                                TextColor = Color.FromArgb("#0F172A"),
                                BackgroundColor = Color.FromArgb("#F1F5F9"),
                                FontSize = 13,
                                FontAttributes = FontAttributes.Bold,
                                CornerRadius = 6,
                                HeightRequest = 40,
                                Command = _viewModel.CopyAllCommand,
                                CommandParameter = _record
                            }
                            .Column(0),

                            new Button
                            {
                                Text = "Compartir",
                                TextColor = Colors.White,
                                BackgroundColor = Color.FromArgb("#0F172A"),
                                FontSize = 13,
                                FontAttributes = FontAttributes.Bold,
                                CornerRadius = 6,
                                HeightRequest = 40,
                                Command = _viewModel.ShareRecordCommand,
                                CommandParameter = _record
                            }
                            .Column(1)
                        }
                    },

                    // Botón Eliminar
                    new Button
                    {
                        Text = "Eliminar Registro",
                        TextColor = Color.FromArgb("#EF4444"),
                        BackgroundColor = Colors.Transparent,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 13,
                        HeightRequest = 40
                    }
                    .Invoke(btn => btn.Clicked += async (s, e) =>
                    {
                        bool confirm = await DisplayAlertAsync("Confirmar", "¿Desea eliminar este registro de Pago Móvil?", "Eliminar", "Cancelar");
                        if (confirm)
                        {
                            _viewModel.DeleteRecordCommand.Execute(_record);
                            await Navigation.PopAsync();
                        }
                    })
                }
            }
        };
    }
}
