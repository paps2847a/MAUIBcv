using System;
using System.Collections.Generic;
using BcvExchangeApp.Models;

namespace BcvExchangeApp.Store;

public abstract record BcvMsg;

// Mensajes de usuario
public record FetchLatestRates : BcvMsg;
public record SelectDate(DateTime Date) : BcvMsg;
public record SelectCurrency(string Currency) : BcvMsg;
public record ChangeAmount(string Amount) : BcvMsg;
public record ToggleDirection : BcvMsg;

// Mensajes del sistema (Efectos)
public record LoadingStarted : BcvMsg;
public record RatesLoaded(double UsdRate, double EurRate, DateTime Date, string StatusMessage) : BcvMsg;
public record RatesFailed(string StatusMessage, double? UsdRate, double? EurRate, DateTime? Date) : BcvMsg;
public record HistoryLoaded(IReadOnlyList<ExchangeRate> History) : BcvMsg;
