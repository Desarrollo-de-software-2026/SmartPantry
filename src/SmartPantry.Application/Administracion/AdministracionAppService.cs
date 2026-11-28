using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SmartPantry.Despensa;
using SmartPantry.Permissions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace SmartPantry.Administracion;

[Authorize(SmartPantryPermissions.Administration.Default)]
public class AdministracionAppService : ApplicationService, IAdministracionAppService
{
    private readonly IRepository<IdentityUser, System.Guid> _userRepository;
    private readonly IRepository<ItemDespensa, System.Guid> _itemRepository;
    private readonly IRepository<AdvertenciaVencimiento, System.Guid> _warningRepository;
    private readonly SmartPantry.Productos.IExternalCatalogMetrics _externalMetrics;

    public AdministracionAppService(
        IRepository<IdentityUser, System.Guid> userRepository,
        IRepository<ItemDespensa, System.Guid> itemRepository,
        IRepository<AdvertenciaVencimiento, System.Guid> warningRepository,
        SmartPantry.Productos.IExternalCatalogMetrics externalMetrics)
    {
        _userRepository = userRepository;
        _itemRepository = itemRepository;
        _warningRepository = warningRepository;
        _externalMetrics = externalMetrics;
    }

    public async Task<MetricasDespensaDto> GetMetricasAsync()
    {
        var users = await _userRepository.GetQueryableAsync();
        var items = await _itemRepository.GetQueryableAsync();
        var warnings = await _warningRepository.GetQueryableAsync();
        var totalUsers = await AsyncExecuter.CountAsync(users);
        var activeUsers = await AsyncExecuter.CountAsync(users.Where(user => user.IsActive));

        return new MetricasDespensaDto
        {
            UsuariosTotales = totalUsers,
            UsuariosActivos = activeUsers,
            UsuariosDeshabilitados = totalUsers - activeUsers,
            ItemsDespensa = await AsyncExecuter.CountAsync(items),
            AlertasActivas = await AsyncExecuter.CountAsync(warnings.Where(warning => warning.EstaActiva)),
            IntegracionExterna = _externalMetrics.Snapshot()
        };
    }
}
