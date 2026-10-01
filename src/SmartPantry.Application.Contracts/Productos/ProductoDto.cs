using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.Productos;

public class ProductoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Marca { get; set; }
    public string Categoria { get; set; }
    public string UrlImagen { get; set; }
}

