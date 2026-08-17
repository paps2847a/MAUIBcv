using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using BcvExchangeApp.Data;
using BcvExchangeApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BcvExchangeApp.ViewModels;

public record Bank(string Code, string Name)
{
    public string DisplayName => $"{Code} - {Name}";
}

public partial class PagoMovilViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _dbSemaphore = new(1, 1);

    [ObservableProperty]
    public partial IReadOnlyList<PagoMovilRecord> Records { get; set; } = Array.Empty<PagoMovilRecord>();

    private IReadOnlyList<PagoMovilRecord> _allRecords = Array.Empty<PagoMovilRecord>();

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial IReadOnlyList<Bank> Banks { get; set; } = StaticBanks;

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    // Form fields
    [ObservableProperty]
    public partial string FormCedula { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FormPhone { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Bank? FormSelectedBank { get; set; }

    [ObservableProperty]
    public partial string FormErrorMessage { get; set; } = string.Empty;

    public PagoMovilViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private IServiceScope CreateDbScope(out BcvDbContext dbContext)
    {
        var scope = _serviceProvider.CreateScope();
        dbContext = scope.ServiceProvider.GetRequiredService<BcvDbContext>();
        return scope;
    }

    // Partial change notification handlers
    partial void OnStatusMessageChanged(string value)
    {
        // Limpiar mensaje tras unos segundos de forma asíncrona
        if (!string.IsNullOrEmpty(value))
        {
            Task.Run(async () =>
            {
                await Task.Delay(3000);
                MainThread.BeginInvokeOnMainThread(() => StatusMessage = string.Empty);
            });
        }
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();

    public async Task InitializeAsync()
    {
        MainThread.BeginInvokeOnMainThread(() => Banks = StaticBanks);
        
        // Si ya tenemos registros cargados, actualizamos de forma silenciosa en segundo plano
        if (Records != null && Records.Count > 0)
        {
            _ = Task.Run(async () => await LoadRecordsAsync(silent: true));
            return;
        }

        await LoadRecordsAsync(silent: false);
    }

    [RelayCommand]
    public async Task LoadRecordsAsync(bool silent = false)
    {
        if (!silent)
        {
            MainThread.BeginInvokeOnMainThread(() => IsLoading = true);
        }

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
                    _allRecords = list;
                    ApplyFilter();
                    IsLoading = false;
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al cargar registros: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() => IsLoading = false);
        }
        finally
        {
            _dbSemaphore.Release();
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            Records = _allRecords;
        }
        else
        {
            string query = SearchQuery.Trim().ToLowerInvariant();
            var filtered = new List<PagoMovilRecord>();
            foreach (var r in _allRecords)
            {
                if (r.Cedula.ToLowerInvariant().Contains(query) ||
                    r.Phone.ToLowerInvariant().Contains(query) ||
                    r.BankName.ToLowerInvariant().Contains(query) ||
                    r.BankCode.ToLowerInvariant().Contains(query))
                {
                    filtered.Add(r);
                }
            }
            Records = filtered;
        }
    }

    [RelayCommand]
    private async Task AddRecordAsync(INavigation navigation)
    {
        FormErrorMessage = string.Empty;

        // Validaciones
        string cleanCedula = FormCedula.Trim();
        if (string.IsNullOrWhiteSpace(cleanCedula))
        {
            FormErrorMessage = "Por favor, ingrese el número de cédula.";
            return;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(cleanCedula, @"^\d+$"))
        {
            FormErrorMessage = "La cédula debe contener únicamente números (sin letras, guiones ni puntos).";
            return;
        }

        if (string.IsNullOrWhiteSpace(FormPhone))
        {
            FormErrorMessage = "Por favor, ingrese el número de teléfono.";
            return;
        }

        if (FormSelectedBank == null)
        {
            FormErrorMessage = "Por favor, seleccione un banco.";
            return;
        }

        // Verificar si ya existe un registro de Pago Móvil duplicado
        bool exists = false;
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                exists = await dbContext.PagoMovilRecords.AnyAsync(r =>
                    r.BankCode == FormSelectedBank.Code &&
                    r.Phone == FormPhone.Trim() &&
                    r.Cedula == cleanCedula);
            }
        }
        catch (Exception ex)
        {
            FormErrorMessage = $"Error al verificar duplicados: {ex.Message}";
            return;
        }
        finally
        {
            _dbSemaphore.Release();
        }

        if (exists)
        {
            FormErrorMessage = "Este registro de Pago Móvil ya existe (mismo banco, teléfono y cédula).";
            return;
        }

        var newRecord = new PagoMovilRecord
        {
            Cedula = cleanCedula,
            Phone = FormPhone.Trim(),
            BankCode = FormSelectedBank.Code,
            BankName = FormSelectedBank.Name,
            CreatedAt = DateTime.Now
        };

        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                dbContext.PagoMovilRecords.Add(newRecord);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            FormErrorMessage = $"Error al guardar en base de datos: {ex.Message}";
            return;
        }
        finally
        {
            _dbSemaphore.Release();
        }

        // Limpiar formulario
        FormCedula = string.Empty;
        FormPhone = string.Empty;
        FormSelectedBank = null;

        // Cargar registros fuera del bloqueo para evitar interbloqueos (deadlocks)
        await LoadRecordsAsync();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            StatusMessage = "Registro de Pago Móvil guardado con éxito.";
            await navigation.PopAsync();
        });
    }

    [RelayCommand]
    private async Task DeleteRecordAsync(PagoMovilRecord record)
    {
        await _dbSemaphore.WaitAsync();
        try
        {
            using (CreateDbScope(out var dbContext))
            {
                dbContext.PagoMovilRecords.Remove(record);
                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al eliminar registro: {ex.Message}");
        }
        finally
        {
            _dbSemaphore.Release();
        }

        // Cargar registros fuera del bloqueo para evitar interbloqueos (deadlocks)
        await LoadRecordsAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusMessage = "Registro eliminado con éxito.";
        });
    }

    [RelayCommand]
    private async Task CopyFieldAsync(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            await Clipboard.Default.SetTextAsync(text);
            StatusMessage = $"Dato copiado: {text}";
        }
    }

    [RelayCommand]
    private async Task CopyAllAsync(PagoMovilRecord record)
    {
        string text = $"{record.Cedula}\n{record.Phone}\n{record.BankCode} - {record.BankName}";
        await Clipboard.Default.SetTextAsync(text);
        StatusMessage = "Datos de Pago Móvil copiados (formato crudo).";
    }

    [RelayCommand]
    private async Task ShareRecordAsync(PagoMovilRecord record)
    {
        string text = $"Datos de Pago Móvil:\nBanco: {record.BankCode} - {record.BankName}\nCédula: {record.Cedula}\nTeléfono: {record.Phone}";
        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Text = text,
            Title = "Compartir Pago Móvil"
        });
    }

    // Static Fallback Banks
    public static readonly IReadOnlyList<Bank> StaticBanks = new List<Bank>
    {
        new("0102", "BANCO DE VENEZUELA"),
        new("0104", "BANCO VENEZOLANO DE CREDITO"),
        new("0105", "BANCO MERCANTIL"),
        new("0108", "BBVA PROVINCIAL"),
        new("0114", "BANCARIBE"),
        new("0115", "BANCO EXTERIOR"),
        new("0128", "BANCO CARONI"),
        new("0134", "BANESCO"),
        new("0137", "BANCO SOFITASA"),
        new("0138", "BANCO PLAZA"),
        new("0146", "BANGENTE"),
        new("0151", "BANCO FONDO COMUN"),
        new("0156", "100% BANCO"),
        new("0157", "DELSUR BANCO UNIVERSAL"),
        new("0163", "BANCO DEL TESORO"),
        new("0168", "BANCRECER"),
        new("0169", "R4 BANCO MICROFINANCIERO C.A."),
        new("0171", "BANCO ACTIVO"),
        new("0172", "BANCAMIGA BANCO UNIVERSAL, C.A."),
        new("0173", "BANCO INTERNACIONAL DE DESARROLLO"),
        new("0174", "BANPLUS"),
        new("0175", "BANCO DIGITAL DE LOS TRABAJADORES, BANCO UNIVERSAL"),
        new("0177", "BANFANB"),
        new("0178", "N58 BANCO DIGITAL BANCO MICROFINANCIERO S A"),
        new("0191", "BANCO NACIONAL DE CREDITO")
    };
}
