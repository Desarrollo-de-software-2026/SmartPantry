using SmartPantry.Despensa;
using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace SmartPantry.Despensas;

public class Despensa : AggregateRoot<Guid>
{
    public Guid IdUsuario { get; private set; }

    // Colección de ítems
    private readonly List<ItemDespensa> _items;
    public IReadOnlyCollection<ItemDespensa> Items => _items.AsReadOnly();

    private Despensa()
    {
        _items = new List<ItemDespensa>();
    }

    public Despensa(Guid id, Guid idUsuario) : base(id)
    {
        IdUsuario = idUsuario;
        _items = new List<ItemDespensa>();
    }

    public void AgregarItem(ItemDespensa item)
    {
        _items.Add(item);
    }
}