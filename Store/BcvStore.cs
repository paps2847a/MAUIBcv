using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BcvExchangeApp.Data;
using BcvExchangeApp.Models;
using BcvExchangeApp.Services;

namespace BcvExchangeApp.Store;

public class BcvStore : INotifyPropertyChanged
{
    private readonly BcvScraperService _scraperService;
    private readonly IServiceProvider _serviceProvider;
    private BcvState _state;

    // Control de concurrencia para evitar accesos simultáneos a EF Core
    private readonly SemaphoreSlim _dbSemaphore = new(1, 1);
    private bool _isInitialized = false;
    private readonly object _initLock = new();

    public BcvState State => _state;

    // Inyectamos IServiceProvider para resolver BcvDbContext de forma diferida (lazy)
    // Esto evita cargar las librerías de Entity Framework Core durante el constructor de MainPage al iniciar la app
    public BcvStore(BcvScraperService scraperService, IServiceProvider serviceProvider)
    {
        _scraperService = scraperService;
        _serviceProvider = serviceProvider;
        _state = GetInitialState();
    }

    private BcvDbContext GetDbContext()
    {
        return _serviceProvider.GetRequiredService<BcvDbContext>();
    }

    private static BcvState GetInitialState()
    {
        return new BcvState(
            IsBusy: true,
            StatusMessage: "Conectando al Banco Central...",
            UsdRate: 0,
            EurRate: 0,
            SelectedDate: DateTime.Today,
            FormattedDate: DateTime.Today.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES")),
            AmountText: "1",
            SelectedCurrency: "USD",
            IsToVes: true,
            ConversionResult: "0.00 VES",
            History: Array.Empty<ExchangeRate>()
        );
    }

    // Inicializar base de datos y cargar primer estado
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
                var dbContext = GetDbContext();
                await dbContext.Database.EnsureCreatedAsync();
            }
            finally
            {
                _dbSemaphore.Release();
            }

            await LoadHistoryEffectAsync();
            await FetchRatesEffectAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error durante la inicialización: {ex.Message}");
            Dispatch(new RatesFailed($"Error de inicialización: {ex.Message}", null, null, null));
        }
    }

    // --- Despachador de Mensajes (El corazón de MVU) ---
    public void Dispatch(BcvMsg msg)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var oldState = _state;
            var newState = Reduce(oldState, msg);

            if (msg is FetchLatestRates)
            {
                newState = Reduce(newState, new LoadingStarted("Conectando al Banco Central..."));
            }
            else if (msg is SelectDate sd)
            {
                newState = Reduce(newState, new LoadingStarted($"Buscando tasas para el {sd.Date:dd/MM/yyyy}..."));
            }

            if (newState != oldState)
            {
                _state = newState;
                OnPropertyChanged(nameof(State));
            }

            switch (msg)
            {
                case FetchLatestRates:
                    Task.Run(FetchRatesEffectAsync);
                    break;
                case SelectDate sd:
                    Task.Run(() => LoadRatesForDateEffectAsync(sd.Date));
                    break;
            }
        });
    }

    // --- Función Reductora (Pure-like state transition) ---
    private BcvState Reduce(BcvState state, BcvMsg msg)
    {
        switch (msg)
        {
            case LoadingStarted ls:
                return state with { IsBusy = true, StatusMessage = ls.StatusMessage };

            case SelectDate sd:
                return state with
                {
                    SelectedDate = sd.Date,
                    FormattedDate = sd.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"))
                };

            case RatesLoaded rl:
                var stateWithRates = state with
                {
                    UsdRate = rl.UsdRate,
                    EurRate = rl.EurRate,
                    SelectedDate = rl.Date,
                    FormattedDate = rl.Date.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES")),
                    StatusMessage = rl.StatusMessage,
                    IsBusy = false
                };
                return Recalculate(stateWithRates);

            case RatesFailed rf:
                var stateWithFail = state with
                {
                    StatusMessage = rf.StatusMessage,
                    IsBusy = false
                };
                if (rf.UsdRate.HasValue) stateWithFail = stateWithFail with { UsdRate = rf.UsdRate.Value };
                if (rf.EurRate.HasValue) stateWithFail = stateWithFail with { EurRate = rf.EurRate.Value };
                if (rf.Date.HasValue)
                {
                    stateWithFail = stateWithFail with
                    {
                        SelectedDate = rf.Date.Value,
                        FormattedDate = rf.Date.Value.ToString("dd 'de' MMMM, yyyy", new CultureInfo("es-ES"))
                    };
                }
                return Recalculate(stateWithFail);

            case HistoryLoaded hl:
                return state with { History = hl.History };

            case SelectCurrency sc:
                return Recalculate(state with { SelectedCurrency = sc.Currency });

            case ChangeAmount ca:
                return Recalculate(state with { AmountText = ca.Amount });

            case ToggleDirection:
                return Recalculate(state with { IsToVes = !state.IsToVes });

            default:
                return state;
        }
    }

    private BcvState Recalculate(BcvState state)
    {
        if (string.IsNullOrWhiteSpace(state.AmountText))
        {
            return state with { ConversionResult = "Monto inválido" };
        }

        string cleanAmount = state.AmountText.Replace(",", ".");
        if (!double.TryParse(cleanAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out double amount))
        {
            return state with { ConversionResult = "Monto inválido" };
        }

        double rate = state.SelectedCurrency == "USD" ? state.UsdRate : state.EurRate;
        if (rate <= 0)
        {
            return state with { ConversionResult = "Tasa no disponible" };
        }

        if (state.IsToVes)
        {
            double result = amount * rate;
            return state with { ConversionResult = $"{result.ToString("N2", new CultureInfo("es-VE"))} VES" };
        }
        else
        {
            double result = amount / rate;
            return state with { ConversionResult = $"{result.ToString("N2", CultureInfo.InvariantCulture)} {state.SelectedCurrency}" };
        }
    }

    // --- Efectos Secundarios Asíncronos ---

    private async Task FetchRatesEffectAsync()
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
            var dbContext = GetDbContext();
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

                Dispatch(new HistoryLoaded(newHistory));
                Dispatch(new RatesLoaded(
                    scraped.UsdRate,
                    scraped.EurRate,
                    scraped.Date,
                    $"Tasas actualizadas desde el BCV (Fecha Valor: {scraped.Date:dd/MM/yyyy})."
                ));
            }
            else
            {
                var today = DateTime.Today;
                var todayRate = await dbContext.ExchangeRates
                    .FirstOrDefaultAsync(e => e.Date == today);

                if (todayRate != null)
                {
                    Dispatch(new RatesLoaded(
                        todayRate.UsdRate,
                        todayRate.EurRate,
                        todayRate.Date,
                        "Sin conexión. Mostrando tasas guardadas para el día de hoy."
                    ));
                }
                else
                {
                    Dispatch(new RatesFailed(
                        "Sin conexión. No se encontraron tasas registradas para el día de hoy.",
                        0, 0, today
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en base de datos: {ex.Message}");
            Dispatch(new RatesFailed($"Error de base de datos: {ex.Message}", null, null, null));
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async Task LoadRatesForDateEffectAsync(DateTime date)
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            var dbContext = GetDbContext();
            var targetDate = date.Date;
            var rate = await dbContext.ExchangeRates
                .FirstOrDefaultAsync(e => e.Date == targetDate);

            if (rate != null)
            {
                Dispatch(new RatesLoaded(
                    rate.UsdRate,
                    rate.EurRate,
                    rate.Date,
                    $"Mostrando tasas históricas para el {rate.Date:dd/MM/yyyy}."
                ));
            }
            else
            {
                Dispatch(new RatesFailed(
                    $"No hay datos guardados para el {date:dd/MM/yyyy}. Presiona Refrescar para intentar buscar online.",
                    0, 0, date
                ));
            }
        }
        catch (Exception ex)
        {
            Dispatch(new RatesFailed($"Error al buscar tasa histórica: {ex.Message}", null, null, null));
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private async Task LoadHistoryEffectAsync()
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            var dbContext = GetDbContext();
            var historyList = await dbContext.ExchangeRates
                .OrderByDescending(e => e.Date)
                .Take(15)
                .ToListAsync();

            Dispatch(new HistoryLoaded(historyList));
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

    // --- Notificación de cambio de propiedad ---
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
