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

    public Task ProcesarVencimientosAsync()
    {
        // La regla de negocio usa la fecha local del servidor.
        return ProcesarVencimientosAsync(DateTime.Today);
    }

    public async Task ProcesarVencimientosAsync(DateTime hoy)
    {
        hoy = hoy.Date;
        const int diasUmbral = 7;
        var items = await _itemRepository.GetListAsync();

        foreach (var item in items)
        {
            var advertencia = await _advertenciaRepository.FirstOrDefaultAsync(
                a => a.ItemDespensaId == item.Id);

            TipoAdvertencia? tipoEsperado = null;
            var fechaVencimiento = item.FechaVencimiento?.FechaVenc;

            if (!item.EstaConsumido && fechaVencimiento.HasValue)
            {
                var fecha = fechaVencimiento.Value.Date;
                if (fecha < hoy)
                {
                    tipoEsperado = TipoAdvertencia.Vencido;
                }
                else if (fecha <= hoy.AddDays(diasUmbral))
                {
                    tipoEsperado = TipoAdvertencia.ProximoAVencer;
                }
            }

            if (!tipoEsperado.HasValue)
            {
                if (advertencia?.EstaActiva == true)
                {
                    advertencia.Desactivar();
                    await _advertenciaRepository.UpdateAsync(advertencia);
                }

                continue;
            }

            var tipoTexto = tipoEsperado.Value.ToString();
            if (advertencia == null)
            {
                await _advertenciaRepository.InsertAsync(
                    new AdvertenciaVencimiento(GuidGenerator.Create(), item.Id, tipoTexto, fechaVencimiento!.Value));
                continue;
            }

            var fechaRegistrada = fechaVencimiento!.Value.Date;
            if (advertencia.TipoAdvertencia != tipoTexto ||
                advertencia.FechaVencimientoRegistrada?.Date != fechaRegistrada)
            {
                advertencia.Actualizar(tipoTexto, fechaRegistrada);
            }

            if (!advertencia.EstaActiva)
            {
                advertencia.Activar();
            }

            await _advertenciaRepository.UpdateAsync(advertencia);
        }
    }
}
