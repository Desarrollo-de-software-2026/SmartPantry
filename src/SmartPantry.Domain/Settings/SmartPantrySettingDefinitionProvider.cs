using Volo.Abp.Settings;

namespace SmartPantry.Settings;

public class SmartPantrySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        var preferences = new SettingDefinition(
            SmartPantrySettings.PreferenciasAlimentarias,
            "",
            isVisibleToClients: false);
        preferences.Providers.Add("U");
        context.Add(preferences);
    }
}
