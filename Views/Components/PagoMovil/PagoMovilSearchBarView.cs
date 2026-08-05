using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.PagoMovil;

public class PagoMovilSearchBarView : Border
{
    public PagoMovilSearchBarView()
    {
        StrokeShape = new RoundRectangle { CornerRadius = 8 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = new Thickness(12, 4);

        Content = new Grid
        {
            ColumnDefinitions = Columns.Define(Auto, Star),
            ColumnSpacing = 8,
            Children =
            {
                new Label()
                    .Text("🔍")
                    .FontSize(14)
                    .TextColor(AppStyle.TextSecondary)
                    .CenterVertical()
                    .Column(0),

                new Entry { ClearButtonVisibility = ClearButtonVisibility.WhileEditing, HeightRequest = 40 }
                    .Placeholder("Buscar por banco, cédula o teléfono...")
                    .PlaceholderColor(AppStyle.Slate400)
                    .TextColor(AppStyle.TextPrimary)
                    .BackgroundColor(AppStyle.Transparent)
                    .FontSize(13)
                    .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.SearchQuery), BindingMode.TwoWay)
                    .Column(1)
            }
        };
    }
}
