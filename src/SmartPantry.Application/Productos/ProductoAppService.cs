using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

public class ProductoAppService :
    CrudAppService<
        Producto,
        ProductoDto,
        Guid,
        GetProductosInput,
        CreateProductoDto,
        UpdateProductoDto>,
    IProductoAppService
{
    public ProductoAppService(
        IRepository<Producto, Guid> repository)
        : base(repository)
    {
    }

    public override async Task<ProductoDto> UpdateAsync(
        Guid id,
        UpdateProductoDto input)
    {
        var producto = await Repository.GetAsync(id);

        producto.Actualizar(
            input.Nombre,
            input.Marca,
            input.Categoria,
            input.UrlImagen);

        await Repository.UpdateAsync(producto, autoSave: true);

        return ObjectMapper.Map<Producto, ProductoDto>(producto);
    }
}