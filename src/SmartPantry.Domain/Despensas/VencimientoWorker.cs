using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

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

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var manager = workerContext.ServiceProvider.GetRequiredService<AdvertenciaVencimientoManager>();
        await manager.ProcesarVencimientosAsync();
    }
}