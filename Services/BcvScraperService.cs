using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BcvExchangeApp.Models;

namespace BcvExchangeApp.Services;

public class BcvScraperService
{
    private const string BcvUrl = "https://www.bcv.org.ve/";
    private readonly HttpClient _httpClient;

    public BcvScraperService()
    {
        // Ignoramos errores de certificados SSL/TLS para evitar bloqueos por certificados no reconocidos en móviles/desarrollo
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        
        _httpClient = new HttpClient(handler);
        // User Agent para emular a un navegador moderno y evitar bloqueos por user-agent vacío
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        // Tiempo de espera para evitar bloqueos prolongados
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<ExchangeRate?> ScrapeRatesAsync()
    {
        try
        {
            // Descargar el contenido HTML de la página del BCV
            string html = await _httpClient.GetStringAsync(BcvUrl);

            // 1. Extraer la tasa del Dólar
            var usdMatch = Regex.Match(html, @"id=""dolar""[\s\S]*?<strong[^>]*?>([\s\S]*?)<\/strong>");
            if (!usdMatch.Success)
            {
                throw new Exception("No se pudo encontrar el contenedor del Dólar en la página.");
            }
            string usdString = usdMatch.Groups[1].Value.Trim().Replace(",", ".");
            if (!double.TryParse(usdString, NumberStyles.Any, CultureInfo.InvariantCulture, out double usdRate))
            {
                throw new Exception($"No se pudo parsear el valor del Dólar: '{usdString}'");
            }

            // 2. Extraer la tasa del Euro
            var eurMatch = Regex.Match(html, @"id=""euro""[\s\S]*?<strong[^>]*?>([\s\S]*?)<\/strong>");
            if (!eurMatch.Success)
            {
                throw new Exception("No se pudo encontrar el contenedor del Euro en la página.");
            }
            string eurString = eurMatch.Groups[1].Value.Trim().Replace(",", ".");
            if (!double.TryParse(eurString, NumberStyles.Any, CultureInfo.InvariantCulture, out double eurRate))
            {
                throw new Exception($"No se pudo parsear el valor del Euro: '{eurString}'");
            }

            // 3. Extraer la fecha valor (Fecha de vigencia)
            // Estructura: <span class="date-display-single" property="dc:date" datatype="xsd:dateTime" content="2026-06-16T00:00:00-04:00">
            var dateMatch = Regex.Match(html, @"date-display-single""[^>]*?content=""([^""]+)""");
            DateTime dateValue = DateTime.Today; // Fallback
            if (dateMatch.Success)
            {
                string rawDate = dateMatch.Groups[1].Value;
                if (DateTime.TryParse(rawDate, out DateTime parsedDate))
                {
                    dateValue = parsedDate.Date; // Nos quedamos solo con la fecha (medianoche)
                }
            }

            return new ExchangeRate
            {
                Date = dateValue,
                UsdRate = usdRate,
                EurRate = eurRate,
                CreatedAt = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error de scraping BCV: {ex.Message}");
            throw; // Propagar para que el ViewModel lo maneje
        }
    }
}
