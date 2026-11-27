using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;

namespace SmartPantry.Perfil;

[Authorize]
public class ProfilePictureAppService : ApplicationService, IProfilePictureAppService
{
    private const int MaxBytes = 1024 * 1024;
    private readonly IBlobContainer<ProfilePictureContainer> _container;

    public ProfilePictureAppService(IBlobContainer<ProfilePictureContainer> container) => _container = container;

    public async Task<ProfilePictureDto?> GetAsync()
    {
        var key = GetUserKey();
        if (!await _container.ExistsAsync(key)) return null;
        var bytes = await _container.GetAllBytesAsync(key);
        var contentType = DetectContentType(bytes);
        return contentType == null ? null : new ProfilePictureDto { ContentType = contentType, Base64 = Convert.ToBase64String(bytes) };
    }

    public async Task SaveAsync(ProfilePictureDto input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Base64) || input.Base64.Length > (MaxBytes * 4 / 3) + 8)
            throw new BusinessException("SmartPantry:InvalidProfilePicture");
        byte[] bytes;
        try { bytes = Convert.FromBase64String(input.Base64); }
        catch (FormatException) { throw new BusinessException("SmartPantry:InvalidProfilePicture"); }
        var detected = DetectContentType(bytes);
        if (bytes.Length == 0 || bytes.Length > MaxBytes || detected == null || detected != input.ContentType)
            throw new BusinessException("SmartPantry:InvalidProfilePicture");
        await _container.SaveAsync(GetUserKey(), bytes, overrideExisting: true);
    }

    public Task DeleteAsync() => _container.DeleteAsync(GetUserKey());

    private string GetUserKey() => CurrentUser.Id?.ToString("N") ?? throw new AbpAuthorizationException("Se requiere un usuario autenticado.");

    private static string? DetectContentType(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return "image/png";
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return "image/jpeg";
        if (bytes.Length >= 12 && bytes[0] == 'R' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == 'F' && bytes[8] == 'W' && bytes[9] == 'E' && bytes[10] == 'B' && bytes[11] == 'P') return "image/webp";
        return null;
    }
}
