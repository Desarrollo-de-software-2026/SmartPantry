using SmartPantry.Despensa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.SettingManagement;
using SmartPantry.Preferencias;

namespace SmartPantry.Despensas;

public class AdvertenciaVencimientoManager : DomainService
{
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciaRepository;
    private readonly IRepository<Despensa, Guid> _despensaRepository;
    private readonly ISettingManager _settingManager;

    public AdvertenciaVencimientoManager(
        IRepository<ItemDespensa, Guid> itemRepository,
        IRepository<AdvertenciaVencimiento, Guid> advertenciaRepository,
        IRepository<Despensa, Guid> despensaRepository,
        ISettingManager settingManager)
    {
        _itemRepository = itemRepository;
        _advertenciaRepository = advertenciaRepository;
        _despensaRepository = despensaRepository;
        _settingManager = settingManager;
    }

    public Task ProcesarVencimientosAsync()
    {
        // La regla de negocio usa la fecha local del servidor.
        return ProcesarVencimientosAsync(DateTime.Today);
    }

    public async Task ProcesarVencimientosAsync(DateTime hoy)
    {
        hoy = hoy.Date;
        var items = await _itemRepository.GetListAsync();
        var pantries = await _despensaRepository.GetListAsync();
        var pantryOwners = pantries.ToDictionary(x => x.Id, x => x.IdUsuario);
        var userPreferences = new Dictionary<Guid, PreferenciasAlimentariasDto>();

        foreach (var item in items)
        {
            var advertencia = await _advertenciaRepository.FirstOrDefaultAsync(
                a => a.ItemDespensaId == item.Id);

            TipoAdvertencia? tipoEsperado = null;
            var fechaVencimiento = item.FechaVencimiento?.FechaVenc;
            var preferences = pantryOwners.TryGetValue(item.DespensaId, out var userId)
                ? await GetPreferencesAsync(userId, userPreferences)
                : new PreferenciasAlimentariasDto();

            if (!preferences.AlertasVencimiento)
            {
                if (advertencia?.EstaActiva == true)
                {
                    advertencia.Desactivar();
                    await _advertenciaRepository.UpdateAsync(advertencia);
                }
                continue;
            }

            if (!item.EstaConsumido && fechaVencimiento.HasValue)
            {
                var fecha = fechaVencimiento.Value.Date;
                if (fecha < hoy)
                {
                    tipoEsperado = TipoAdvertencia.Vencido;
                }
                else if (fecha <= hoy.AddDays(preferences.DiasAnticipacion))
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

    private async Task<PreferenciasAlimentariasDto> GetPreferencesAsync(
        Guid userId,
        IDictionary<Guid, PreferenciasAlimentariasDto> cache)
    {
        if (cache.TryGetValue(userId, out var cached)) return cached;

        var json = await _settingManager.GetOrNullAsync(
            SmartPantry.Settings.SmartPantrySettings.PreferenciasAlimentarias,
            "U",
            userId.ToString(),
            true);
        PreferenciasAlimentariasDto preferences;
        try
        {
            preferences = string.IsNullOrWhiteSpace(json)
                ? new PreferenciasAlimentariasDto()
                : JsonSerializer.Deserialize<PreferenciasAlimentariasDto>(json) ?? new PreferenciasAlimentariasDto();
        }
        catch (JsonException)
        {
            preferences = new PreferenciasAlimentariasDto();
        }

        preferences.DiasAnticipacion = Math.Clamp(preferences.DiasAnticipacion, 1, 30);
        cache[userId] = preferences;
        return preferences;
    }
}
