using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using BcvExchangeApp.Data;
using BcvExchangeApp.Models;
using BcvExchangeApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BcvExchangeApp.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly BcvScraperService _scraperService;
    private readonly IServiceProvider _serviceProvider;

    private readonly SemaphoreSlim _dbSemaphore = new(1, 1);
    private bool _isInitialized = false;
    private readonly object _initLock = new();

    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double UsdRate { get; set; }

    [ObservableProperty]
    public partial double EurRate { get; set; }

    [ObservableProperty]
    public partial double UsdtRate { get; set; }

    [ObservableProperty]
    public partial bool AutoFetchOnStartup { get; set; }

    [ObservableProperty]
    public partial DateTime SelectedDate { get; set; }

    [ObservableProperty]
    public partial string FormattedDate { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AmountText { get; set; } = "1";

    [ObservableProperty]
    public partial string SelectedCurrency { get; set; } = "USD";

    [ObservableProperty]
    public partial bool IsToVes { get; set; } = true;

    [ObservableProperty]
    public partial string ConversionResult { get; set; } = "0.00 VES";

    [ObservableProperty]
    public partial IReadOnlyList<ExchangeRate> History { get; set; } = Array.Empty<ExchangeRate>();

    public MainViewModel(BcvScraperService scraperService, IServiceProvider serviceProvider)
    {
        _scraperService = scraperService;
        _serviceProvider = serviceProvider;

        // Cargar preferencia del usuario usando Preferences (default: true)
        AutoFetchOnStartup = Preferences.Default.Get("AutoFetchOnStartup", true);

        IsBusy = true;
        StatusMessage = "Cargando datos...";
        SelectedDate = DateTime.Today;
        FormattedDate = DateTime.Today.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
    }

    // Handlers de cambio de propiedades
    partial void OnUsdRateChanged(double value) => Recalculate();
    partial void OnEurRateChanged(double value) => Recalculate();
    partial void OnUsdtRateChanged(double value) => Recalculate();
    partial void OnAmountTextChanged(string value) => Recalculate();
    partial void OnSelectedCurrencyChanged(string value) => Recalculate();
    partial void OnIsToVesChanged(bool value) => Recalculate();

    partial void OnAutoFetchOnStartupChanged(bool value)
    {
        Preferences.Default.Set("AutoFetchOnStartup", value);
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        FormattedDate = value.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
        if (!IsBusy)
        {
            Task.Run(() => LoadRatesForDateAsync(value));
        }
    }

    private IServiceScope CreateDbScope(out BcvDbContext dbContext)
    {
        var scope = _serviceProvider.CreateScope();
        dbContext = scope.ServiceProvider.GetRequiredService<BcvDbContext>();
        return scope;
    }

    public async Task InitializeAsync()
    {
        lock (_initLock)
        {
            if (_isInitialized) return;
            _isInitialized = true;
        }

        try
        {
            await LoadHistoryAsync();

            if (AutoFetchOnStartup)
            {
                await FetchRatesAsync();
            }
            else
            {
                await LoadLatestFromDbAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error durante la inicialización: {ex.Message}");
            SetBusyState(false, $"Error de inicialización: {ex.Message}");
        }
    }

    private async Task LoadLatestFromDbAsync()
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                var latestRate = await dbContext.ExchangeRates
                    .OrderByDescending(e => e.Date)
                    .FirstOrDefaultAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (latestRate != null)
                    {
                        UsdRate = latestRate.UsdRate;
                        EurRate = latestRate.EurRate;
                        UsdtRate = latestRate.UsdtRate;
                        SelectedDate = latestRate.Date;
                        FormattedDate = latestRate.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                        SetBusyState(false, "Consulta automática desactivada. Presione 'Actualizar' para buscar tasas online.");
                    }
                    else
                    {
                        SetBusyState(false, "Consulta automática desactivada. Presione 'Actualizar' para buscar tasas online.");
                    }
                });
            }
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async Task FetchRatesAsync()
    {
        ExchangeRate? scraped = null;
        try
        {
            scraped = await _scraperService.ScrapeRatesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de scraping BCV/Binance: {ex.Message}");
        }

        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                if (scraped != null)
                {
                    var existing = await dbContext.ExchangeRates
                        .FirstOrDefaultAsync(e => e.Date == scraped.Date);

                    if (existing == null)
                    {
                        dbContext.ExchangeRates.Add(scraped);
                    }
                    else
                    {
                        existing.UsdRate = scraped.UsdRate;
                        existing.EurRate = scraped.EurRate;
                        if (scraped.UsdtRate > 0)
                        {
                            existing.UsdtRate = scraped.UsdtRate;
                        }
                        existing.CreatedAt = DateTime.Now;
                    }
                    await dbContext.SaveChangesAsync();

                    var newHistory = await dbContext.ExchangeRates
                        .OrderByDescending(e => e.Date)
                        .Take(15)
                        .ToListAsync();

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        History = newHistory;
                        UsdRate = scraped.UsdRate;
                        EurRate = scraped.EurRate;
                        if (scraped.UsdtRate > 0) UsdtRate = scraped.UsdtRate;
                        SelectedDate = scraped.Date;
                        FormattedDate = scraped.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                        SetBusyState(false, $"Tasas actualizadas (BCV & Binance USDT - Fecha: {scraped.Date:dd/MM/yyyy}).");
                    });
                }
                else
                {
                    await LoadLatestFromDbAsync();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en base de datos: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetBusyState(false, $"Error de base de datos: {ex.Message}");
            });
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    [RelayCommand]
    private async Task FetchLatestRatesAsync()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsBusy = true;
            StatusMessage = "Conectando con BCV y Binance...";
        });
        await FetchRatesAsync();
    }

    private async Task LoadRatesForDateAsync(DateTime date)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsBusy = true;
            StatusMessage = $"Buscando tasas para el {date:dd/MM/yyyy}...";
        });

        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                var targetDate = date.Date;
                var rate = await dbContext.ExchangeRates
                    .FirstOrDefaultAsync(e => e.Date == targetDate);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (rate != null)
                    {
                        UsdRate = rate.UsdRate;
                        EurRate = rate.EurRate;
                        UsdtRate = rate.UsdtRate;
                        SelectedDate = rate.Date;
                        FormattedDate = rate.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                        SetBusyState(false, $"Mostrando tasas guardadas para el {rate.Date:dd/MM/yyyy}.");
                    }
                    else
                    {
                        UsdRate = 0;
                        EurRate = 0;
                        UsdtRate = 0;
                        SelectedDate = date;
                        FormattedDate = date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                        SetBusyState(false, $"No hay datos guardados para el {date:dd/MM/yyyy}. Presiona Actualizar para intentar buscar online.");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetBusyState(false, $"Error al buscar tasa histórica: {ex.Message}");
            });
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async Task LoadHistoryAsync()
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                var historyList = await dbContext.ExchangeRates
                    .OrderByDescending(e => e.Date)
                    .Take(15)
                    .ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    History = historyList;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar historial: {ex.Message}");
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private void SetBusyState(bool isBusy, string statusMessage)
    {
        IsBusy = isBusy;
        StatusMessage = statusMessage;
    }

    [RelayCommand]
    private async Task CopyAmountToConvertAsync()
    {
        if (!string.IsNullOrWhiteSpace(AmountText))
        {
            await Clipboard.Default.SetTextAsync(AmountText);
            StatusMessage = "Monto a convertir copiado al portapapeles.";
        }
    }

    [RelayCommand]
    private async Task CopyResultAsync()
    {
        if (!string.IsNullOrWhiteSpace(ConversionResult))
        {
            string cleanText = ConversionResult;
            int spaceIndex = ConversionResult.LastIndexOf(' ');
            if (spaceIndex > 0)
            {
                cleanText = ConversionResult.Substring(0, spaceIndex).Trim();
            }

            if (cleanText != "Monto" && cleanText != "Tasa" && cleanText != "Monto inválido" && cleanText != "Tasa no disponible")
            {
                await Clipboard.Default.SetTextAsync(cleanText);
                StatusMessage = $"Resultado ({cleanText}) copiado al portapapeles.";
            }
        }
    }

    [RelayCommand]
    private void ToggleDirection()
    {
        IsToVes = !IsToVes;
    }

    [RelayCommand]
    private void SelectCurrency(string currency)
    {
        SelectedCurrency = currency;
    }

    private void Recalculate()
    {
        if (string.IsNullOrWhiteSpace(AmountText))
        {
            ConversionResult = "Monto inválido";
            return;
        }

        string cleanAmount = AmountText.Replace(",", ".");
        if (!double.TryParse(cleanAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out double amount))
        {
            ConversionResult = "Monto inválido";
            return;
        }

        double rate = SelectedCurrency switch
        {
            "USD" => UsdRate,
            "EUR" => EurRate,
            "USDT" => UsdtRate,
            _ => UsdRate
        };

        if (rate <= 0)
        {
            ConversionResult = "Tasa no disponible";
            return;
        }

        if (IsToVes)
        {
            double result = amount * rate;
            ConversionResult = $"{result.ToString("N2", new CultureInfo("es-VE"))} VES";
        }
        else
        {
            double result = amount / rate;
            ConversionResult = $"{result.ToString("N2", CultureInfo.InvariantCulture)} {SelectedCurrency}";
        }
    }
}
