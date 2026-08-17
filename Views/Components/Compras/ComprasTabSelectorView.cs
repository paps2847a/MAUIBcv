using Microsoft.Maui;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace BcvExchangeApp.Views.Components.Compras;

public class ComprasTabSelectorView : Grid
{
    public ComprasTabSelectorView(ComprasViewModel viewModel)
    {
        ColumnDefinitions = Columns.Define(Star, Star);
        ColumnSpacing = 10;

        Children.Add(CreateTabButton(viewModel, "Lista Activa", "List", 0));
        Children.Add(CreateTabButton(viewModel, "Historial", "History", 1));
    }

    private Button CreateTabButton(ComprasViewModel viewModel, string label, string tabKey, int columnIndex)
    {
        var btn = new Button
        {
            CornerRadius = 8,
            HeightRequest = 40,
            Command = viewModel.SwitchTabCommand,
            CommandParameter = tabKey
        }
        .Text(label)
        .Bold()
        .Bind(Button.BackgroundColorProperty, nameof(ComprasViewModel.ActiveTab),
            convert: (string? tab) => tab == tabKey ? AppStyle.Slate900 : AppStyle.InputBackground)
        .Bind(Button.TextColorProperty, nameof(ComprasViewModel.ActiveTab),
            convert: (string? tab) => tab == tabKey ? AppStyle.White : AppStyle.TextSecondary)
        .Column(columnIndex);

        btn.Clicked += async (s, e) => await AppStyle.AnimateClickAsync(btn);

        return btn;
    }
}
