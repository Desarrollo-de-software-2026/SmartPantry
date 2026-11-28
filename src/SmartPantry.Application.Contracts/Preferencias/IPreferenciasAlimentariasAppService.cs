using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.Preferencias;

public interface IPreferenciasAlimentariasAppService : IApplicationService
{
    Task<PreferenciasAlimentariasDto> GetAsync();
    Task<PreferenciasAlimentariasDto> UpdateAsync(PreferenciasAlimentariasDto input);
}
