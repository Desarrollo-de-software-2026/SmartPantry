using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

[Authorize]
public class ProductoAppService : CrudAppService<
    Producto,
    ProductoDto,
    Guid,
    GetProductosInput,
    CreateProductoDto,
    UpdateProductoDto>,
    IProductoAppService
{
    private readonly IExternalProductCatalogClient? _externalProductCatalogClient;

    public ProductoAppService(IRepository<Producto, Guid> repository) : base(repository)
    {
    }

    public ProductoAppService(
        IRepository<Producto, Guid> repository,
        IExternalProductCatalogClient externalProductCatalogClient) : base(repository)
    {
        _externalProductCatalogClient = externalProductCatalogClient;
    }

    public Task<ExternalProductDto?> GetExternalAsync(GetExternalProductInput input)
    {
        return CallExternalAsync(() => GetExternalClient().GetByBarcodeAsync(input.Barcode.Trim()));
    }

    public async Task<PagedResultDto<ExternalProductDto>> SearchExternalAsync(
        SearchExternalProductsInput input)
    {
        // El servicio externo interpreta algunos símbolos como operadores de búsqueda.
        // Los reemplazamos por espacios para que la entrada se procese como texto literal.
        var query = Regex.Replace(input.Query ?? string.Empty, @"[^\p{L}\p{N}\s]", " ");
        query = Regex.Replace(query, @"\s+", " ").Trim();
        if (query.Length < 2)
        {
            throw new UserFriendlyException("Ingresá al menos dos letras o números para buscar.");
        }

        return await CallExternalAsync(() =>
            GetExternalClient().SearchAsync(query, input.Page, input.PageSize));
    }

    public async Task<SaveExternalProductResultDto> SaveExternalAsync(
        GetExternalProductInput input)
    {
        var barcode = input.Barcode.Trim();
        var existing = await Repository.FirstOrDefaultAsync(
            product => product.CodigoBarras == barcode);

        if (existing != null)
        {
            return new SaveExternalProductResultDto
            {
                Producto = ObjectMapper.Map<Producto, ProductoDto>(existing),
                YaExistia = true
            };
        }

        var externalProduct = await CallExternalAsync(
            () => GetExternalClient().GetByBarcodeAsync(barcode));
        if (externalProduct == null)
        {
            throw new UserFriendlyException(
                "No se encontró ese producto en el catálogo externo.");
        }

        if (string.IsNullOrWhiteSpace(externalProduct.Name))
        {
            throw new UserFriendlyException(
                "El producto no tiene nombre disponible y no se puede guardar.");
        }

        var product = new Producto(
            GuidGenerator.Create(),
            externalProduct.Name,
            MissingValue(externalProduct.Brand),
            MissingValue(externalProduct.Category),
            externalProduct.ImageUrl ?? string.Empty);
        product.EstablecerCodigoBarras(barcode);

        await Repository.InsertAsync(product, autoSave: true);

        return new SaveExternalProductResultDto
        {
            Producto = ObjectMapper.Map<Producto, ProductoDto>(product),
            YaExistia = false
        };
    }

    private async Task<T> CallExternalAsync<T>(Func<Task<T>> request)
    {
        try
        {
            return await request();
        }
        catch (HttpRequestException exception) when (
            exception.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new UserFriendlyException(L["Catalog:ExternalRateLimited"]);
        }
        catch (HttpRequestException)
        {
            throw new UserFriendlyException(L["Catalog:ExternalUnavailable"]);
        }
        catch (TaskCanceledException)
        {
            throw new UserFriendlyException(L["Catalog:ExternalUnavailable"]);
        }
    }

    private IExternalProductCatalogClient GetExternalClient()
    {
        return _externalProductCatalogClient
            ?? throw new InvalidOperationException("El cliente externo no está configurado.");
    }

    private static string MissingValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "No disponible" : value.Trim();
    }
}
