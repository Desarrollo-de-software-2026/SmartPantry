using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Preferencias;

public class PreferenciasAlimentariasDto
{
    public List<string> Alergenos { get; set; } = new();
    public bool AlertasVencimiento { get; set; } = true;

    [Range(1, 30)]
    public int DiasAnticipacion { get; set; } = 7;
}
