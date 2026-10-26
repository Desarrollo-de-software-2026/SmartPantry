using System;
using System.Collections.Generic;
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
    private static readonly DateTime Hoy = new(2026, 10, 10);

    [Fact]
    public async Task Debe_Generar_Advertencia_Para_Item_Vencido()
    {
        var itemId = await GuardarItemAsync(Hoy.AddDays(-1));
        await ProcesarAsync();

        var advertencia = await ObtenerAdvertenciaAsync(itemId);
        advertencia.ShouldNotBeNull();
        advertencia!.TipoAdvertencia.ShouldBe(TipoAdvertencia.Vencido.ToString());
        advertencia.EstaActiva.ShouldBeTrue();
    }

    [Fact]
    public async Task Debe_Incluir_Dia_Limite_Y_Excluir_Fecha_Fuera_Del_Umbral()
    {
        var itemLimite = await GuardarItemAsync(Hoy.AddDays(7));
        var itemFuera = await GuardarItemAsync(Hoy.AddDays(8));

        await ProcesarAsync();

        var advertenciaLimite = await ObtenerAdvertenciaAsync(itemLimite);
        advertenciaLimite.ShouldNotBeNull();
        advertenciaLimite!.TipoAdvertencia.ShouldBe(TipoAdvertencia.ProximoAVencer.ToString());
        (await ObtenerAdvertenciaAsync(itemFuera)).ShouldBeNull();
    }

    [Fact]
    public async Task No_Debe_Generar_Advertencia_Si_No_Tiene_Fecha_O_Esta_Consumido()
    {
        var itemSinFecha = await GuardarItemAsync(null);
        var itemConsumido = await GuardarItemAsync(Hoy.AddDays(2), consumido: true);

        await ProcesarAsync();

        (await ObtenerAdvertenciaAsync(itemSinFecha)).ShouldBeNull();
        (await ObtenerAdvertenciaAsync(itemConsumido)).ShouldBeNull();
    }

    [Fact]
    public async Task Debe_Desactivar_Actualizar_Y_Reactivar_La_Misma_Advertencia()
    {
        var itemId = await GuardarItemAsync(Hoy.AddDays(3));
        await ProcesarAsync();
        var original = await ObtenerAdvertenciaAsync(itemId);
        original.ShouldNotBeNull();
        var advertenciaId = original!.Id;

        await CambiarFechaAsync(itemId, null);
        await ProcesarAsync();
        var sinFecha = await ObtenerAdvertenciaAsync(itemId);
        sinFecha.ShouldNotBeNull();
        sinFecha!.Id.ShouldBe(advertenciaId);
        sinFecha.EstaActiva.ShouldBeFalse();

        await CambiarFechaAsync(itemId, Hoy.AddDays(3));
        await ProcesarAsync();
        var reactivada = await ObtenerAdvertenciaAsync(itemId);
        reactivada.ShouldNotBeNull();
        reactivada!.Id.ShouldBe(advertenciaId);
        reactivada.EstaActiva.ShouldBeTrue();

        await CambiarFechaAsync(itemId, Hoy.AddDays(8));
        await ProcesarAsync();
        var fueraDelUmbral = await ObtenerAdvertenciaAsync(itemId);
        fueraDelUmbral.ShouldNotBeNull();
        fueraDelUmbral!.Id.ShouldBe(advertenciaId);
        fueraDelUmbral.EstaActiva.ShouldBeFalse();

        await CambiarFechaAsync(itemId, Hoy.AddDays(-1));
        await ProcesarAsync();
        var vencida = await ObtenerAdvertenciaAsync(itemId);
        vencida.ShouldNotBeNull();
        vencida!.Id.ShouldBe(advertenciaId);
        vencida.EstaActiva.ShouldBeTrue();
        vencida.TipoAdvertencia.ShouldBe(TipoAdvertencia.Vencido.ToString());

        await MarcarComoConsumidoAsync(itemId);
        await ProcesarAsync();
        var consumida = await ObtenerAdvertenciaAsync(itemId);
        consumida.ShouldNotBeNull();
        consumida!.Id.ShouldBe(advertenciaId);
        consumida.EstaActiva.ShouldBeFalse();
    }

    [Fact]
    public async Task Debe_Ser_Idempotente_Y_No_Duplicar_Advertencias()
    {
        var itemId = await GuardarItemAsync(Hoy.AddDays(3));

        await ProcesarAsync();
        await ProcesarAsync();

        var advertencias = await InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>()
                .GetListAsync(a => a.ItemDespensaId == itemId));
        advertencias.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Ejecuciones_Cercanas_No_Deben_Duplicar_Advertencias()
    {
        var itemId = await GuardarItemAsync(Hoy.AddDays(3));

        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => ProcesarAsync()));

        var advertencias = await InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>()
                .GetListAsync(a => a.ItemDespensaId == itemId));
        advertencias.Count.ShouldBe(1);
    }
    [Fact]
    public async Task Debe_Actualizar_Fecha_De_Advertencia_Aunque_Mantenga_La_Misma_Categoria()
    {
        var fechaInicial = Hoy.AddDays(3);
        var nuevaFecha = Hoy.AddDays(5);
        var itemId = await GuardarItemAsync(fechaInicial);

        await ProcesarAsync();
        var advertenciaInicial = await ObtenerAdvertenciaAsync(itemId);
        advertenciaInicial.ShouldNotBeNull();
        advertenciaInicial!.TipoAdvertencia.ShouldBe(TipoAdvertencia.ProximoAVencer.ToString());
        advertenciaInicial.FechaVencimientoRegistrada.ShouldBe(fechaInicial.Date);

        await CambiarFechaAsync(itemId, nuevaFecha);
        await ProcesarAsync();

        var actualizada = await ObtenerAdvertenciaAsync(itemId);
        actualizada.ShouldNotBeNull();
        actualizada!.TipoAdvertencia.ShouldBe(TipoAdvertencia.ProximoAVencer.ToString());
        actualizada.FechaVencimientoRegistrada.ShouldBe(nuevaFecha.Date);
        actualizada.Id.ShouldBe(advertenciaInicial.Id);
    }

    private async Task<Guid> GuardarItemAsync(DateTime? fecha, bool consumido = false)
    {
        var itemId = Guid.NewGuid();
        var item = new ItemDespensa(
            itemId,
            Guid.NewGuid(),
            new Cantidad(1, "Unidades"),
            fecha.HasValue ? new FechaVencimiento(fecha.Value) : null,
            new Nota("Prueba TP08"));

        if (consumido)
            item.MarcarComoConsumido();

        var despensa = new Despensa(Guid.NewGuid(), Guid.NewGuid());
        despensa.AgregarItem(item);

        await InUnitOfWorkAsync(async services =>
        {
            await services.GetRequiredService<IRepository<Despensa, Guid>>()
                .InsertAsync(despensa);
        });

        return itemId;
    }

    private async Task CambiarFechaAsync(Guid itemId, DateTime? fecha)
    {
        await InUnitOfWorkAsync(async services =>
        {
            var repository = services.GetRequiredService<IRepository<ItemDespensa, Guid>>();
            var item = await repository.GetAsync(itemId);
            item.ActualizarFechaVencimiento(
                fecha.HasValue ? new FechaVencimiento(fecha.Value) : null);
            await repository.UpdateAsync(item);
        });
    }

    private async Task MarcarComoConsumidoAsync(Guid itemId)
    {
        await InUnitOfWorkAsync(async services =>
        {
            var repository = services.GetRequiredService<IRepository<ItemDespensa, Guid>>();
            var item = await repository.GetAsync(itemId);
            item.MarcarComoConsumido();
            await repository.UpdateAsync(item);
        });
    }
    private Task ProcesarAsync() => InUnitOfWorkAsync(async services =>
    {
        await services.GetRequiredService<AdvertenciaVencimientoManager>()
            .ProcesarVencimientosAsync(Hoy);
    });

    private Task<AdvertenciaVencimiento?> ObtenerAdvertenciaAsync(Guid itemId) =>
        InUnitOfWorkAsync(services =>
            services.GetRequiredService<IRepository<AdvertenciaVencimiento, Guid>>()
                .FirstOrDefaultAsync(a => a.ItemDespensaId == itemId));

    private async Task InUnitOfWorkAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = manager.Begin();
        await action(scope.ServiceProvider);
        await unitOfWork.CompleteAsync();
    }

    private async Task<TResult> InUnitOfWorkAsync<TResult>(Func<IServiceProvider, Task<TResult>> action)
    {
        using var scope = ServiceProvider.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        using var unitOfWork = manager.Begin();
        var result = await action(scope.ServiceProvider);
        await unitOfWork.CompleteAsync();
        return result;
    }
}
