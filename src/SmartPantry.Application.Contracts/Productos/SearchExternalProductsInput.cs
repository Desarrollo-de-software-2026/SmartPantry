using System.ComponentModel.DataAnnotations;

namespace SmartPantry.Productos;

public class SearchExternalProductsInput
{
    [Required]
    [StringLength(80, MinimumLength = 2)]
    public string Query { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 50)]
    public int PageSize { get; set; } = 10;
}
