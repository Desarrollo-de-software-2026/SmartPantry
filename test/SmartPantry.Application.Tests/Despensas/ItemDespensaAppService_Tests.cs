using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SmartPantry.Despensa;
using SmartPantry.Productos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SmartPantry.Despensas;

public class ItemDespensaAppService_Tests
    : SmartPantryApplicationTestBase<SmartPantryApplicationTestModule>
{
    private static readonly Guid CurrentUserId = Guid.Parse("2e701e62-0953-4dd3-910b-dc6cc93ccb0d");

    [Fact]
    public void App_service_requires_authentication()
    {
        typeof(ItemDespensaAppService)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetListAsync_returns_only_current_users_items_with_product_names_and_paging()
    {
        var data = await SeedAsync(includeSecondOwnedItem: true);
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();

        var firstPage = await WithUnitOfWorkAsync(() => service.GetListAsync(new()
        {
            SkipCount = 0,
            MaxResultCount = 1
        }));
        var secondPage = await WithUnitOfWorkAsync(() => service.GetListAsync(new()
        {
            SkipCount = 1,
            MaxResultCount = 1
        }));

        firstPage.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
        firstPage.Items.Count.ShouldBe(1);
        firstPage.Items[0].NombreProducto.ShouldBe("Producto de prueba");
        secondPage.Items.Count.ShouldBe(1);
        secondPage.Items[0].Id.ShouldNotBe(firstPage.Items[0].Id);
        firstPage.Items.Any(x => x.Id == data.OtherUserItemId).ShouldBeFalse();
        secondPage.Items.Any(x => x.Id == data.OtherUserItemId).ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateAsync_updates_an_owned_items_optional_values()
    {
        var data = await SeedAsync();
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();
        var expiry = new DateTime(2027, 3, 14);

        var updated = await WithUnitOfWorkAsync(() => service.UpdateAsync(data.OwnedItemId, new()
        {
            Cantidad = 2.5m,
            Unidad = "kg",
            FechaVencimiento = expiry
        }));

        updated.Cantidad.ShouldBe(2.5m);
        updated.Unidad.ShouldBe("kg");
        updated.FechaVencimiento!.Value.Date.ShouldBe(expiry);
    }

    [Fact]
    public async Task UpdateAsync_returns_not_found_for_another_users_item()
    {
        var data = await SeedAsync();
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            WithUnitOfWorkAsync(() => service.UpdateAsync(data.OtherUserItemId, new()
            {
                Cantidad = 9,
                Unidad = "kg"
            })));
    }

    [Fact]
    public async Task MarkAsConsumedAsync_returns_not_found_for_another_users_item()
    {
        var data = await SeedAsync();
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            WithUnitOfWorkAsync(() => service.MarkAsConsumedAsync(data.OtherUserItemId)));
    }

    [Fact]
    public async Task DeleteAsync_returns_not_found_for_another_users_item()
    {
        var data = await SeedAsync();
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            WithUnitOfWorkAsync(() => service.DeleteAsync(data.OtherUserItemId)));
    }

    [Fact]
    public async Task MarkAsConsumedAsync_marks_an_owned_item_without_deleting_it()
    {
        var data = await SeedAsync();
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();

        await WithUnitOfWorkAsync(() => service.MarkAsConsumedAsync(data.OwnedItemId));
        var result = await WithUnitOfWorkAsync(() => service.GetListAsync(new()
        {
            SkipCount = 0,
            MaxResultCount = 100
        }));

        result.Items.Single(x => x.Id == data.OwnedItemId).EstaConsumido.ShouldBeTrue();
    }

    [Fact]
    public async Task DeleteAsync_deletes_an_owned_item()
    {
        var data = await SeedAsync();
        var service = ServiceProvider.GetRequiredService<ItemDespensaAppService>();

        await WithUnitOfWorkAsync(() => service.DeleteAsync(data.OwnedItemId));
        var result = await WithUnitOfWorkAsync(() => service.GetListAsync(new()
        {
            SkipCount = 0,
            MaxResultCount = 100
        }));

        result.Items.Any(x => x.Id == data.OwnedItemId).ShouldBeFalse();
    }

    [Fact]
    public void Update_dto_rejects_negative_quantity_invalid_unit_and_unpaired_values()
    {
        Validate(new UpdateItemDespensaDto { Cantidad = -1, Unidad = "kg" }).ShouldNotBeEmpty();
        Validate(new UpdateItemDespensaDto { Cantidad = 1, Unidad = new string('u', 21) }).ShouldNotBeEmpty();
        Validate(new UpdateItemDespensaDto { Cantidad = 1, Unidad = null }).ShouldNotBeEmpty();
        Validate(new UpdateItemDespensaDto { Cantidad = null, Unidad = null }).ShouldBeEmpty();
    }

    private static List<ValidationResult> Validate(object value)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);
        return results;
    }

    private async Task<SeededItems> SeedAsync(bool includeSecondOwnedItem = false)
    {
        var productRepository = ServiceProvider.GetRequiredService<IRepository<Producto, Guid>>();
        var pantryRepository = ServiceProvider.GetRequiredService<IRepository<Despensa, Guid>>();
        var product = new Producto(
            Guid.NewGuid(), "Producto de prueba", "Marca", "Alimentos", "https://example.test/product.png");
        var ownedItem = new ItemDespensa(Guid.NewGuid(), product.Id, new Cantidad(1, "unidad"), null, new Nota("nota de prueba"));
        var otherItem = new ItemDespensa(Guid.NewGuid(), product.Id, new Cantidad(1, "unidad"), null, new Nota("nota de prueba"));
        var ownPantry = new Despensa(Guid.NewGuid(), CurrentUserId);
        var otherPantry = new Despensa(Guid.NewGuid(), Guid.NewGuid());
        ownPantry.AgregarItem(ownedItem);
        otherPantry.AgregarItem(otherItem);
        var ownedItems = new List<ItemDespensa> { ownedItem };
        if (includeSecondOwnedItem)
        {
            var secondOwnedItem = new ItemDespensa(Guid.NewGuid(), product.Id, new Cantidad(1, "unidad"), null, new Nota("nota de prueba"));
            ownPantry.AgregarItem(secondOwnedItem);
            ownedItems.Add(secondOwnedItem);
        }

        await WithUnitOfWorkAsync(async () =>
        {
            await productRepository.InsertAsync(product);
            await pantryRepository.InsertAsync(ownPantry);
            await pantryRepository.InsertAsync(otherPantry);
        });

        return new SeededItems(ownedItem.Id, otherItem.Id);
    }

    private sealed record SeededItems(Guid OwnedItemId, Guid OtherUserItemId);
}
