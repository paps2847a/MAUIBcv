using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Compras;

public class AddProductCardView : Border
{
    public AddProductCardView(ComprasViewModel viewModel, System.Func<Task>? onAddClickedAnimation = null)
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = 16;

        var addBtn = new Button
        {
            CornerRadius = 8,
            HeightRequest = 40,
            Command = viewModel.AddItemCommand
        }
        .Text("+ Agregar a la Lista")
        .TextColor(AppStyle.White)
        .BackgroundColor(AppStyle.Slate900)
        .Bold()
        .Margin(new Thickness(0, 4, 0, 0));

        addBtn.Clicked += async (s, e) =>
        {
            await AppStyle.AnimateClickAsync(addBtn);
            if (onAddClickedAnimation != null)
            {
                await onAddClickedAnimation();
            }
        };

        Content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label().Text("Agregar Producto").FontSize(14).Bold().TextColor(AppStyle.TextPrimary),
                
                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label().Text("Nombre del Producto (Opcional)").FontSize(11).TextColor(AppStyle.TextSecondary),
                        new Entry()
                            .Placeholder("Ej: Harina PAN, Leche (o dejar vacío)")
                            .PlaceholderColor(AppStyle.Slate400)
                            .TextColor(AppStyle.TextPrimary)
                            .BackgroundColor(AppStyle.InputBackground)
                            .Height(40)
                            .FontSize(13)
                            .Bind(Entry.TextProperty, nameof(ComprasViewModel.FormName), BindingMode.TwoWay)
                    }
                },

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
                                new Label().Text("Precio").FontSize(11).TextColor(AppStyle.TextSecondary),
                                new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 40 }
                                    .Placeholder("Ej: 1.50 o 60")
                                    .PlaceholderColor(AppStyle.Slate400)
                                    .TextColor(AppStyle.TextPrimary)
                                    .BackgroundColor(AppStyle.InputBackground)
                                    .FontSize(13)
                                    .Bind(Entry.TextProperty, nameof(ComprasViewModel.FormPrice), BindingMode.TwoWay)
                            }
                        }.Column(0),

                        new VerticalStackLayout
                        {
                            Spacing = 4,
                            Children =
                            {
                                new Label().Text("Cantidad").FontSize(11).TextColor(AppStyle.TextSecondary),
                                new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 40 }
                                    .Placeholder("1")
                                    .PlaceholderColor(AppStyle.Slate400)
                                    .TextColor(AppStyle.TextPrimary)
                                    .BackgroundColor(AppStyle.InputBackground)
                                    .FontSize(13)
                                    .Bind(Entry.TextProperty, nameof(ComprasViewModel.FormQuantityText), BindingMode.TwoWay)
                            }
                        }.Column(1)
                    }
                },

                new VerticalStackLayout
                {
                    Spacing = 4,
                    Children =
                    {
                        new Label().Text("Moneda del Precio").FontSize(11).TextColor(AppStyle.TextSecondary),
                        new Grid
                        {
                            ColumnDefinitions = Columns.Define(Star, Star, Star, Star),
                            ColumnSpacing = 6,
                            Children =
                            {
                                CreateCurrencyButton(viewModel, "USD", "USD", 0),
                                CreateCurrencyButton(viewModel, "VES", "VES", 1),
                                CreateCurrencyButton(viewModel, "EUR", "EUR", 2),
                                CreateCurrencyButton(viewModel, "USDT", "USDT", 3)
                            }
                        }
                    }
                },

                addBtn
            }
        };
    }

    private Button CreateCurrencyButton(ComprasViewModel viewModel, string label, string currencyCode, int columnIndex)
    {
        var btn = new Button
        {
            CornerRadius = 6,
            HeightRequest = 36,
            Command = viewModel.SelectFormCurrencyCommand,
            CommandParameter = currencyCode
        }
        .Text(label)
        .FontSize(11)
        .Bold()
        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.FormCurrency),
            convert: (string? c) => c == currencyCode ? AppStyle.Slate900 : AppStyle.InputBackground)
        .Bind(Button.TextColorProperty, nameof(ComprasViewModel.FormCurrency),
            convert: (string? c) => c == currencyCode ? AppStyle.White : AppStyle.TextSecondary)
        .Column(columnIndex);

        btn.Clicked += async (s, e) => await AppStyle.AnimateClickAsync(btn);

        return btn;
    }
}
