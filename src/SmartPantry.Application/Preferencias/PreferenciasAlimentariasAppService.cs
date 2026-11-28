using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.SettingManagement;

namespace SmartPantry.Preferencias;

[Authorize]
public class PreferenciasAlimentariasAppService : ApplicationService, IPreferenciasAlimentariasAppService
{
    private const string SettingName = SmartPantry.Settings.SmartPantrySettings.PreferenciasAlimentarias;
    private const string UserSettingProvider = "U";
    private readonly ISettingManager _settingManager;

    public PreferenciasAlimentariasAppService(ISettingManager settingManager)
    {
        _settingManager = settingManager;
    }

    public async Task<PreferenciasAlimentariasDto> GetAsync()
    {
        var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("Se requiere un usuario autenticado.");
        var json = await _settingManager.GetOrNullAsync(SettingName, UserSettingProvider, userId.ToString(), true);
        if (string.IsNullOrWhiteSpace(json)) return new PreferenciasAlimentariasDto();

        try
        {
            return JsonSerializer.Deserialize<PreferenciasAlimentariasDto>(json) ?? new PreferenciasAlimentariasDto();
        }
        catch (JsonException)
        {
            return new PreferenciasAlimentariasDto();
        }
    }

    public async Task<PreferenciasAlimentariasDto> UpdateAsync(PreferenciasAlimentariasDto input)
    {
        var userId = CurrentUser.Id ?? throw new AbpAuthorizationException("Se requiere un usuario autenticado.");
        if (input.DiasAnticipacion is < 1 or > 30)
        {
            throw new BusinessException("SmartPantry:InvalidExpiryAlertDays");
        }

        var normalized = new PreferenciasAlimentariasDto
        {
            AlertasVencimiento = input.AlertasVencimiento,
            DiasAnticipacion = input.DiasAnticipacion,
            Alergenos = (input.Alergenos ?? new())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Where(value => value.Length <= 50)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(30)
                .ToList()
        };

        await _settingManager.SetAsync(
            SettingName,
            JsonSerializer.Serialize(normalized),
            UserSettingProvider,
            userId.ToString(),
            true);
        return normalized;
    }
}
