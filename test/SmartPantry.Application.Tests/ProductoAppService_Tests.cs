using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Productos;

// Agrega <SmartPantryApplicationTestModule> aquí:
public class ProductoAppService_Tests : SmartPantryApplicationTestBase<SmartPantryApplicationTestModule>
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
            Categoria = " Lácteos "
        };

        // Act
        var createdResult =
            await _productoAppService.CreateAsync(input);

        // Assert
        createdResult.ShouldNotBeNull();
        createdResult.Id.ShouldNotBe(Guid.Empty);
        createdResult.Nombre.ShouldBe("Leche Descremada");
        createdResult.Marca.ShouldBe("La Serenisima");
        createdResult.Categoria.ShouldBe("Lácteos");

        // Get and verify
        var fetchedResult =
            await _productoAppService.GetAsync(createdResult.Id);

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