using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
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

    // Backing fields with Source Generator attributes
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private double _usdRate;

    [ObservableProperty]
    private double _eurRate;

    private DateTime _selectedDate;
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                FormattedDate = value.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                Task.Run(() => LoadRatesForDateAsync(value));
            }
        }
    }

    [ObservableProperty]
    private string _formattedDate = string.Empty;

    [ObservableProperty]
    private string _amountText = "1";

    [ObservableProperty]
    private string _selectedCurrency = "USD";

    [ObservableProperty]
    private bool _isToVes = true;

    [ObservableProperty]
    private string _conversionResult = "0.00 VES";

    [ObservableProperty]
    private IReadOnlyList<ExchangeRate> _history = Array.Empty<ExchangeRate>();

    public MainViewModel(BcvScraperService scraperService, IServiceProvider serviceProvider)
    {
        _scraperService = scraperService;
        _serviceProvider = serviceProvider;

        // Set initial values (direct backing field assignments)
        _isBusy = true;
        _statusMessage = "Conectando al Banco Central...";
        _selectedDate = DateTime.Today;
        _formattedDate = DateTime.Today.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
    }

    private IServiceScope CreateDbScope(out BcvDbContext dbContext)
    {
        var scope = _serviceProvider.CreateScope();
        dbContext = scope.ServiceProvider.GetRequiredService<BcvDbContext>();
        return scope;
    }

    // Partial change notification handlers
    partial void OnUsdRateChanged(double value) => Recalculate();
    partial void OnEurRateChanged(double value) => Recalculate();
    partial void OnAmountTextChanged(string value) => Recalculate();
    partial void OnSelectedCurrencyChanged(string value) => Recalculate();
    partial void OnIsToVesChanged(bool value) => Recalculate();

    // Initializer
    public async Task InitializeAsync()
    {
        lock (_initLock)
        {
            if (_isInitialized) return;
            _isInitialized = true;
        }

        try
        {
            await _dbSemaphore.WaitAsync();
            try
            {
                using (CreateDbScope(out var dbContext))
                {
                    await dbContext.Database.EnsureCreatedAsync();
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "CREATE TABLE IF NOT EXISTS \"PagoMovilRecords\" (" +
                        "\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                        "\"Cedula\" TEXT NOT NULL, " +
                        "\"Phone\" TEXT NOT NULL, " +
                        "\"BankCode\" TEXT NOT NULL, " +
                        "\"BankName\" TEXT NOT NULL, " +
                        "\"CreatedAt\" TEXT NOT NULL" +
                        ");"
                    );
                }
            }
            finally
            {
                _dbSemaphore.Release();
            }

            await LoadHistoryAsync();
            await FetchRatesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error durante la inicialización: {ex.Message}");
            SetBusyState(false, $"Error de inicialización: {ex.Message}");
        }
    }

    // Fetch Rates
    private async Task FetchRatesAsync()
    {
        ExchangeRate? scraped = null;
        try
        {
            scraped = await _scraperService.ScrapeRatesAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de scraping BCV: {ex.Message}");
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
                        _selectedDate = scraped.Date; // update backing field directly to avoid triggering OnSelectedDateChanged task
                        OnPropertyChanged(nameof(SelectedDate));
                        FormattedDate = scraped.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                        SetBusyState(false, $"Tasas actualizadas desde el BCV (Fecha Valor: {scraped.Date:dd/MM/yyyy}).");
                    });
                }
                else
                {
                    var today = DateTime.Today;
                    var todayRate = await dbContext.ExchangeRates
                        .FirstOrDefaultAsync(e => e.Date == today);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (todayRate != null)
                        {
                            UsdRate = todayRate.UsdRate;
                            EurRate = todayRate.EurRate;
                            _selectedDate = todayRate.Date;
                            OnPropertyChanged(nameof(SelectedDate));
                            FormattedDate = todayRate.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                            SetBusyState(false, "Sin conexión. Mostrando tasas guardadas para el día de hoy.");
                        }
                        else
                        {
                            UsdRate = 0;
                            EurRate = 0;
                            _selectedDate = today;
                            OnPropertyChanged(nameof(SelectedDate));
                            FormattedDate = today.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                            SetBusyState(false, "Sin conexión. No se encontraron tasas registradas para el día de hoy.");
                        }
                    });
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
              StatusMessage = "Conectando al Banco Central...";
          });
          await FetchRatesAsync();
      }

      // Load rates for a historical date
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
                        _selectedDate = rate.Date;
                        OnPropertyChanged(nameof(SelectedDate));
                        FormattedDate = rate.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"));
                        SetBusyState(false, $"Mostrando tasas históricas para el {rate.Date:dd/MM/yyyy}.");
                    }
                    else
                    {
                        UsdRate = 0;
                        EurRate = 0;
                        _selectedDate = date;
                        OnPropertyChanged(nameof(SelectedDate));
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

      // Load recent history (up to 15 rates)
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

          double rate = SelectedCurrency == "USD" ? UsdRate : EurRate;
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
