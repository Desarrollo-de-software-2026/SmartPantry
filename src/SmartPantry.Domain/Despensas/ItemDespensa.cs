using SmartPantry.Despensa;
using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Despensa;

public class ItemDespensa : Entity<Guid>
{
    public Guid DespensaId { get; private set; }
    public Guid IdProducto { get; private set; }
    public bool EstaConsumido { get; private set; }

    // Cantidad/unidad y vencimiento son opcionales según RF-10.
    public Cantidad? Cantidad { get; private set; }
    public FechaVencimiento? FechaVencimiento { get; private set; }
    public Nota Nota { get; private set; }

    private ItemDespensa() { } // Para EF Core

    public ItemDespensa(
        Guid id,
        Guid idProducto,
        Cantidad? cantidad = null,
        FechaVencimiento? fechaVencimiento = null,
        Nota nota = null) : base(id)
    {
        IdProducto = idProducto;
        Cantidad = cantidad;
        FechaVencimiento = fechaVencimiento;
        Nota = nota;
        EstaConsumido = false;
    }

    internal void AsignarADespensa(Guid despensaId)
    {
        if (DespensaId != Guid.Empty && DespensaId != despensaId)
        {
            throw new InvalidOperationException("El ítem ya pertenece a otra despensa.");
        }

        DespensaId = despensaId;
    }

    public void ActualizarCantidad(Cantidad? cantidad)
    {
        Cantidad = cantidad;
    }

    public void ActualizarFechaVencimiento(FechaVencimiento? fechaVencimiento)
    {
        FechaVencimiento = fechaVencimiento;
    }

    public void MarcarComoConsumido()
    {
        EstaConsumido = true;
    }
}
