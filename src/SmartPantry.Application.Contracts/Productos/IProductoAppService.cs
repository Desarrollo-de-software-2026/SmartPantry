using System;
using Volo.Abp.Application.Services;

namespace SmartPantry.Productos;

public interface IProductoAppService : ICrudAppService<
    ProductoDto,          // DTO para mostrar los datos
    Guid,                 // Tipo de la clave primaria (ID)
    GetProductosInput,    // DTO para paginación, filtros y orden
    CreateProductoDto,    // DTO para la creación
    UpdateProductoDto>    // DTO para la modificación
{
}