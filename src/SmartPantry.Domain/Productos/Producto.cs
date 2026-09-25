using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Productos;

public class Producto : AuditedAggregateRoot<Guid>
{
    public string Nombre { get; private set; }
    public string Marca { get; private set; }
    public string Categoria { get; private set; }
    public string UrlImagen { get; private set; }

    private Producto() { } // Requerido por EF Core

    public Producto(Guid id, string nombre, string marca, string categoria, string urlImagen) : base(id)
    {
        SetNombre(nombre);
        SetMarca(marca);
        SetCategoria(categoria);

        // UrlImagen puede ser opcional o aceptar nulos dependiendo de tus reglas de negocio
        UrlImagen = urlImagen?.Trim();
    }

    private void SetNombre(string nombre)
    {
        Check.NotNullOrWhiteSpace(nombre, nameof(nombre), ProductoConsts.MaxNombreLength);
        Nombre = nombre.Trim();
    }

    private void SetMarca(string marca)
    {
        Check.NotNullOrWhiteSpace(marca, nameof(marca), ProductoConsts.MaxMarcaLength);
        Marca = marca.Trim();
    }

    private void SetCategoria(string categoria)
    {
        Check.NotNullOrWhiteSpace(categoria, nameof(categoria), ProductoConsts.MaxCategoriaLength);
        Categoria = categoria.Trim();
    }
}