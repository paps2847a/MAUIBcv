using System;

namespace BcvExchangeApp.Models;

public class ShoppingItem
{
    public int Id { get; set; }
    
    // Nombre opcional
    public string Name { get; set; } = string.Empty;
    
    public double Price { get; set; }
    
    // Moneda: "USD", "EUR" o "VES"
    public string Currency { get; set; } = "USD";
    
    public int Quantity { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public double TotalPrice => Price * Quantity;
}
