using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views.Components;

public class StatusBannerView : Border
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(StatusBannerView), string.Empty, propertyChanged: (b, o, n) => 
        {
            var view = (StatusBannerView)b;
            string msg = (string)n;
            view._label.Text = msg;
            view.IsVisible = !string.IsNullOrEmpty(msg);
        });

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private readonly Label _label;

    public StatusBannerView()
    {
        StrokeShape = new RoundRectangle { CornerRadius = 8 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = new Thickness(16, 12);
        IsVisible = false;

        _label = new Label
        {
            LineBreakMode = LineBreakMode.WordWrap,
            TextColor = AppStyle.TextSecondary,
            FontSize = 12
        };

        Content = _label;
    }
}
