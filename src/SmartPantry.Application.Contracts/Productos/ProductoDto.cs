using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Productos;

public class ProductoDto : AuditedEntityDto<Guid>
{
    public string Nombre { get; set; }
    public string Marca { get; set; }
    public string Categoria { get; set; }
    public string UrlImagen { get; set; }
}

public class CreateProductoDto
{
    public string Nombre { get; set; }
    public string Marca { get; set; }
    public string Categoria { get; set; }
    public string UrlImagen { get; set; }
}

// Nuevo DTO para actualización
public class UpdateProductoDto
{
    public string Nombre { get; set; }
    public string Marca { get; set; }
    public string Categoria { get; set; }
    public string UrlImagen { get; set; }
}

// Nuevo DTO para paginación y ordenamiento de listas
public class GetProductosInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}