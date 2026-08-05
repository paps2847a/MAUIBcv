using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Main;

public class CurrencyConverterCardView : Border
{
    public CurrencyConverterCardView(MainViewModel viewModel)
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = 20;

        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                new Label()
                    .Text("Conversor de monedas")
                    .FontSize(14)
                    .Bold()
                    .TextColor(AppStyle.TextPrimary),

                new Grid
                {
                    ColumnDefinitions = Columns.Define(Star, Star, Star),
                    ColumnSpacing = 8,
                    Children =
                    {
                        CreateCurrencyButton(viewModel, "USD ($)", "USD", 0),
                        CreateCurrencyButton(viewModel, "EUR (€)", "EUR", 1),
                        CreateCurrencyButton(viewModel, "USDT ($₮)", "USDT", 2)
                    }
                },

                new Grid
                {
                    ColumnDefinitions = Columns.Define(Star, Auto, Auto),
                    ColumnSpacing = 10,
                    Children =
                    {
                        new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 42 }
                            .Placeholder("Ingrese monto")
                            .PlaceholderColor(AppStyle.Slate400)
                            .TextColor(AppStyle.TextPrimary)
                            .BackgroundColor(AppStyle.InputBackground)
                            .Bind(Entry.TextProperty, nameof(MainViewModel.AmountText), BindingMode.TwoWay)
                            .Column(0),

                        new Button { CornerRadius = 8, HeightRequest = 42, WidthRequest = 42, Command = viewModel.CopyAmountToConvertCommand }
                            .Text("⎘")
                            .FontSize(14)
                            .TextColor(AppStyle.TextSecondary)
                            .BackgroundColor(AppStyle.InputBackground)
                            .Column(1),

                        new Button { CornerRadius = 8, HeightRequest = 42, Command = viewModel.ToggleDirectionCommand }
                            .TextColor(AppStyle.White)
                            .BackgroundColor(AppStyle.Slate900)
                            .Bold()
                            .Bind(Button.TextProperty, nameof(MainViewModel.IsToVes),
                                convert: (bool toVes) => toVes ? "Divisa a VES" : "VES a Divisa")
                            .Column(2)
                    }
                },

                new Border
                {
                    StrokeShape = new RoundRectangle { CornerRadius = 8 },
                    BackgroundColor = AppStyle.InputBackground,
                    Padding = 12,
                    Content = new Grid
                    {
                        ColumnDefinitions = Columns.Define(Star, Auto),
                        Children =
                        {
                            new VerticalStackLayout
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                Spacing = 4,
                                Children =
                                {
                                    new Label().Text("RESULTADO ESTIMADO").FontSize(9).TextColor(AppStyle.TextSecondary).TextCenterHorizontal(),
                                    new Label()
                                        .FontSize(24)
                                        .Bold()
                                        .TextColor(AppStyle.TextPrimary)
                                        .Bind(Label.TextProperty, nameof(MainViewModel.ConversionResult))
                                }
                            }.Column(0),

                            new Button
                            {
                                BackgroundColor = AppStyle.Transparent,
                                HeightRequest = 40,
                                WidthRequest = 40,
                                Command = viewModel.CopyResultCommand
                            }
                            .Text("⎘")
                            .FontSize(16)
                            .TextColor(AppStyle.TextSecondary)
                            .Column(1)
                            .CenterVertical()
                        }
                    }
                }
            }
        };
    }

    private Button CreateCurrencyButton(MainViewModel viewModel, string label, string currencyCode, int columnIndex)
    {
        return new Button
        {
            CornerRadius = 8,
            HeightRequest = 40,
            Command = viewModel.SelectCurrencyCommand,
            CommandParameter = currencyCode
        }
        .Text(label)
        .FontSize(12)
        .Bold()
        .Bind(Button.BackgroundColorProperty, nameof(MainViewModel.SelectedCurrency),
            convert: (string? curr) => curr == currencyCode ? AppStyle.Slate900 : AppStyle.InputBackground)
        .Bind(Button.TextColorProperty, nameof(MainViewModel.SelectedCurrency),
            convert: (string? curr) => curr == currencyCode ? AppStyle.White : AppStyle.TextSecondary)
        .Column(columnIndex);
    }
}
