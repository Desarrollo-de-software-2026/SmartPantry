using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SmartPantry.Despensa;
using SmartPantry.Productos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Despensas;

/// <summary>
/// Administra los ítems de la despensa del usuario autenticado.
/// Un ítem que no existe o pertenece a otra persona se responde como 404.
/// </summary>
[Authorize]
public class ItemDespensaAppService : ApplicationService, IItemDespensaAppService
{
    private const int MaxPageSize = 100;

    private readonly IRepository<Despensa, Guid> _despensaRepository;
    private readonly IRepository<ItemDespensa, Guid> _itemRepository;
    private readonly IRepository<Producto, Guid> _productoRepository;
    private readonly IRepository<AdvertenciaVencimiento, Guid> _advertenciaRepository;

    public ItemDespensaAppService(
        IRepository<Despensa, Guid> despensaRepository,
        IRepository<ItemDespensa, Guid> itemRepository,
        IRepository<Producto, Guid> productoRepository,
        IRepository<AdvertenciaVencimiento, Guid> advertenciaRepository)
    {
        _despensaRepository = despensaRepository;
        _itemRepository = itemRepository;
        _productoRepository = productoRepository;
        _advertenciaRepository = advertenciaRepository;
    }

    public async Task<PagedResultDto<ItemDespensaDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var userId = GetCurrentUserId();
        var ownedPantries = await _despensaRepository.GetListAsync(x => x.IdUsuario == userId);
        var pantryIds = ownedPantries.Select(x => x.Id).ToList();
        if (pantryIds.Count == 0)
        {
            return new PagedResultDto<ItemDespensaDto>(0, new List<ItemDespensaDto>());
        }

        var queryable = await _itemRepository.GetQueryableAsync();
        var query = queryable.Where(x => pantryIds.Contains(x.DespensaId));
        var totalCount = await AsyncExecuter.CountAsync(query);
        var pageSize = Math.Clamp(input.MaxResultCount, 1, MaxPageSize);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Id).Skip(Math.Max(input.SkipCount, 0)).Take(pageSize));

        var productIds = items.Select(x => x.IdProducto).Distinct().ToList();
        var products = productIds.Count == 0
            ? new List<Producto>()
            : await _productoRepository.GetListAsync(x => productIds.Contains(x.Id));
        var productNames = products.ToDictionary(x => x.Id, x => x.Nombre);

        return new PagedResultDto<ItemDespensaDto>(
            totalCount,
            items.Select(x => MapItem(x, productNames.GetValueOrDefault(x.IdProducto))).ToList());
    }

    public async Task<ItemDespensaDto> UpdateAsync(Guid id, UpdateItemDespensaDto input)
    {
        var item = await GetOwnedItemAsync(id);
        item.ActualizarCantidad(
            input.Cantidad.HasValue
                ? new Cantidad(input.Cantidad.Value, input.Unidad!.Trim())
                : null);
        item.ActualizarFechaVencimiento(
            input.FechaVencimiento.HasValue
                ? new FechaVencimiento(input.FechaVencimiento.Value.Date)
                : null);

        await _itemRepository.UpdateAsync(item);
        return await MapItemAsync(item);
    }

    public async Task MarkAsConsumedAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        item.MarcarComoConsumido();
        await _itemRepository.UpdateAsync(item);
    }

    public async Task DeleteAsync(Guid id)
    {
        var item = await GetOwnedItemAsync(id);
        await _advertenciaRepository.DeleteAsync(x => x.ItemDespensaId == item.Id);
        await _itemRepository.DeleteAsync(item);
    }

    private Guid GetCurrentUserId()
    {
        return CurrentUser.Id
            ?? throw new AbpAuthorizationException("Se requiere un usuario autenticado.");
    }

    private async Task<ItemDespensa> GetOwnedItemAsync(Guid id)
    {
        var item = await _itemRepository.FindAsync(id);
        if (item == null)
        {
            throw new EntityNotFoundException(typeof(ItemDespensa), id);
        }

        var userId = GetCurrentUserId();
        var pantry = await _despensaRepository.FirstOrDefaultAsync(
            x => x.Id == item.DespensaId && x.IdUsuario == userId);
        if (pantry == null)
        {
            throw new EntityNotFoundException(typeof(ItemDespensa), id);
        }

        return item;
    }

    private async Task<ItemDespensaDto> MapItemAsync(ItemDespensa item)
    {
        var product = await _productoRepository.FindAsync(item.IdProducto);
        return MapItem(item, product?.Nombre);
    }

    private static ItemDespensaDto MapItem(ItemDespensa item, string? productName)
    {
        return new ItemDespensaDto
        {
            Id = item.Id,
            IdProducto = item.IdProducto,
            NombreProducto = productName,
            Cantidad = item.Cantidad?.Monto,
            Unidad = item.Cantidad?.Unidad,
            FechaVencimiento = item.FechaVencimiento?.FechaVenc,
            EstaConsumido = item.EstaConsumido
        };
    }
}
