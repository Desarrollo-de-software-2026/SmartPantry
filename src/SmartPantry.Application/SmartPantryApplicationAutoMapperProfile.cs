using AutoMapper;
using SmartPantry.Productos;
using SmartPantry.Books;


namespace SmartPantry;

public class SmartPantryApplicationAutoMapperProfile : Profile
{
    public SmartPantryApplicationAutoMapperProfile()
    {
        // Mapeo para la operación RF-08
        CreateMap<Producto, ProductoDto>();
        CreateMap<CreateProductoDto, Producto>();
        CreateMap<UpdateProductoDto, Producto>();

        CreateMap<Book, BookDto>()
            .ForMember(dto => dto.AuthorName, options => options.Ignore());
        CreateMap<CreateUpdateBookDto, Book>();
    }
}
