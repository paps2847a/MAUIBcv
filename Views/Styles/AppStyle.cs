using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace BcvExchangeApp.Views.Styles;

public static class AppStyle
{
    // Colores Principales Slate (Tema Claro Higiénico y Elegante)
    public static readonly Color Slate900 = Color.FromArgb("#0F172A");
    public static readonly Color Slate800 = Color.FromArgb("#1E293B");
    public static readonly Color Slate700 = Color.FromArgb("#334155");
    public static readonly Color Slate600 = Color.FromArgb("#475569");
    public static readonly Color Slate500 = Color.FromArgb("#64748B");
    public static readonly Color Slate400 = Color.FromArgb("#94A3B8");
    public static readonly Color Slate300 = Color.FromArgb("#CBD5E1");
    public static readonly Color Slate200 = Color.FromArgb("#E2E8F0");
    public static readonly Color Slate100 = Color.FromArgb("#F1F5F9");
    public static readonly Color Slate50  = Color.FromArgb("#F8FAFC");

    // Colores Funcionales
    public static readonly Color SuccessGreen = Color.FromArgb("#16A34A");
    public static readonly Color DangerRed    = Color.FromArgb("#EF4444");
    public static readonly Color AccentBlue   = Color.FromArgb("#2563EB");
    public static readonly Color MutedGray    = Color.FromArgb("#94A3B8");
    public static readonly Color White        = Colors.White;
    public static readonly Color Transparent  = Colors.Transparent;

    // Propiedades de la App (Tema Principal Limpio y Legible)
    public static Color PageBackground => Slate50;       // #F8FAFC
    public static Color CardBackground => White;         // #FFFFFF
    public static Color InputBackground => Slate100;     // #F1F5F9
    public static Color BorderColor => Slate200;        // #E2E8F0
    public static Color TextPrimary => Slate900;        // #0F172A (Texto Oscuro Legible)
    public static Color TextSecondary => Slate600;      // #475569

    // Menú Flyout (Leftbar)
    public static Color FlyoutBackground => White;        // Fondo blanco
    public static Color FlyoutHeaderBackground => Slate900; // Encabezado azul oscuro slate
    public static Color FlyoutHeaderTitleColor => White;  // Texto encabezado blanco
    public static Color FlyoutItemText => Slate900;       // Texto de opciones AZUL OSCURO / NEGRO NÍTIDO
    public static Color FlyoutItemTextSelected => White;
    public static Color FlyoutItemSelectedBg => Slate900;

    // Animaciones Reutilizables
    public static async Task AnimateClickAsync(View? view)
    {
        if (view == null) return;
        await view.ScaleToAsync(0.94, 70, Easing.CubicOut);
        await view.ScaleToAsync(1.0, 70, Easing.CubicIn);
    }

    public static async Task FlashCardAsync(Border? card)
    {
        if (card == null) return;
        var originalBg = card.BackgroundColor;
        await card.ScaleToAsync(1.02, 90, Easing.CubicOut);
        card.BackgroundColor = Slate100;
        await card.ScaleToAsync(1.0, 90, Easing.CubicIn);
        card.BackgroundColor = originalBg;
    }
}
