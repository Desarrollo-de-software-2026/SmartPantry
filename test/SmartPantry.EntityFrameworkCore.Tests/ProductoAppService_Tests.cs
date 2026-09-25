using Shouldly;
using SmartPantry.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Productos;

public class ProductoAppService_Tests : SmartPantryEntityFrameworkCoreTestBase
{
    private readonly IProductoAppService _productoAppService;

    public ProductoAppService_Tests()
    {
        _productoAppService = GetRequiredService<IProductoAppService>();
    }

    [Fact]
    public async Task Should_Create_And_Get_Producto_Successfully()
    {
        // Arrange
        var input = new CreateProductoDto
        {
            Nombre = "Leche Descremada",
            Marca = "  La Serenisima  ",
            Categoria = " Lácteos ",
            UrlImagen = "https://ejemplo.com/imagenes/leche.jpg" // <-- AGREGAR AQUÍ
        };

        // Act
        var createdResult = await _productoAppService.CreateAsync(input);

        // Assert
        createdResult.ShouldNotBeNull();
        createdResult.Id.ShouldNotBe(Guid.Empty);
        createdResult.Nombre.ShouldBe("Leche Descremada");
        createdResult.Marca.ShouldBe("La Serenisima");
        createdResult.Categoria.ShouldBe("Lácteos");
        createdResult.UrlImagen.ShouldBe("https://ejemplo.com/imagenes/leche.jpg");

        // Get and verify
        var fetchedResult = await _productoAppService.GetAsync(createdResult.Id);
        fetchedResult.ShouldNotBeNull();
        fetchedResult.Id.ShouldBe(createdResult.Id);
        fetchedResult.Nombre.ShouldBe(createdResult.Nombre);
    }

    [Fact]
    public async Task Should_Not_Create_Producto_Without_Mandatory_Fields()
    {
        // Arrange
        var input = new CreateProductoDto
        {
            Nombre = "",
            Marca = "La Serenisima",
            Categoria = "Lácteos"
        };

        // Act & Assert
        await Should.ThrowAsync<AbpValidationException>(async () =>
        {
            await _productoAppService.CreateAsync(input);
        });
    }
}