using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using BcvExchangeApp.Data;
using BcvExchangeApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;

namespace BcvExchangeApp.ViewModels;

public partial class ComprasViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _dbSemaphore = new(1, 1);
    private ExchangeRate? _latestRate;
    private bool _isInitialized = false;
    private readonly object _initLock = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private IReadOnlyList<ShoppingItem> _shoppingItems = Array.Empty<ShoppingItem>();

    [ObservableProperty]
    private IReadOnlyList<PurchaseRecord> _purchaseHistory = Array.Empty<PurchaseRecord>();

    [ObservableProperty]
    private IReadOnlyList<PagoMovilRecord> _pagoMovilList = Array.Empty<PagoMovilRecord>();

    [ObservableProperty]
    private PagoMovilRecord? _selectedPagoMovil;

    // Form fields
    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private string _formPrice = string.Empty;

    [ObservableProperty]
    private string _formCurrency = "USD"; // Default to USD

    [ObservableProperty]
    private string _formQuantityText = "1";

    // Totals
    [ObservableProperty]
    private double _totalVes;

    [ObservableProperty]
    private double _totalUsd;

    [ObservableProperty]
    private double _totalEur;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HistoryPageText))]
    private int _historyPageNumber = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HistoryPageText))]
    private int _totalHistoryPages = 1;

    [ObservableProperty]
    private bool _hasPreviousHistoryPage;

    [ObservableProperty]
    private bool _hasNextHistoryPage;

    public string HistoryPageText => $"Página {HistoryPageNumber} de {TotalHistoryPages}";

    [ObservableProperty]
    private string _activeTab = "List"; // "List" or "History"

    public ComprasViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private IServiceScope CreateDbScope(out BcvDbContext dbContext)
    {
        var scope = _serviceProvider.CreateScope();
        dbContext = scope.ServiceProvider.GetRequiredService<BcvDbContext>();
        return scope;
    }

    partial void OnStatusMessageChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            Task.Delay(3000).ContinueWith(_ => StatusMessage = string.Empty);
        }
    }

    public async Task InitializeAsync()
    {
        bool shouldInitDb = false;
        lock (_initLock)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                shouldInitDb = true;
            }
        }

        if (shouldInitDb)
        {
            MainThread.BeginInvokeOnMainThread(() => IsLoading = true);
            try
            {
                await _dbSemaphore.WaitAsync();
                try
                {
                    using (CreateDbScope(out var dbContext))
                    {
                        await dbContext.Database.EnsureCreatedAsync();

                        // Asegurar creación de tablas en bases de datos preexistentes
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "CREATE TABLE IF NOT EXISTS \"ShoppingItems\" (" +
                            "\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                            "\"Name\" TEXT NOT NULL, " +
                            "\"Price\" REAL NOT NULL, " +
                            "\"Currency\" TEXT NOT NULL, " +
                            "\"Quantity\" INTEGER NOT NULL, " +
                            "\"CreatedAt\" TEXT NOT NULL" +
                            ");"
                        );

                        await dbContext.Database.ExecuteSqlRawAsync(
                            "CREATE TABLE IF NOT EXISTS \"PurchaseRecords\" (" +
                            "\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                            "\"PurchaseDate\" TEXT NOT NULL, " +
                            "\"TotalVes\" REAL NOT NULL, " +
                            "\"TotalUsd\" REAL NOT NULL, " +
                            "\"TotalEur\" REAL NOT NULL, " +
                            "\"ItemSummary\" TEXT NOT NULL, " +
                            "\"ItemsJson\" TEXT NOT NULL DEFAULT '', " +
                            "\"UsdRate\" REAL NOT NULL DEFAULT 0.0, " +
                            "\"EurRate\" REAL NOT NULL DEFAULT 0.0, " +
                            "\"CreatedAt\" TEXT NOT NULL" +
                            ");"
                        );

                        // Crear índice en PurchaseDate para optimizar el ordenamiento de paginación
                        await dbContext.Database.ExecuteSqlRawAsync(
                            "CREATE INDEX IF NOT EXISTS \"IX_PurchaseRecords_PurchaseDate\" ON \"PurchaseRecords\" (\"PurchaseDate\");"
                        );

                        try
                        {
                            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"ItemsJson\" TEXT NOT NULL DEFAULT '';");
                        }
                        catch { /* ignorado si ya existe */ }

                        try
                        {
                            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"UsdRate\" REAL NOT NULL DEFAULT 0.0;");
                        }
                        catch { /* ignorado si ya existe */ }

                        try
                        {
                            await dbContext.Database.ExecuteSqlRawAsync("ALTER TABLE \"PurchaseRecords\" ADD COLUMN \"EurRate\" REAL NOT NULL DEFAULT 0.0;");
                        }
                        catch { /* ignorado si ya existe */ }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al asegurar tablas de compras: {ex.Message}");
                }
                finally
                {
                    _dbSemaphore.Release();
                }

                await _dbSemaphore.WaitAsync();
                try
                {
                    using (CreateDbScope(out var dbContext))
                    {
                        _latestRate = await dbContext.ExchangeRates
                            .OrderByDescending(r => r.Date)
                            .FirstOrDefaultAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al cargar tasa de cambio: {ex.Message}");
                }
                finally
                {
                    _dbSemaphore.Release();
                }

                await LoadItemsAsync();
                await LoadHistoryAsync();
                await LoadPagoMovilsAsync();

                if (_latestRate == null)
                {
                    StatusMessage = "Sin tasas guardadas. Actualice tasas en 'Inicio' para conversiones precisas.";
                }
            }
            finally
            {
                MainThread.BeginInvokeOnMainThread(() => IsLoading = false);
            }
        }
        else
        {
            // Carga silenciosa en segundo plano (sin bloquear la interfaz ni encender spinner)
            _ = Task.Run(async () =>
            {
                await _dbSemaphore.WaitAsync();
                try
                {
                    using (CreateDbScope(out var dbContext))
                    {
                        _latestRate = await dbContext.ExchangeRates
                            .OrderByDescending(r => r.Date)
                            .FirstOrDefaultAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error silencioso al cargar tasas: {ex.Message}");
                }
                finally
                {
                    _dbSemaphore.Release();
                }

                await LoadItemsAsync();
                await LoadHistoryAsync();
                await LoadPagoMovilsAsync();
            });
        }
    }

    public async Task LoadItemsAsync()
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                var list = await dbContext.ShoppingItems
                    .OrderBy(i => i.CreatedAt)
                    .ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShoppingItems = list;
                    RecalculateTotals();
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar items: {ex.Message}");
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    public async Task LoadHistoryAsync(int? targetPage = null)
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                // Contar registros totales para calcular paginación
                int totalCount = await dbContext.PurchaseRecords.CountAsync();
                int totalPages = (int)Math.Ceiling(totalCount / 5.0);
                if (totalPages == 0) totalPages = 1;
                
                int page = targetPage ?? HistoryPageNumber;
                if (page > totalPages) page = totalPages;
                if (page < 1) page = 1;
                
                var list = await dbContext.PurchaseRecords
                    .OrderByDescending(r => r.PurchaseDate)
                    .Skip((page - 1) * 5)
                    .Take(5)
                    .ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HistoryPageNumber = page;
                    TotalHistoryPages = totalPages;
                    HasPreviousHistoryPage = page > 1;
                    HasNextHistoryPage = page < totalPages;
                    PurchaseHistory = list;
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

    public async Task LoadPagoMovilsAsync()
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                var list = await dbContext.PagoMovilRecords
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PagoMovilList = list;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar pago móviles: {ex.Message}");
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        string priceRaw = FormPrice ?? string.Empty;
        string cleanPrice = priceRaw.Trim().Replace(",", ".");
        if (string.IsNullOrWhiteSpace(cleanPrice) || !double.TryParse(cleanPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out double price) || price <= 0)
        {
            StatusMessage = "Por favor ingrese un precio válido.";
            return;
        }

        int quantity = 1;
        string qtyRaw = FormQuantityText ?? string.Empty;
        string qtyStr = qtyRaw.Trim();
        if (!string.IsNullOrWhiteSpace(qtyStr))
        {
            if (!int.TryParse(qtyStr, out quantity) || quantity <= 0)
            {
                StatusMessage = "Por favor ingrese una cantidad válida.";
                return;
            }
        }

        // El nombre es opcional. Si está vacío, se asigna "Producto" por defecto
        string nameRaw = FormName ?? string.Empty;
        string name = nameRaw.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Producto";
        }

        var newItem = new ShoppingItem
        {
            Name = name,
            Price = price,
            Currency = FormCurrency ?? "USD",
            Quantity = quantity,
            CreatedAt = DateTime.Now
        };

        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                dbContext.ShoppingItems.Add(newItem);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al guardar: {ex.Message}";
            return;
        }
        finally
        {
            _dbSemaphore.Release();
        }

        // Limpiar campos del formulario
        FormName = string.Empty;
        FormPrice = string.Empty;
        FormQuantityText = "1";

        await LoadItemsAsync();
        StatusMessage = "Producto agregado.";
    }

    [RelayCommand]
    private async Task DeleteItemAsync(ShoppingItem item)
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                dbContext.ShoppingItems.Remove(item);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar ítem: {ex.Message}");
        }
        finally
        {
            _dbSemaphore.Release();
        }

        await LoadItemsAsync();
        StatusMessage = "Producto eliminado.";
    }

    [RelayCommand]
    private async Task CompletePurchaseAsync()
    {
        if (ShoppingItems.Count == 0)
        {
            StatusMessage = "El carrito está vacío.";
            return;
        }

        // Resumen de la compra
        var summaries = ShoppingItems.Select(i => $"{i.Name} ({i.Quantity})");
        string summary = string.Join(", ", summaries);
        if (summary.Length > 250)
        {
            summary = summary.Substring(0, 247) + "...";
        }

        string itemsJson = string.Empty;
        try
        {
            itemsJson = JsonSerializer.Serialize(ShoppingItems);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al serializar productos: {ex.Message}");
        }

        var record = new PurchaseRecord
        {
            PurchaseDate = DateTime.Now,
            TotalVes = TotalVes,
            TotalUsd = TotalUsd,
            TotalEur = TotalEur,
            ItemSummary = summary,
            ItemsJson = itemsJson,
            UsdRate = _latestRate?.UsdRate ?? 0.0,
            EurRate = _latestRate?.EurRate ?? 0.0,
            CreatedAt = DateTime.Now
        };

        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                dbContext.PurchaseRecords.Add(record);
                dbContext.ShoppingItems.RemoveRange(dbContext.ShoppingItems);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al registrar compra: {ex.Message}";
            return;
        }
        finally
        {
            _dbSemaphore.Release();
        }

        HistoryPageNumber = 1;
        await LoadItemsAsync();
        await LoadHistoryAsync();
        StatusMessage = "Compra registrada con éxito.";
    }

    [RelayCommand]
    private async Task DeletePurchaseAsync(PurchaseRecord record)
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                dbContext.PurchaseRecords.Remove(record);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar registro de compra: {ex.Message}");
        }
        finally
        {
            _dbSemaphore.Release();
        }

        await LoadHistoryAsync();
        StatusMessage = "Registro de compra eliminado.";
    }

    [RelayCommand]
    private async Task NextHistoryPageAsync()
    {
        if (HistoryPageNumber < TotalHistoryPages)
        {
            await LoadHistoryAsync(HistoryPageNumber + 1);
        }
    }

    [RelayCommand]
    private async Task PreviousHistoryPageAsync()
    {
        if (HistoryPageNumber > 1)
        {
            await LoadHistoryAsync(HistoryPageNumber - 1);
        }
    }

    [RelayCommand]
    private void SwitchTab(string tab)
    {
        ActiveTab = tab;
    }

    [RelayCommand]
    private void SelectFormCurrency(string currency)
    {
        FormCurrency = currency;
    }

    [RelayCommand]
    private async Task CopyPagoMovilAsync()
    {
        if (SelectedPagoMovil != null)
        {
            string text = $"{SelectedPagoMovil.Cedula}\n{SelectedPagoMovil.Phone}\n{SelectedPagoMovil.BankCode} - {SelectedPagoMovil.BankName}";
            await Clipboard.Default.SetTextAsync(text);
            StatusMessage = "Pago Móvil copiado al portapapeles.";
        }
    }

    private void RecalculateTotals()
    {
        double ves = 0;
        double usd = 0;
        double eur = 0;

        double usdRate = _latestRate?.UsdRate ?? 0;
        double eurRate = _latestRate?.EurRate ?? 0;

        foreach (var item in ShoppingItems)
        {
            double itemTotal = item.TotalPrice;
            if (item.Currency == "VES")
            {
                ves += itemTotal;
                if (usdRate > 0) usd += itemTotal / usdRate;
                if (eurRate > 0) eur += itemTotal / eurRate;
            }
            else if (item.Currency == "USD")
            {
                if (usdRate > 0) ves += itemTotal * usdRate;
                usd += itemTotal;
                if (eurRate > 0 && usdRate > 0) eur += (itemTotal * usdRate) / eurRate;
            }
            else if (item.Currency == "EUR")
            {
                if (eurRate > 0) ves += itemTotal * eurRate;
                if (usdRate > 0 && eurRate > 0) usd += (itemTotal * eurRate) / usdRate;
                eur += itemTotal;
            }
        }

        TotalVes = ves;
        TotalUsd = usd;
        TotalEur = eur;
    }

}
