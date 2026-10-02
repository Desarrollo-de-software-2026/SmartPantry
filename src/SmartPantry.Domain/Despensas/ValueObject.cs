using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Values;

namespace SmartPantry.Despensa;

public class Cantidad : ValueObject
{
    public decimal Monto { get; private set; }
    public string Unidad { get; private set; }

    private Cantidad() { } // Para EF Core

    public Cantidad(decimal monto, string unidad)
    {
        Monto = monto;
        Unidad = unidad;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Monto;
        yield return Unidad;
    }
}

public class FechaVencimiento : ValueObject
{
    public DateTime FechaVenc { get; private set; }

    private FechaVencimiento() { }

    public FechaVencimiento(DateTime fechaVenc)
    {
        FechaVenc = fechaVenc;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FechaVenc;
    }
}

public class Nota : ValueObject
{
    public string Texto { get; private set; }

    private Nota() { }

    public Nota(string texto)
    {
        Texto = texto;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Texto;
    }
}