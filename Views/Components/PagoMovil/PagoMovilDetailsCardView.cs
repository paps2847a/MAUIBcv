using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.PagoMovil;

public class PagoMovilDetailsCardView : Border
{
    public PagoMovilDetailsCardView(PagoMovilRecord record, PagoMovilViewModel viewModel, Page page)
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = 20;

        Content = new VerticalStackLayout
        {
            Spacing = 20,
            Children =
            {
                CreateDetailRow("Banco Receptor", $"{record.BankCode} - {record.BankName}", viewModel.CopyFieldCommand, record.BankName),
                CreateDetailRow("Cédula de Identidad", record.Cedula, viewModel.CopyFieldCommand, record.Cedula),
                CreateDetailRow("Número de Teléfono", record.Phone, viewModel.CopyFieldCommand, record.Phone),

                new BoxView { HeightRequest = 1, BackgroundColor = AppStyle.BorderColor },

                new Grid
                {
                    ColumnDefinitions = Columns.Define(Star, Star),
                    ColumnSpacing = 10,
                    Children =
                    {
                        new Button
                        {
                            TextColor = AppStyle.TextPrimary,
                            BackgroundColor = AppStyle.InputBackground,
                            CornerRadius = 8,
                            HeightRequest = 40,
                            Command = viewModel.CopyAllCommand,
                            CommandParameter = record
                        }
                        .Text("Copiar Todo")
                        .FontSize(13)
                        .Bold()
                        .Column(0),

                        new Button
                        {
                            TextColor = AppStyle.White,
                            BackgroundColor = AppStyle.Slate900,
                            CornerRadius = 8,
                            HeightRequest = 40,
                            Command = viewModel.ShareRecordCommand,
                            CommandParameter = record
                        }
                        .Text("Compartir")
                        .FontSize(13)
                        .Bold()
                        .Column(1)
                    }
                },

                new Button
                {
                    TextColor = AppStyle.DangerRed,
                    BackgroundColor = AppStyle.Transparent,
                    HeightRequest = 40
                }
                .Text("Eliminar Registro")
                .Bold()
                .FontSize(13)
                .Invoke(btn => btn.Clicked += async (s, e) =>
                {
                    bool confirm = await page.DisplayAlertAsync("Confirmar", "¿Desea eliminar este registro de Pago Móvil?", "Eliminar", "Cancelar");
                    if (confirm)
                    {
                        viewModel.DeleteRecordCommand.Execute(record);
                        await page.Navigation.PopAsync();
                    }
                })
            }
        };
    }

    private Grid CreateDetailRow(string title, string value, System.Windows.Input.ICommand copyCommand, object copyParameter)
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
                        new Label().Text(title).FontSize(11).TextColor(AppStyle.TextSecondary),
                        new Label().Text(value).FontSize(14).Bold().TextColor(AppStyle.TextPrimary)
                    }
                }.Column(0),

                new Button
                {
                    BackgroundColor = AppStyle.Transparent,
                    HeightRequest = 36,
                    WidthRequest = 36,
                    Padding = 0,
                    Command = copyCommand,
                    CommandParameter = copyParameter
                }
                .Text("⎘")
                .FontSize(14)
                .TextColor(AppStyle.TextSecondary)
                .Column(1)
                .CenterVertical()
            }
        };
    }
}
