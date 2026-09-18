using AutoMapper;
using SmartPantry.Productos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SmartPantry;

public class SmartPantryApplicationAutoMapperProfile : Profile
{
    public SmartPantryApplicationAutoMapperProfile()
    {
        // Mapeo para la operación RF-08
        CreateMap<Producto, ProductoDto>();
    }
}