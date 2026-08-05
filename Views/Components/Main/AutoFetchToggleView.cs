using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Main;

public class AutoFetchToggleView : Border
{
    public AutoFetchToggleView()
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = new Thickness(16, 10);

        var toggleSwitch = new Switch
        {
            OnColor = AppStyle.Slate900,
            ThumbColor = AppStyle.White
        }
        .Bind(Switch.IsToggledProperty, nameof(MainViewModel.AutoFetchOnStartup), BindingMode.TwoWay)
        .Column(1)
        .CenterVertical();

        Content = new Grid
        {
            ColumnDefinitions = Columns.Define(Star, Auto),
            Children =
            {
                new VerticalStackLayout
                {
                    Spacing = 2,
                    VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Label()
                            .Text("Auto-consultar al abrir la app")
                            .FontSize(13)
                            .Bold()
                            .TextColor(AppStyle.TextPrimary),
                        new Label()
                            .Text("Descarga automáticamente las tasas al iniciar")
                            .FontSize(11)
                            .TextColor(AppStyle.TextSecondary)
                    }
                }.Column(0),

                toggleSwitch
            }
        };
    }
}
