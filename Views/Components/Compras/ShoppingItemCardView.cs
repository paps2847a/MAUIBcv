using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Compras;

public static class ShoppingItemCardView
{
    public static View CreateCard(ComprasViewModel viewModel, System.Func<Task>? onItemDeletedAnimation = null)
    {
        var deleteBtn = new Button
        {
            TextColor = AppStyle.DangerRed,
            BackgroundColor = AppStyle.Transparent,
            HeightRequest = 32,
            WidthRequest = 32,
            Padding = 0
        }
        .Text("✕")
        .Bold()
        .FontSize(13)
        .Column(2)
        .CenterVertical();

        deleteBtn.Clicked += async (s, e) =>
        {
            await AppStyle.AnimateClickAsync(deleteBtn);
            if (deleteBtn.BindingContext is ShoppingItem item)
            {
                viewModel.DeleteItemCommand.Execute(item);
                if (onItemDeletedAnimation != null)
                {
                    await onItemDeletedAnimation();
                }
            }
        };

        return new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Padding = new Thickness(12, 10),
            Margin = new Thickness(0, 0, 0, 8),
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        Children =
                        {
                            new Label { LineBreakMode = LineBreakMode.TailTruncation }
                                .Bold()
                                .TextColor(AppStyle.TextPrimary)
                                .FontSize(13)
                                .Bind(Label.TextProperty, nameof(ShoppingItem.Name)),

                            new Label()
                                .TextColor(AppStyle.TextSecondary)
                                .FontSize(11)
                                .Bind(Label.TextProperty, nameof(ShoppingItem.Quantity), stringFormat: "Cantidad: {0}")
                        }
                    }.Column(0),

                    new VerticalStackLayout
                    {
                        Spacing = 2,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        Children =
                        {
                            new Label()
                                .Bold()
                                .TextColor(AppStyle.TextPrimary)
                                .FontSize(13)
                                .Bind(Label.TextProperty, convert: (ShoppingItem? item) => 
                                {
                                    if (item == null) return string.Empty;
                                    string symbol = item.Currency == "USD" ? "$" : item.Currency == "EUR" ? "€" : "Bs";
                                    return $"{item.TotalPrice:N2} {symbol}";
                                }),

                            new Label()
                                .TextColor(AppStyle.Slate400)
                                .FontSize(10)
                                .Bind(Label.TextProperty, convert: (ShoppingItem? item) =>
                                {
                                    if (item == null) return string.Empty;
                                    string symbol = item.Currency == "USD" ? "$" : item.Currency == "EUR" ? "€" : "Bs";
                                    return $"{item.Quantity}x {item.Price:N2} {symbol}";
                                })
                        }
                    }
                    .Column(1)
                    .Margin(new Thickness(0, 0, 8, 0)),

                    deleteBtn
                }
            }
        };
    }
}
