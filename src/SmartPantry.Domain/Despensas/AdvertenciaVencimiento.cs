using System;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Despensa;

public class AdvertenciaVencimiento : Entity<Guid>
{
    public Guid ItemDespensaId { get; private set; }
    public string TipoAdvertencia { get; private set; }
    public DateTime? FechaVencimientoRegistrada { get; private set; }
    public bool EstaActiva { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    private AdvertenciaVencimiento() { }

    public AdvertenciaVencimiento(Guid id, Guid itemDespensaId, string tipoAdvertencia, DateTime fechaVencimiento) : base(id)
    {
        ItemDespensaId = itemDespensaId;
        TipoAdvertencia = tipoAdvertencia;
        FechaVencimientoRegistrada = fechaVencimiento.Date;
        EstaActiva = true;
        FechaCreacion = DateTime.UtcNow;
    }

    public void ActualizarTipo(string tipoAdvertencia)
    {
        TipoAdvertencia = tipoAdvertencia;
    }

    public void Actualizar(string tipoAdvertencia, DateTime fechaVencimiento)
    {
        TipoAdvertencia = tipoAdvertencia;
        FechaVencimientoRegistrada = fechaVencimiento.Date;
    }

    public void Desactivar() => EstaActiva = false;
    public void Activar() => EstaActiva = true;
}
