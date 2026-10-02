using SmartPantry.Despensa;
using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Despensa;

public class ItemDespensa : Entity<Guid>
{
    public Guid IdProducto { get; private set; }
    public bool EstaConsumido { get; private set; }

    // Value Objects mapeados
    public Cantidad Cantidad { get; private set; }
    public FechaVencimiento FechaVencimiento { get; private set; }
    public Nota Nota { get; private set; }

    private ItemDespensa() { } // Para EF Core

    public ItemDespensa(
        Guid id,
        Guid idProducto,
        Cantidad cantidad,
        FechaVencimiento fechaVencimiento = null,
        Nota nota = null) : base(id)
    {
        IdProducto = idProducto;
        Cantidad = cantidad;
        FechaVencimiento = fechaVencimiento;
        Nota = nota;
        EstaConsumido = false;
    }

    public void MarcarComoConsumido()
    {
        EstaConsumido = true;
    }
}