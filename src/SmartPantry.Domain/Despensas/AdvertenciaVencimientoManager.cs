using SmartPantry.Despensa;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SmartPantry.Despensas;

public class AdvertenciaVencimientoManager : DomainService
{
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciaRepository;

    public AdvertenciaVencimientoManager(
        IRepository<ItemDespensa, Guid> itemRepository,
        IRepository<AdvertenciaVencimiento, Guid> advertenciaRepository)
    {
        _itemRepository = itemRepository;
        _advertenciaRepository = advertenciaRepository;
    }

    public async Task ProcesarVencimientosAsync()
    {
        var hoy = DateTime.Today;
        const int diasUmbral = 7;
        var items = await _itemRepository.GetListAsync();

        foreach (var item in items)
        {
            var fechaVencimiento = item.FechaVencimiento?.FechaVenc;
            if (!fechaVencimiento.HasValue)
            {
                continue;
            }

            TipoAdvertencia? tipo = null;
            var fecha = fechaVencimiento.Value.Date;
            if (fecha < hoy)
            {
                tipo = TipoAdvertencia.Vencido;
            }
            else if (fecha <= hoy.AddDays(diasUmbral))
            {
                tipo = TipoAdvertencia.ProximoAVencer;
            }

            if (!tipo.HasValue)
            {
                continue;
            }

            var tipoTexto = tipo.Value.ToString();
            var yaExiste = await _advertenciaRepository.AnyAsync(a =>
                a.ItemDespensaId == item.Id && a.TipoAdvertencia == tipoTexto);

            if (!yaExiste)
            {
                await _advertenciaRepository.InsertAsync(new AdvertenciaVencimiento(
                    GuidGenerator.Create(), item.Id, tipoTexto));
            }
        }
    }
}
