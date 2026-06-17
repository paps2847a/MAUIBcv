using System;

namespace BcvExchangeApp.Models;

public class PagoMovilRecord
{
    public int Id { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string DisplayName => $"{BankName} - {Cedula}";
}
