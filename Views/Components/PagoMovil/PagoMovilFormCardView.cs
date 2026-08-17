using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using CommunityToolkit.Maui.Markup;
using BcvExchangeApp.Models;
using BcvExchangeApp.ViewModels;
using BcvExchangeApp.Views.Styles;

namespace BcvExchangeApp.Views.Components.PagoMovil;

public class PagoMovilFormCardView : Border
{
    public PagoMovilFormCardView(PagoMovilViewModel viewModel, INavigation navigation)
    {
        StrokeShape = new RoundRectangle { CornerRadius = 10 };
        Stroke = AppStyle.BorderColor;
        StrokeThickness = 1;
        BackgroundColor = AppStyle.CardBackground;
        Padding = 20;

        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                new Label()
                    .TextColor(AppStyle.DangerRed)
                    .FontSize(12)
                    .Bold()
                    .TextCenterHorizontal()
                    .Bind(Label.TextProperty, nameof(PagoMovilViewModel.FormErrorMessage))
                    .Bind(Label.IsVisibleProperty, nameof(PagoMovilViewModel.FormErrorMessage),
                        convert: (string? err) => !string.IsNullOrEmpty(err)),

                new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label().Text("Cédula de Identidad").FontSize(12).Bold().TextColor(AppStyle.TextSecondary),
                        new Entry { Keyboard = Keyboard.Numeric, HeightRequest = 42 }
                            .Placeholder("Ej: 28127336")
                            .PlaceholderColor(AppStyle.Slate400)
                            .TextColor(AppStyle.TextPrimary)
                            .BackgroundColor(AppStyle.InputBackground)
                            .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.FormCedula), BindingMode.TwoWay)
                    }
                },

                new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label().Text("Número de Teléfono").FontSize(12).Bold().TextColor(AppStyle.TextSecondary),
                        new Entry { Keyboard = Keyboard.Telephone, HeightRequest = 42 }
                            .Placeholder("Ej: 04121234567")
                            .PlaceholderColor(AppStyle.Slate400)
                            .TextColor(AppStyle.TextPrimary)
                            .BackgroundColor(AppStyle.InputBackground)
                            .Bind(Entry.TextProperty, nameof(PagoMovilViewModel.FormPhone), BindingMode.TwoWay)
                    }
                },

                new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Label().Text("Banco Receptor").FontSize(12).Bold().TextColor(AppStyle.TextSecondary),
                        new Border
                        {
                            StrokeShape = new RoundRectangle { CornerRadius = 8 },
                            BackgroundColor = AppStyle.InputBackground,
                            Content = new Picker
                            {
                                Title = "Seleccione un banco",
                                TitleColor = AppStyle.Slate400,
                                TextColor = AppStyle.TextPrimary,
                                ItemDisplayBinding = new Binding(nameof(Bank.DisplayName))
                            }
                            .Bind(Picker.ItemsSourceProperty, nameof(PagoMovilViewModel.Banks))
                            .Bind(Picker.SelectedItemProperty, nameof(PagoMovilViewModel.FormSelectedBank), BindingMode.TwoWay)
                        }
                        .Padding(new Thickness(10, 0))
                    }
                },

                new Button 
                { 
                    CornerRadius = 8, 
                    HeightRequest = 45,
                    Command = viewModel.AddRecordCommand,
                    CommandParameter = navigation
                }
                .Text("Guardar Registro")
                .TextColor(AppStyle.White)
                .BackgroundColor(AppStyle.Slate900)
                .Bold()
            }
        };
    }
}
