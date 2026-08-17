using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BcvExchangeApp.Models;

namespace BcvExchangeApp.Services;

public partial class BcvScraperService
{
    private const string BcvUrl = "https://www.bcv.org.ve/";
    private const string BinanceP2pUrl = "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search";
    private const string BinanceUsdtWebUrl = "https://www.binance.com/es-LA/price/tether/VES";

    private readonly HttpClient _httpClient;

    public BcvScraperService()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        
        _httpClient = new HttpClient(handler);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<ExchangeRate?> ScrapeRatesAsync()
    {
        try
        {
            // 1. Descargar HTML de la página del BCV (Dólar y Euro)
            string html = await _httpClient.GetStringAsync(BcvUrl);

            var usdMatch = DolarRegex().Match(html);
            if (!usdMatch.Success)
            {
                throw new Exception("No se pudo encontrar el contenedor del Dólar en la página del BCV.");
            }
            string usdString = usdMatch.Groups[1].Value.Trim().Replace(",", ".");
            if (!double.TryParse(usdString, NumberStyles.Any, CultureInfo.InvariantCulture, out double usdRate))
            {
                throw new Exception($"No se pudo parsear el valor del Dólar: '{usdString}'");
            }

            var eurMatch = EuroRegex().Match(html);
            if (!eurMatch.Success)
            {
                throw new Exception("No se pudo encontrar el contenedor del Euro en la página del BCV.");
            }
            string eurString = eurMatch.Groups[1].Value.Trim().Replace(",", ".");
            if (!double.TryParse(eurString, NumberStyles.Any, CultureInfo.InvariantCulture, out double eurRate))
            {
                throw new Exception($"No se pudo parsear el valor del Euro: '{eurString}'");
            }

            var dateMatch = DateRegex().Match(html);
            DateTime dateValue = DateTime.Today;
            if (dateMatch.Success)
            {
                string rawDate = dateMatch.Groups[1].Value;
                if (DateTime.TryParse(rawDate, out DateTime parsedDate))
                {
                    dateValue = parsedDate.Date;
                }
            }

            // 2. Obtener tasa USDT de Binance (API P2P en tiempo real con fallback a web scraping)
            double usdtRate = await FetchBinanceUsdtRateAsync();

            return new ExchangeRate
            {
                Date = dateValue,
                UsdRate = usdRate,
                EurRate = eurRate,
                UsdtRate = usdtRate,
                CreatedAt = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de scraping BCV/Binance: {ex.Message}");
            throw;
        }
    }

    private async Task<double> FetchBinanceUsdtRateAsync()
    {
        // Método 1: Petición directa a la API P2P C2C oficial de Binance (Datos JSON en tiempo real sin pasar por renderizado HTML/React)
        try
        {
            var jsonPayload = new
            {
                asset = "USDT",
                fiat = "VES",
                merchantCheck = false,
                page = 1,
                rows = 5,
                tradeType = "BUY"
            };

            string jsonString = JsonSerializer.Serialize(jsonPayload);
            using var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(BinanceP2pUrl, content);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var dataArray) && dataArray.ValueKind == JsonValueKind.Array && dataArray.GetArrayLength() > 0)
                {
                    var firstAd = dataArray[0];
                    if (firstAd.TryGetProperty("adv", out var advObj) && advObj.TryGetProperty("price", out var priceProp))
                    {
                        string priceStr = priceProp.GetString() ?? string.Empty;
                        if (double.TryParse(priceStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double p2pPrice))
                        {
                            return p2pPrice;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al consultar API P2P de Binance: {ex.Message}");
        }

        // Método 2 (Fallback): Scraping HTML / Meta-tags
        try
        {
            string binanceHtml = await _httpClient.GetStringAsync(BinanceUsdtWebUrl);

            var usdtMatch = UsdtDivRegex().Match(binanceHtml);
            if (!usdtMatch.Success)
            {
                usdtMatch = UsdtMetaRegex().Match(binanceHtml);
            }

            if (usdtMatch.Success)
            {
                string usdtString = usdtMatch.Groups[1].Value.Trim().Replace("&nbsp;", "").Replace(" ", "").Replace(",", ".");
                if (double.TryParse(usdtString, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedUsdt))
                {
                    return parsedUsdt;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en fallback de scraping Binance HTML: {ex.Message}");
        }

        return 0.0;
    }

    [GeneratedRegex(@"id=""dolar""[\s\S]*?<strong[^>]*?>([\s\S]*?)<\/strong>")]
    private static partial Regex DolarRegex();

    [GeneratedRegex(@"id=""euro""[\s\S]*?<strong[^>]*?>([\s\S]*?)<\/strong>")]
    private static partial Regex EuroRegex();

    [GeneratedRegex(@"date-display-single""[^>]*?content=""([^""]+)""")]
    private static partial Regex DateRegex();

    [GeneratedRegex(@"class=""[^""]*text-PrimaryText[^""]*t-body1[^""]*""[^>]*>\s*VES(?:&nbsp;|\s)+([0-9.,]+)\s*<\/div>", RegexOptions.IgnoreCase)]
    private static partial Regex UsdtDivRegex();

    [GeneratedRegex(@"1\s*USDT\s*=\s*([0-9.,]+)\s*VES", RegexOptions.IgnoreCase)]
    private static partial Regex UsdtMetaRegex();
}
