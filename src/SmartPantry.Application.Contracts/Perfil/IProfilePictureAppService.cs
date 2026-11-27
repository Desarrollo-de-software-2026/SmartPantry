using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SmartPantry.Perfil;

public interface IProfilePictureAppService : IApplicationService
{
    Task<ProfilePictureDto?> GetAsync();
    Task SaveAsync(ProfilePictureDto input);
    Task DeleteAsync();
}
