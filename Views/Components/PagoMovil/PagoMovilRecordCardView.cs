using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.PagoMovil;

public static class PagoMovilRecordCardView
{
    public static View CreateCard(PagoMovilViewModel viewModel)
    {
        var border = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Stroke = AppStyle.BorderColor,
            StrokeThickness = 1,
            BackgroundColor = AppStyle.CardBackground,
            Padding = new Thickness(16, 14),
            Margin = new Thickness(0, 0, 0, 10),
            Content = new Grid
            {
                ColumnDefinitions = Columns.Define(Star, Auto),
                ColumnSpacing = 8,
                Children =
                {
                    new Label { LineBreakMode = LineBreakMode.TailTruncation }
                        .Bold()
                        .TextColor(AppStyle.TextPrimary)
                        .FontSize(13)
                        .CenterVertical()
                        .Bind(Label.TextProperty, nameof(PagoMovilRecord.DisplayName)),

                    new Label()
                        .Text("→")
                        .TextColor(AppStyle.TextSecondary)
                        .FontSize(16)
                        .Bold()
                        .CenterVertical()
                        .Column(1)
                }
            }
        };

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) =>
        {
            await AppStyle.AnimateClickAsync(border);
            if (border.BindingContext is PagoMovilRecord record)
            {
                await Shell.Current.Navigation.PushAsync(new PagoMovilDetailPage(record, viewModel));
            }
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }
}
