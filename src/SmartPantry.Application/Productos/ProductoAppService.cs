using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

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

    // 1. Constructor para el CRUD clásico (usado por las pruebas anteriores y el contenedor base)
    public ProductoAppService(IRepository<Producto, Guid> repository) : base(repository)
    {
    }

    // 2. Constructor para cuando se inyecta el cliente externo (usado por el Host y el TP07)
    public ProductoAppService(
        IRepository<Producto, Guid> repository,
        IExternalProductCatalogClient externalProductCatalogClient) : base(repository)
    {
        _externalProductCatalogClient = externalProductCatalogClient;
    }

    // Método del TP07: Consulta externa por código de barras
    public async Task<ExternalProductDto?> GetExternalAsync(GetExternalProductInput input)
    {
        if (_externalProductCatalogClient == null)
        {
            throw new InvalidOperationException("El cliente externo no está configurado.");
        }

        return await _externalProductCatalogClient.GetByBarcodeAsync(input.Barcode);
    }
}