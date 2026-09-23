using Microsoft.AspNetCore.Authorization;
using SmartPantry.Productos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

[AllowAnonymous] // Acceso temporal para probar en Swagger sin token
public class ProductoAppService : SmartPantryAppService, IProductoAppService
{
    private readonly IRepository<Producto, Guid> _productoRepository;

    public ProductoAppService(IRepository<Producto, Guid> productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public async Task<ProductoDto> CreateAsync(CreateProductoDto input)
    {
        var producto = new Producto(
            GuidGenerator.Create(),
            input.Nombre,
            input.Marca,
            input.Categoria,
            input.UrlImagen
        );

        await _productoRepository.InsertAsync(producto);

        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Marca = producto.Marca,
            Categoria = producto.Categoria,
            UrlImagen = producto.UrlImagen
        };
    }

    public async Task<ProductoDto> GetAsync(Guid id)
    {
        var producto = await _productoRepository.GetAsync(id);
        return new ProductoDto
        {
            Id = producto.Id,
            Nombre = producto.Nombre,
            Marca = producto.Marca,
            Categoria = producto.Categoria,
            UrlImagen = producto.UrlImagen
        };
    }
}