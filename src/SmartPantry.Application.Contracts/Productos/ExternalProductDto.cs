using System;

using System.Collections.Generic;

namespace SmartPantry.Productos;

public class ExternalProductDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string IngredientsText { get; set; } = string.Empty;
    public List<string> Allergens { get; set; } = new();
    public List<string> MatchedPreferences { get; set; } = new();
    public bool PreferenceDataAvailable { get; set; }
    public decimal? EnergyKcal100g { get; set; }
    public decimal? Fat100g { get; set; }
    public decimal? Sugars100g { get; set; }
    public decimal? Salt100g { get; set; }
}
