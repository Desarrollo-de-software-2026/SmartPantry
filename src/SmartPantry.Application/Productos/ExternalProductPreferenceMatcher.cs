using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartPantry.Preferencias;
using Volo.Abp.DependencyInjection;
using Volo.Abp.SettingManagement;

namespace SmartPantry.Productos;

public class ExternalProductPreferenceMatcher : ITransientDependency
{
    private readonly ISettingManager _settingManager;

    public ExternalProductPreferenceMatcher(ISettingManager settingManager) => _settingManager = settingManager;

    public async Task ApplyAsync(Guid userId, ExternalProductDto product)
    {
        product.PreferenceDataAvailable = !string.IsNullOrWhiteSpace(product.IngredientsText) || product.Allergens.Count > 0;
        var json = await _settingManager.GetOrNullAsync(
            SmartPantry.Settings.SmartPantrySettings.PreferenciasAlimentarias, "U", userId.ToString(), true);
        if (string.IsNullOrWhiteSpace(json)) return;

        PreferenciasAlimentariasDto? preferences;
        try { preferences = JsonSerializer.Deserialize<PreferenciasAlimentariasDto>(json); }
        catch (JsonException) { return; }
        if (preferences?.Alergenos == null || !product.PreferenceDataAvailable) return;

        // OFF tags can each have their own language prefix (for example en:milk, es:leche).
        var source = Normalize(product.IngredientsText) + " " + string.Join(" ", product.Allergens.Select(Normalize));
        foreach (var term in preferences.Alergenos.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            var aliases = GetAliases(Normalize(term));
            if (aliases.Any(alias => ContainsTerm(source, alias))) product.MatchedPreferences.Add(term.Trim());
        }
        product.MatchedPreferences = product.MatchedPreferences.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IEnumerable<string> GetAliases(string normalizedTerm)
    {
        var aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalizedTerm };
        var known = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["gluten"] = ["gluten", "wheat", "barley", "rye", "oats"],
            ["crustaceos"] = ["crustacean", "crustaceans", "crustaceos"],
            ["huevos"] = ["egg", "eggs", "huevo", "huevos"],
            ["pescado"] = ["fish", "pescado"],
            ["mani"] = ["peanut", "peanuts", "groundnut", "mani"],
            ["soja"] = ["soy", "soya", "soybean", "soybeans", "soja"],
            ["leche"] = ["milk", "dairy", "leche"],
            ["frutos secos"] = ["nuts", "tree nuts", "almond", "hazelnut", "walnut", "frutos secos"],
            ["apio"] = ["celery", "apio"],
            ["mostaza"] = ["mustard", "mostaza"],
            ["sesamo"] = ["sesame", "sesame seeds", "sesamo"],
            ["sulfitos"] = ["sulphites", "sulfites", "sulphur dioxide", "sulfur dioxide", "sulfitos"],
            ["altramuces"] = ["lupin", "lupine", "altramuces"],
            ["moluscos"] = ["mollusc", "molluscs", "mollusk", "mollusks", "moluscos"]
        };
        if (known.TryGetValue(normalizedTerm, out var values))
            foreach (var value in values) aliases.Add(Normalize(value));
        return aliases.Where(value => value.Length > 0);
    }

    private static bool ContainsTerm(string text, string term) => Regex.IsMatch(
        text,
        $"(?<![\\p{{L}}\\p{{N}}]){Regex.Escape(term)}(?![\\p{{L}}\\p{{N}}])",
        RegexOptions.CultureInvariant);

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var chars = decomposed
            .Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            .Select(ch => char.IsLetterOrDigit(ch) ? char.ToLowerInvariant(ch) : ' ')
            .ToArray();
        var normalized = Regex.Replace(new string(chars), @"\s+", " ").Trim();
        return Regex.Replace(normalized, @"^[a-z]{2,3}\s+", string.Empty);
    }
}
