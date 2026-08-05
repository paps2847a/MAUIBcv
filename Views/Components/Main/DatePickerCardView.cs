using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views.Components.Main;

public class DatePickerCardView : Border
{
    public DatePickerCardView()
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = 16;

        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                new Label()
                    .Text("Consultar fecha anterior")
                    .FontSize(13)
                    .Bold()
                    .TextColor(AppStyle.TextPrimary),

                new DatePicker { Format = "dd/MM/yyyy", MaximumDate = DateTime.Today }
                    .TextColor(AppStyle.TextPrimary)
                    .BackgroundColor(AppStyle.InputBackground)
                    .Bind(DatePicker.DateProperty, nameof(MainViewModel.SelectedDate), BindingMode.TwoWay)
            }
        };
    }
}
