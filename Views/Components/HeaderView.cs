using System;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components;

public class HeaderView : Grid
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(HeaderView), string.Empty, propertyChanged: (b, o, n) => ((HeaderView)b)._titleLabel.Text = (string)n);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(HeaderView), string.Empty, propertyChanged: (b, o, n) => ((HeaderView)b)._subtitleLabel.Text = (string)n);

    public static readonly BindableProperty IsBackButtonProperty =
        BindableProperty.Create(nameof(IsBackButton), typeof(bool), typeof(HeaderView), false, propertyChanged: (b, o, n) => ((HeaderView)b)._navButton.Text = (bool)n ? "←" : "☰");

    public static readonly BindableProperty ActionTextProperty =
        BindableProperty.Create(nameof(ActionText), typeof(string), typeof(HeaderView), string.Empty, propertyChanged: (b, o, n) => 
        {
            var view = (HeaderView)b;
            string val = (string)n;
            view._actionButton.Text = val;
            view._actionButton.IsVisible = !string.IsNullOrEmpty(val);
        });

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(nameof(ActionCommand), typeof(ICommand), typeof(HeaderView), null, propertyChanged: (b, o, n) => ((HeaderView)b)._actionButton.Command = (ICommand)n);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool IsBackButton
    {
        get => (bool)GetValue(IsBackButtonProperty);
        set => SetValue(IsBackButtonProperty, value);
    }

    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    private readonly Button _navButton;
    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;
    private readonly Button _actionButton;

    public HeaderView()
    {
        ColumnDefinitions = Columns.Define(Auto, Star, Auto);
        ColumnSpacing = 12;

        _navButton = new Button
        {
            BackgroundColor = AppStyle.Transparent,
            BorderWidth = 0,
            Padding = 0,
            HeightRequest = 40,
            WidthRequest = 40,
            Text = "☰",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = AppStyle.TextPrimary
        }.Column(0).CenterVertical();

        _navButton.Clicked += async (s, e) =>
        {
            await AppStyle.AnimateClickAsync(_navButton);
            if (IsBackButton)
            {
                await Shell.Current.Navigation.PopAsync();
            }
            else
            {
                Shell.Current.FlyoutIsPresented = true;
            }
        };

        _titleLabel = new Label()
            .FontSize(22)
            .Bold()
            .TextColor(AppStyle.TextPrimary);

        _subtitleLabel = new Label()
            .FontSize(13)
            .TextColor(AppStyle.TextSecondary);

        var titleLayout = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { _titleLabel, _subtitleLabel }
        }.Column(1).CenterVertical();

        _actionButton = new Button
        {
            TextColor = AppStyle.White,
            BackgroundColor = AppStyle.Slate900,
            CornerRadius = 15,
            Padding = new Thickness(14, 0),
            HeightRequest = 30,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            IsVisible = false
        }.Column(2).CenterVertical();

        _actionButton.Clicked += async (s, e) => await AppStyle.AnimateClickAsync(_actionButton);

        Children.Add(_navButton);
        Children.Add(titleLayout);
        Children.Add(_actionButton);
    }
}
