using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Despensas;

public class ItemDespensaDto : EntityDto<Guid>
{
    public Guid IdProducto { get; set; }
    public string? NombreProducto { get; set; }
    public decimal? Cantidad { get; set; }
    public string? Unidad { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool EstaConsumido { get; set; }
}

public class UpdateItemDespensaDto : IValidatableObject
{
    [Range(typeof(decimal), "0", "999999999999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal? Cantidad { get; set; }

    [StringLength(20)]
    public string? Unidad { get; set; }

    [Range(typeof(DateTime), "1900-01-01", "9999-12-31")]
    public DateTime? FechaVencimiento { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Cantidad.HasValue != !string.IsNullOrWhiteSpace(Unidad))
        {
            yield return new ValidationResult(
                "La cantidad y la unidad deben informarse juntas o dejarse ambas vacías.",
                new[] { nameof(Cantidad), nameof(Unidad) });
        }
    }
}
