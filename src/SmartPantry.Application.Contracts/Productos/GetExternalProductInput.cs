using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Productos;

public class GetExternalProductInput
{
    [Required]
    [StringLength(14, MinimumLength = 8)]
    public string Barcode { get; set; } = string.Empty;
}