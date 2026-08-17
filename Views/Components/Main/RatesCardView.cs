using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Main;

public class RatesCardView : Grid
{
    public RatesCardView()
    {
        ColumnDefinitions = Columns.Define(Star, Star, Star);
        ColumnSpacing = 10;

        Children.Add(CreateRateCard("DOLAR (USD)", nameof(MainViewModel.UsdRate), "Banco Central", 0));
        Children.Add(CreateRateCard("EURO (EUR)", nameof(MainViewModel.EurRate), "Banco Central", 1));
        Children.Add(CreateRateCard("USDT (TETHER)", nameof(MainViewModel.UsdtRate), "Binance Crypto", 2));
    }

    private Border CreateRateCard(string currencyTitle, string bindingProperty, string providerName, int columnIndex)
    {
        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Padding = new Thickness(12, 14),
            Content = new VerticalStackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label().Text(currencyTitle).FontSize(10).Bold().TextColor(AppStyle.TextSecondary),
                    new Label().FontSize(17).Bold().TextColor(AppStyle.TextPrimary)
                        .Bind(Label.TextProperty, bindingProperty, stringFormat: "{0:N2} VES"),
                    new Label().Text(providerName).FontSize(8).TextColor(AppStyle.Slate400)
                }
            }
        }.Column(columnIndex);
    }
}
