using System;

namespace BcvExchangeApp.Models;

public class PurchaseRecord
{
    public int Id { get; set; }
    
    public DateTime PurchaseDate { get; set; } = DateTime.Now;
    
    public double TotalVes { get; set; }
    
    public double TotalUsd { get; set; }
    
    public double TotalEur { get; set; }
    
    public string ItemSummary { get; set; } = string.Empty; // Resumen de los artículos (ej: "Harina PAN (2), Café (1)")
    
    public string ItemsJson { get; set; } = string.Empty; // Artículos serializados en JSON
    
    public double UsdRate { get; set; }
    
    public double EurRate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
