using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.Administracion;

public interface IAdministracionAppService : IApplicationService
{
    Task<MetricasDespensaDto> GetMetricasAsync();
}
