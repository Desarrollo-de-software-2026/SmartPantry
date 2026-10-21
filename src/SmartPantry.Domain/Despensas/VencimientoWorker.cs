using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace SmartPantry.Despensas;

public class VencimientoWorker : AsyncPeriodicBackgroundWorkerBase
{
    public VencimientoWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        // Intervalo de ejecución en milisegundos (ej. 24 hs = 86400000 ms)
        Timer.Period = 86400000;
    }

    protected override async Task DoWorkAsync(
    PeriodicBackgroundWorkerContext workerContext)
    {
        try
        {
            var services = workerContext.ServiceProvider;

            var uowManager = services.GetRequiredService<IUnitOfWorkManager>();
            using var uow = uowManager.Begin();

            var manager = services.GetRequiredService<AdvertenciaVencimientoManager>();
            await manager.ProcesarVencimientosAsync();

            await uow.CompleteAsync();
        }
        catch (System.Exception exception)
        {
            Logger.LogError(exception,
                "Error al procesar las advertencias de vencimiento. La próxima ejecución periódica continuará normalmente.");
        }
    }
}
