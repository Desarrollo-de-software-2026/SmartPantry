using System;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Productos;

public class ProductoDto : AuditedEntityDto<Guid>
{
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
}

public class CreateProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
}

public class UpdateProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string UrlImagen { get; set; } = string.Empty;
}

public class GetProductosInput : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; } = string.Empty;
}
