using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views.Components;

public class LoadingOverlayView : Grid
{
    public static readonly BindableProperty IsBusyProperty =
        BindableProperty.Create(nameof(IsBusy), typeof(bool), typeof(LoadingOverlayView), false, propertyChanged: (b, o, n) => 
        {
            var view = (LoadingOverlayView)b;
            bool busy = (bool)n;
            view.IsVisible = busy;
            view._activityIndicator.IsRunning = busy;
        });

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(LoadingOverlayView), string.Empty, propertyChanged: (b, o, n) => 
        {
            var view = (LoadingOverlayView)b;
            string msg = (string)n;
            view._messageLabel.Text = msg;
            view._messageLabel.IsVisible = !string.IsNullOrEmpty(msg);
        });

    public bool IsBusy
    {
        get => (bool)GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private readonly ActivityIndicator _activityIndicator;
    private readonly Label _messageLabel;

    public LoadingOverlayView()
    {
        BackgroundColor = Color.FromRgba(15, 23, 42, 100);
        InputTransparent = false;
        IsVisible = false;
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;

        _activityIndicator = new ActivityIndicator
        {
            Color = AppStyle.White,
            HeightRequest = 44,
            WidthRequest = 44,
            HorizontalOptions = LayoutOptions.Center
        };

        _messageLabel = new Label()
            .TextColor(AppStyle.White)
            .FontSize(13)
            .Bold()
            .TextCenterHorizontal();

        var container = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 14 },
            BackgroundColor = AppStyle.Slate900,
            Padding = new Thickness(24, 20),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Children = { _activityIndicator, _messageLabel }
            }
        };

        Children.Add(container);
    }
}
