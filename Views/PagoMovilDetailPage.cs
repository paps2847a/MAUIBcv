using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Models;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views;

public class PagoMovilDetailPage : ContentPage
{
    private readonly PagoMovilRecord _record;
    private readonly PagoMovilViewModel _viewModel;

    public PagoMovilDetailPage(PagoMovilRecord record, PagoMovilViewModel viewModel)
    {
        _record = record;
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
                        BackgroundColor = Colors.Transparent,
                        BorderWidth = 0,
                        Padding = 0,
                        HeightRequest = 40,
                        WidthRequest = 40
                    }
                    .Text("←")
                    .FontSize(20)
                    .Bold()
                    .TextColor(Color.FromArgb("#0F172A"))
                    .Invoke(btn => btn.Clicked += async (s, e) => 
                    {
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
                        new Label()
                            .Text("Detalles")
                            .FontSize(22)
                            .Bold()
                            .TextColor(Color.FromArgb("#0F172A")),
                        new Label()
                            .Text("Información de Pago Móvil")
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
                    .Bind(Label.TextProperty, nameof(PagoMovilViewModel.StatusMessage))
            }
            .Padding(new Thickness(16, 12))
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
                                    new Label().Text("Banco Receptor").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                                    new Label().Text($"{_record.BankCode} - {_record.BankName}").FontSize(14).Bold().TextColor(Color.FromArgb("#0F172A"))
                                }
                            }
                            .Column(0),

                            new Button
                                {
                                    BackgroundColor = Colors.Transparent,
                                    HeightRequest = 36,
                                    WidthRequest = 36,
                                    Padding = 0,
                                    Command = _viewModel.CopyFieldCommand,
                                    CommandParameter = _record.BankName
                                }
                                .Text("⎘")
                                .FontSize(14)
                                .TextColor(Color.FromArgb("#475569"))
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
                                    new Label().Text("Cédula de Identidad").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                                    new Label().Text(_record.Cedula).FontSize(14).Bold().TextColor(Color.FromArgb("#0F172A"))
                                }
                            }
                            .Column(0),

                            new Button
                                {
                                    BackgroundColor = Colors.Transparent,
                                    HeightRequest = 36,
                                    WidthRequest = 36,
                                    Padding = 0,
                                    Command = _viewModel.CopyFieldCommand,
                                    CommandParameter = _record.Cedula
                                }
                                .Text("⎘")
                                .FontSize(14)
                                .TextColor(Color.FromArgb("#475569"))
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
                                    new Label().Text("Número de Teléfono").FontSize(11).TextColor(Color.FromArgb("#64748B")),
                                    new Label().Text(_record.Phone).FontSize(14).Bold().TextColor(Color.FromArgb("#0F172A"))
                                }
                            }
                            .Column(0),

                            new Button
                                {
                                    BackgroundColor = Colors.Transparent,
                                    HeightRequest = 36,
                                    WidthRequest = 36,
                                    Padding = 0,
                                    Command = _viewModel.CopyFieldCommand,
                                    CommandParameter = _record.Phone
                                }
                                .Text("⎘")
                                .FontSize(14)
                                .TextColor(Color.FromArgb("#475569"))
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
                                    TextColor = Color.FromArgb("#0F172A"),
                                    BackgroundColor = Color.FromArgb("#F1F5F9"),
                                    CornerRadius = 6,
                                    HeightRequest = 40,
                                    Command = _viewModel.CopyAllCommand,
                                    CommandParameter = _record
                                }
                                .Text("Copiar Todo")
                                .FontSize(13)
                                .Bold()
                                .Column(0),

                            new Button
                                {
                                    TextColor = Colors.White,
                                    BackgroundColor = Color.FromArgb("#0F172A"),
                                    CornerRadius = 6,
                                    HeightRequest = 40,
                                    Command = _viewModel.ShareRecordCommand,
                                    CommandParameter = _record
                                }
                                .Text("Compartir")
                                .FontSize(13)
                                .Bold()
                                .Column(1)
                        }
                    },

                    // Botón Eliminar
                    new Button
                        {
                            TextColor = Color.FromArgb("#EF4444"),
                            BackgroundColor = Colors.Transparent,
                            HeightRequest = 40
                        }
                        .Text("Eliminar Registro")
                        .Bold()
                        .FontSize(13)
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
        }
        .Padding(20);
    }
}
