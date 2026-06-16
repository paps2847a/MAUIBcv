using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BcvExchangeApp.Data;
using BcvExchangeApp.Models;
using BcvExchangeApp.Services;

namespace BcvExchangeApp.Store;

public class BcvStore : INotifyPropertyChanged
{
    private readonly BcvScraperService _scraperService;
    private readonly BcvDbContext _dbContext;
    private BcvState _state;

    public BcvState State => _state;

    public BcvStore(BcvScraperService scraperService, BcvDbContext dbContext)
    {
        _scraperService = scraperService;
        _dbContext = dbContext;
        _state = GetInitialState();
    }

    private static BcvState GetInitialState()
    {
        return new BcvState(
            IsBusy: false,
            StatusMessage: "Iniciando aplicación...",
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
        await _dbContext.Database.EnsureCreatedAsync();
        await LoadHistoryEffectAsync();
        await FetchRatesEffectAsync(); // Primer scraping en segundo plano al abrir la app
    }

    // --- Despachador de Mensajes (El corazón de MVU) ---
    public void Dispatch(BcvMsg msg)
    {
        // Ejecutar en el hilo principal de UI para evitar problemas de binding y asegurar consistencia
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var oldState = _state;
            
            // 1. Reducir el mensaje original para actualizar la fecha/valores inmediatamente
            var newState = Reduce(oldState, msg);

            // 2. Si el mensaje inicia una acción asíncrona, aplicar el estado de carga sobre el nuevo estado
            if (msg is FetchLatestRates || msg is SelectDate)
            {
                newState = Reduce(newState, new LoadingStarted());
            }

            if (newState != oldState)
            {
                _state = newState;
                OnPropertyChanged(nameof(State));
            }

            // 3. Ejecutar efectos secundarios asincrónicos
            switch (msg)
            {
                case FetchLatestRates:
                    await FetchRatesEffectAsync();
                    break;
                case SelectDate sd:
                    await LoadRatesForDateEffectAsync(sd.Date);
                    break;
            }
        });
    }

    // --- Función Reductora (Pure-like state transition) ---
    private BcvState Reduce(BcvState state, BcvMsg msg)
    {
        switch (msg)
        {
            case LoadingStarted:
                return state with { IsBusy = true, StatusMessage = "Conectando al Banco Central..." };

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

    // --- Lógica del Conversor integrada en la transición de estado ---
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
        try
        {
            var scraped = await _scraperService.ScrapeRatesAsync();
            if (scraped != null)
            {
                var existing = await _dbContext.ExchangeRates
                    .FirstOrDefaultAsync(e => e.Date == scraped.Date);

                if (existing == null)
                {
                    _dbContext.ExchangeRates.Add(scraped);
                }
                else
                {
                    existing.UsdRate = scraped.UsdRate;
                    existing.EurRate = scraped.EurRate;
                    existing.CreatedAt = DateTime.Now;
                }
                await _dbContext.SaveChangesAsync();

                var newHistory = await _dbContext.ExchangeRates
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de conexión o timeout en efecto MVU: {ex.Message}");
            
            // Verificamos si existe un registro local con la fecha del día que transcurre (hoy)
            var today = DateTime.Today;
            var todayRate = await _dbContext.ExchangeRates
                .FirstOrDefaultAsync(e => e.Date == today);

            if (todayRate != null)
            {
                // De existir, se muestran los precios guardados para hoy
                Dispatch(new RatesLoaded(
                    todayRate.UsdRate,
                    todayRate.EurRate,
                    todayRate.Date,
                    "Sin conexión. Mostrando tasas guardadas para el día de hoy."
                ));
            }
            else
            {
                // De no existir, se le indica al usuario la falta de datos para el día actual
                Dispatch(new RatesFailed(
                    "Sin conexión. No se encontraron tasas registradas para el día de hoy.",
                    0, 0, today
                ));
            }
        }
    }

    private async Task LoadRatesForDateEffectAsync(DateTime date)
    {
        try
        {
            var targetDate = date.Date;
            var rate = await _dbContext.ExchangeRates
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
                    0, 0, date // Asignar 0 limpia las tasas antiguas para evitar inconsistencias en UI
                ));
            }
        }
        catch (Exception ex)
        {
            Dispatch(new RatesFailed($"Error al buscar tasa histórica: {ex.Message}", null, null, null));
        }
    }

    private async Task LoadHistoryEffectAsync()
    {
        try
        {
            var historyList = await _dbContext.ExchangeRates
                .OrderByDescending(e => e.Date)
                .Take(15)
                .ToListAsync();

            Dispatch(new HistoryLoaded(historyList));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar historial: {ex.Message}");
        }
    }

    // --- Notificación de cambio de propiedad ---
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
