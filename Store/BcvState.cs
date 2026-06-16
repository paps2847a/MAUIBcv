using System;
using System.Collections.Generic;
using BcvExchangeApp.Models;

namespace BcvExchangeApp.Store;

public record BcvState(
    bool IsBusy,
    string StatusMessage,
    double UsdRate,
    double EurRate,
    DateTime SelectedDate,
    string FormattedDate,
    string AmountText,
    string SelectedCurrency, // "USD" o "EUR"
    bool IsToVes, // True: Divisa -> VES, False: VES -> Divisa
    string ConversionResult,
    IReadOnlyList<ExchangeRate> History
);
