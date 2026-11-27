using SmartPantry.Productos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.HttpApi.Host.Clients;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private static readonly Uri SearchApiUri = new("https://search.openfoodfacts.org/search");
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenFoodFactsProductCatalogClient> _logger;

    public OpenFoodFactsProductCatalogClient(
        HttpClient httpClient,
        ILogger<OpenFoodFactsProductCatalogClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        barcode = barcode.Trim();
        const string fields = "code,product_name,product_name_es,brands,categories,quantity,image_front_url,ingredients_text,ingredients_text_es,allergens,nutriscore_grade,nova_group,nutriments";
        var path = $"product/{Uri.EscapeDataString(barcode)}?fields={Uri.EscapeDataString(fields)}";
        var timer = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.GetAsync(path);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation(
                    "Consulta Open Food Facts por código finalizada en {ElapsedMs} ms. Encontrado: {Found}",
                    timer.ElapsedMilliseconds,
                    false);
                return null;
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException(
                    "El catálogo externo limitó temporalmente las consultas.",
                    inner: null,
                    statusCode: HttpStatusCode.TooManyRequests);
            }

            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
            if (!document.RootElement.TryGetProperty("product", out var product))
            {
                _logger.LogInformation(
                    "Consulta Open Food Facts por código finalizada en {ElapsedMs} ms. Encontrado: {Found}",
                    timer.ElapsedMilliseconds,
                    false);
                return null;
            }

            var result = MapProduct(product);
            _logger.LogInformation(
                "Consulta Open Food Facts por código finalizada en {ElapsedMs} ms. Encontrado: {Found}",
                timer.ElapsedMilliseconds,
                true);
            return result;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Falló la consulta Open Food Facts por código después de {ElapsedMs} ms.",
                timer.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<PagedResultDto<ExternalProductDto>> SearchAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            q = query.Trim(),
            page,
            page_size = pageSize,
            langs = new[] { "es", "en" },
            fields = new[]
            {
                "code", "product_name", "product_name_es", "brands", "categories", "quantity",
                "image_front_url", "ingredients_text", "ingredients_text_es", "allergens",
                "nutriscore_grade", "nova_group", "nutriments"
            }
        };
        var timer = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                SearchApiUri,
                request,
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException(
                    "El catálogo externo limitó temporalmente las búsquedas.",
                    inner: null,
                    statusCode: HttpStatusCode.TooManyRequests);
            }

            response.EnsureSuccessStatusCode();
            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            var totalCount = document.RootElement.TryGetProperty("count", out var count) &&
                             count.ValueKind == JsonValueKind.Number &&
                             count.TryGetInt64(out var parsedCount)
                ? parsedCount
                : 0;
            var products = new List<ExternalProductDto>();

            if (document.RootElement.TryGetProperty("hits", out var hits) &&
                hits.ValueKind == JsonValueKind.Array)
            {
                foreach (var hit in hits.EnumerateArray())
                {
                    var product = MapProduct(hit);
                    if (!string.IsNullOrWhiteSpace(product.Code))
                    {
                        products.Add(product);
                    }
                }
            }

            // Prioriza productos con imagen y datos básicos; los datos faltantes
            // quedan al final sin eliminar esos resultados de la búsqueda.
            products = products
                .OrderByDescending(HasImageAndBasicData)
                .ThenByDescending(product => !string.IsNullOrWhiteSpace(product.ImageUrl))
                .ThenByDescending(CompletenessScore)
                .ToList();

            _logger.LogInformation(
                "Búsqueda Open Food Facts finalizada en {ElapsedMs} ms. Resultados devueltos: {ResultCount}",
                timer.ElapsedMilliseconds,
                products.Count);
            return new PagedResultDto<ExternalProductDto>(totalCount, products);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Falló la búsqueda Open Food Facts después de {ElapsedMs} ms.",
                timer.ElapsedMilliseconds);
            throw;
        }
    }

    private static ExternalProductDto MapProduct(JsonElement product)
    {
        return new ExternalProductDto
        {
            Code = GetText(product, "code"),
            Name = GetText(product, "product_name_es", "product_name"),
            Brand = GetText(product, "brands"),
            Category = GetText(product, "categories"),
            Quantity = GetText(product, "quantity"),
            ImageUrl = GetText(product, "image_front_url"),
            Ingredients = GetText(product, "ingredients_text_es", "ingredients_text"),
            Allergens = GetText(product, "allergens"),
            NutriScore = GetText(product, "nutriscore_grade"),
            NovaGroup = GetInteger(product, "nova_group"),
            EnergyKcal100g = GetNutrient(product, "energy-kcal_100g"),
            Fat100g = GetNutrient(product, "fat_100g"),
            Sugars100g = GetNutrient(product, "sugars_100g"),
            Salt100g = GetNutrient(product, "salt_100g")
        };
    }

    private static bool HasImageAndBasicData(ExternalProductDto product)
    {
        var basicDataCount = 0;
        if (!string.IsNullOrWhiteSpace(product.Name)) basicDataCount++;
        if (!string.IsNullOrWhiteSpace(product.Brand)) basicDataCount++;
        if (!string.IsNullOrWhiteSpace(product.Category)) basicDataCount++;
        if (!string.IsNullOrWhiteSpace(product.Quantity)) basicDataCount++;

        return !string.IsNullOrWhiteSpace(product.ImageUrl) && basicDataCount >= 2;
    }

    private static int CompletenessScore(ExternalProductDto product)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(product.Name)) score++;
        if (!string.IsNullOrWhiteSpace(product.Brand)) score++;
        if (!string.IsNullOrWhiteSpace(product.Category)) score++;
        if (!string.IsNullOrWhiteSpace(product.Quantity)) score++;
        if (!string.IsNullOrWhiteSpace(product.Ingredients)) score++;
        if (!string.IsNullOrWhiteSpace(product.Allergens)) score++;
        if (!string.IsNullOrWhiteSpace(product.NutriScore)) score++;
        if (product.NovaGroup.HasValue) score++;
        if (product.EnergyKcal100g.HasValue) score++;
        if (product.Fat100g.HasValue) score++;
        if (product.Sugars100g.HasValue) score++;
        if (product.Salt100g.HasValue) score++;
        return score;
    }

    private static string GetText(JsonElement element, params string[] properties)
    {
        foreach (var property in properties)
        {
            if (!element.TryGetProperty(property, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(value.GetString()))
            {
                return value.GetString()!;
            }

            if (value.ValueKind == JsonValueKind.Array)
            {
                var values = new List<string>();
                foreach (var item in value.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(item.GetString()))
                    {
                        values.Add(item.GetString()!);
                    }
                }

                if (values.Count > 0)
                {
                    return string.Join(", ", values);
                }
            }
        }

        return string.Empty;
    }

    private static int? GetInteger(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
        {
            return number;
        }

        return value.ValueKind == JsonValueKind.String &&
               int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number)
            ? number
            : null;
    }

    private static decimal? GetNutrient(JsonElement product, string property)
    {
        if (product.TryGetProperty("nutriments", out var nutriments) &&
            nutriments.ValueKind == JsonValueKind.Object &&
            nutriments.TryGetProperty(property, out var value) &&
            value.ValueKind == JsonValueKind.Number &&
            value.TryGetDecimal(out var amount))
        {
            return amount;
        }

        return null;
    }
}
