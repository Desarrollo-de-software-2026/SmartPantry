using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SmartPantry.Productos;

namespace SmartPantry.HttpApi.Host.Clients; // O el namespace que prefieras en el Host

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        barcode = barcode.Trim();
        const string fields = "code,product_name,product_name_es,brands,quantity,image_front_url,nutriments";
        var path = $"product/{Uri.EscapeDataString(barcode)}?fields={Uri.EscapeDataString(fields)}";

        try
        {
            using var response = await _httpClient.GetAsync(path);

            if (response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
            if (!document.RootElement.TryGetProperty("product", out var product))
            {
                return null;
            }

            return new ExternalProductDto
            {
                Code = GetText(product, "code"),
                Name = GetText(product, "product_name_es", "product_name"),
                Brand = GetText(product, "brands"),
                Quantity = GetText(product, "quantity"),
                ImageUrl = GetText(product, "image_front_url"),
                EnergyKcal100g = GetNutrient(product, "energy-kcal_100g"),
                Fat100g = GetNutrient(product, "fat_100g"),
                Sugars100g = GetNutrient(product, "sugars_100g"),
                Salt100g = GetNutrient(product, "salt_100g")
            };
        }
        catch
        {
            return null;
        }
    }

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