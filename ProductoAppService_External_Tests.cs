using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Xunit;

namespace SmartPantry.Productos;

public class ProductoAppService_External_Tests : SmartPantryApplicationTestBase
{
    private readonly IExternalProductCatalogClient _externalProductCatalogClientMock;
    private readonly ProductoAppService _productoAppService;

    public ProductoAppService_External_Tests()
    {
        // Creamos el mock de la interfaz externa
        _externalProductCatalogClientMock = Substitute.For<IExternalProductCatalogClient>();

        // Instanciamos el AppService inyectándole el mock y el repositorio por defecto de la base de pruebas
        var repository = GetRequiredService<Volo.Abp.Domain.Repositories.IRepository<Producto, System.Guid>>();
        _productoAppService = new ProductoAppService(repository, _externalProductCatalogClientMock);
    }

    [Fact]
    public async Task Should_Get_External_Product_When_Exists()
    {
        // Arrange
        var barcode = "3017620422003";
        var fakeExternalDto = new ExternalProductDto
        {
            Code = barcode,
            Name = "Nutella",
            Brand = "Ferrero"
        };

        _externalProductCatalogClientMock
            .GetByBarcodeAsync(barcode)
            .Returns(fakeExternalDto);

        // Act
        var result = await _productoAppService.GetExternalAsync(new GetExternalProductInput { Barcode = barcode });

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Nutella");
        result.Brand.ShouldBe("Ferrero");
    }

    [Fact]
    public async Task Should_Return_Null_When_Product_Not_Found()
    {
        // Arrange
        var barcode = "0000000000000";

        _externalProductCatalogClientMock
            .GetByBarcodeAsync(barcode)
            .Returns((ExternalProductDto?)null);

        // Act
        var result = await _productoAppService.GetExternalAsync(new GetExternalProductInput { Barcode = barcode });

        // Assert
        result.ShouldBeNull();
    }
}