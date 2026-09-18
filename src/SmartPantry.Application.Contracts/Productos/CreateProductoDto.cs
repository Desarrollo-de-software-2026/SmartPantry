// Ruta: src/SmartPantry.Application.Contracts/Productos/CreateProductoDto.cs
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Productos;

public class CreateProductoDto
{
    [Required]
    [StringLength(ProductoConsts.MaxNombreLength)]
    public string Nombre { get; set; }

    [Required]
    [StringLength(ProductoConsts.MaxMarcaLength)]
    public string Marca { get; set; }

    [Required]
    [StringLength(ProductoConsts.MaxCategoriaLength)]
    public string Categoria { get; set; }

    [StringLength(ProductoConsts.MaxUrlImagenLength)]
    public string UrlImagen { get; set; }
}