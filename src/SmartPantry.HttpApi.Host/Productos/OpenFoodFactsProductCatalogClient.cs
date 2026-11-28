using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SmartPantry.Productos;

namespace SmartPantry.HttpApi.Host.Clients;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;
    private readonly IExternalCatalogMetrics _metrics;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient, IExternalCatalogMetrics metrics)
    {
        _httpClient = httpClient;
        _metrics = metrics;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        barcode = barcode.Trim();
        const string fields = "code,product_name,product_name_es,brands,quantity,image_front_url,nutriments,ingredients_text,ingredients_text_es,allergens,allergens_tags";
        var path = $"product/{Uri.EscapeDataString(barcode)}?fields={Uri.EscapeDataString(fields)}";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.GetAsync(path);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _metrics.RecordNotFound(stopwatch.Elapsed);
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                _metrics.RecordError(stopwatch.Elapsed, $"HTTP {(int)response.StatusCode}");
                return null;
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
            if (!document.RootElement.TryGetProperty("product", out var product))
            {
                _metrics.RecordNotFound(stopwatch.Elapsed);
                return null;
            }

            var result = new ExternalProductDto
            {
                Code = GetText(product, "code"),
                Name = GetText(product, "product_name_es", "product_name"),
                Brand = GetText(product, "brands"),
                Quantity = GetText(product, "quantity"),
                ImageUrl = GetText(product, "image_front_url"),
                IngredientsText = GetText(product, "ingredients_text_es", "ingredients_text"),
                Allergens = GetTexts(product, "allergens_tags")
                    .Concat(SplitAllergens(GetText(product, "allergens")))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                EnergyKcal100g = GetNutrient(product, "energy-kcal_100g"),
                Fat100g = GetNutrient(product, "fat_100g"),
                Sugars100g = GetNutrient(product, "sugars_100g"),
                Salt100g = GetNutrient(product, "salt_100g")
            };
            _metrics.RecordSuccess(stopwatch.Elapsed);
            return result;
        }
        catch (TaskCanceledException)
        {
            _metrics.RecordError(stopwatch.Elapsed, "timeout");
            return null;
        }
        catch (HttpRequestException)
        {
            _metrics.RecordError(stopwatch.Elapsed, "network");
            return null;
        }
        catch (JsonException)
        {
            _metrics.RecordError(stopwatch.Elapsed, "invalid-response");
            return null;
        }
        catch (Exception)
        {
            _metrics.RecordError(stopwatch.Elapsed, "unexpected-error");
            return null;
        }
    }

    private static IEnumerable<string> GetTexts(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value) || value.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();
        return value.EnumerateArray()
            .Where(entry => entry.ValueKind == JsonValueKind.String)
            .Select(entry => entry.GetString())
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Select(text => text!.Trim())
            .ToList();
    }

    private static IEnumerable<string> SplitAllergens(string value) => value
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static string GetText(JsonElement element, params string[] properties)
    {
        foreach (var property in properties)
        {
            if (element.TryGetProperty(property, out var value) &&
                value.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(value.GetString()))
            {
                return value.GetString()!;
            }
        }
        return string.Empty;
    }

    private static decimal? GetNutrient(JsonElement product, string property)
    {
        if (product.TryGetProperty("nutriments", out var nutriments) &&
            nutriments.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.Number)
        {
            return value.GetDecimal();
        }
        return null;
    }
}
