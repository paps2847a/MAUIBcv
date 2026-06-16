using System;

namespace BcvExchangeApp.Models;

public class ExchangeRate
{
    public int Id { get; set; }
    
    // Fecha valor reportada por el BCV (guardada a medianoche, 00:00:00, para facilitar búsquedas)
    public DateTime Date { get; set; }
    
    // Tasa del Dólar (USD) en Bolívares (VES)
    public double UsdRate { get; set; }
    
    // Tasa del Euro (EUR) en Bolívares (VES)
    public double EurRate { get; set; }
    
    // Fecha y hora en la que se descargó/guardó el registro localmente
    public DateTime CreatedAt { get; set; }
}
