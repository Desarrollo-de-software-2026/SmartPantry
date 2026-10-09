using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SmartPantry.Despensa;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Xunit;

namespace SmartPantry.Despensas;

public class AdvertenciaVencimientoManager_Tests : SmartPantryDomainTestBase<SmartPantryDomainTestModule>
{
    [Fact]
    public async Task Debe_Generar_Advertencia_Para_Item_Vencido()
    {
        var despensaId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var despensa = new Despensa(despensaId, Guid.NewGuid());
        var itemVencido = new ItemDespensa(
            itemId,
            Guid.NewGuid(),
            new Cantidad(1, "Litro"),
            new FechaVencimiento(DateTime.Today.AddDays(-2)),
            new Nota("Probando worker"));

        despensa.AgregarItem(itemVencido);

        await InUnitOfWorkAsync(async services =>
        {
            await services.GetRequiredService<IRepository<Despensa, Guid>>()
                .InsertAsync(despensa);
        });

        var persistidos = await InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<ItemDespensa, Guid>>().GetListAsync());
        persistidos.ShouldContain(persistido => persistido.Id == itemId);

        await InUnitOfWorkAsync(async services =>
        {
        });

        await InUnitOfWorkAsync(async services =>
        {
            await services.GetRequiredService<AdvertenciaVencimientoManager>()
                .ProcesarVencimientosAsync();
        });

        var advertencias = await InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>()
                .GetListAsync());

        advertencias.ShouldContain(a =>
            a.ItemDespensaId == itemId &&
            a.TipoAdvertencia == TipoAdvertencia.Vencido.ToString());
    }

    [Fact]
    public async Task Debe_Ser_Idempotente_Y_No_Duplicar_Advertencias()
    {
        var despensaId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var despensa = new Despensa(despensaId, Guid.NewGuid());
        var item = new ItemDespensa(
            itemId,
            Guid.NewGuid(),
            new Cantidad(2, "Unidades"),
            new FechaVencimiento(DateTime.Today.AddDays(3)),
            new Nota("Prueba idempotencia"));

        despensa.AgregarItem(item);

        await InUnitOfWorkAsync(async services =>
        {
            await services.GetRequiredService<IRepository<Despensa, Guid>>()
                .InsertAsync(despensa);
        });

        var persistidos = await InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<ItemDespensa, Guid>>().GetListAsync());
        persistidos.ShouldContain(persistido => persistido.Id == itemId);

        await InUnitOfWorkAsync(async services =>
        {
        });

        for (var i = 0; i < 2; i++)
        {
            await InUnitOfWorkAsync(async services =>
            {
                await services.GetRequiredService<AdvertenciaVencimientoManager>()
                    .ProcesarVencimientosAsync();
            });
        }

        var advertencias = await InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>()
                .GetListAsync());

        var advertenciaDelItem = advertencias
            .Where(a => a.ItemDespensaId == itemId)
            .ToList();

        advertenciaDelItem.Count.ShouldBe(1);
        advertenciaDelItem[0].TipoAdvertencia.ShouldBe(TipoAdvertencia.ProximoAVencer.ToString());
    }

    private async Task InUnitOfWorkAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();

        await action(scope.ServiceProvider);
        await unitOfWork.CompleteAsync();
    }

    private async Task<TResult> InUnitOfWorkAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var unitOfWorkManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = unitOfWorkManager.Begin();

        var result = await action(scope.ServiceProvider);
        await unitOfWork.CompleteAsync();
        return result;
    }
}