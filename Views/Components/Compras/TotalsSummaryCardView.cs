using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Compras;

public class TotalsSummaryCardView : Border
{
    public TotalsSummaryCardView(ComprasViewModel viewModel, Page page)
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = 16;

        var completeBtn = new Button { CornerRadius = 8, HeightRequest = 42 }
            .Text("Guardar y Completar Compra")
            .TextColor(AppStyle.White)
            .BackgroundColor(AppStyle.SuccessGreen)
            .Bold();

        completeBtn.Clicked += async (s, e) =>
        {
            await AppStyle.AnimateClickAsync(completeBtn);
            if (viewModel.ShoppingItems.Count == 0)
            {
                await page.DisplayAlertAsync("Vacío", "No hay productos en la lista.", "Aceptar");
                return;
            }

            bool confirm = await page.DisplayAlertAsync("Confirmar", "¿Desea registrar esta compra y vaciar la lista activa?", "Registrar", "Cancelar");
            if (confirm)
            {
                viewModel.CompletePurchaseCommand.Execute(null);
            }
        };

        var copyPagoBtn = new Button
        {
            CornerRadius = 6,
            HeightRequest = 36,
            Command = viewModel.CopyPagoMovilCommand
        }
        .Text("Copiar")
        .TextColor(AppStyle.White)
        .BackgroundColor(AppStyle.Slate900)
        .Bold()
        .FontSize(11)
        .Column(1);

        copyPagoBtn.Clicked += async (s, e) => await AppStyle.AnimateClickAsync(copyPagoBtn);

        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                new Label().Text("Totales Estimados").FontSize(14).Bold().TextColor(AppStyle.TextPrimary),
                
                new Grid
                {
                    ColumnDefinitions = Columns.Define(Star, Star, Star, Star),
                    ColumnSpacing = 4,
                    Children =
                    {
                        CreateTotalCell("VES", nameof(ComprasViewModel.TotalVes), "{0:N2} Bs", 0),
                        CreateTotalCell("USD", nameof(ComprasViewModel.TotalUsd), "${0:N2}", 1),
                        CreateTotalCell("EUR", nameof(ComprasViewModel.TotalEur), "€{0:N2}", 2),
                        CreateTotalCell("USDT", nameof(ComprasViewModel.TotalUsdt), "$₮{0:N2}", 3)
                    }
                },

                new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label().Text("Copiar Pago Móvil rápido").FontSize(11).TextColor(AppStyle.TextSecondary),
                        new Grid
                        {
                            ColumnDefinitions = Columns.Define(Star, Auto),
                            ColumnSpacing = 8,
                            Children =
                            {
                                new Border
                                {
                                    StrokeShape = new RoundRectangle { CornerRadius = 6 },
                                    BackgroundColor = AppStyle.InputBackground,
                                    Content = new Picker
                                    {
                                        Title = "Seleccione contacto",
                                        TitleColor = AppStyle.Slate400,
                                        TextColor = AppStyle.TextPrimary,
                                        ItemDisplayBinding = new Binding(nameof(PagoMovilRecord.DisplayName)),
                                        HeightRequest = 36
                                    }
                                    .Bind(Picker.ItemsSourceProperty, nameof(ComprasViewModel.PagoMovilList))
                                    .Bind(Picker.SelectedItemProperty, nameof(ComprasViewModel.SelectedPagoMovil), BindingMode.TwoWay)
                                }
                                .Padding(new Thickness(8, 0)),

                                copyPagoBtn
                            }
                        }
                    }
                },

                completeBtn
            }
        };
    }

    private VerticalStackLayout CreateTotalCell(string title, string bindingProp, string stringFormat, int columnIndex)
    {
        return new VerticalStackLayout
        {
            Spacing = 2,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Label().Text(title).FontSize(9).Bold().TextColor(AppStyle.TextSecondary),
                new Label().FontSize(13).Bold().TextColor(AppStyle.TextPrimary)
                    .Bind(Label.TextProperty, bindingProp, stringFormat: stringFormat)
            }
        }.Column(columnIndex);
    }
}
