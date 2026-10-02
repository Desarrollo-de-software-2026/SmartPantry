using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Xunit;
using Volo.Abp.Domain.Repositories;

namespace SmartPantry.Productos;

public class ProductoAppService_External_Tests
{
    private readonly IExternalProductCatalogClient _externalProductCatalogClientMock;
    private readonly IRepository<Producto, System.Guid> _productoRepositoryMock;
    private readonly ProductoAppService _productoAppService;

    public ProductoAppService_External_Tests()
    {
        // 1. Mocks de las dependencias con NSubstitute
        _externalProductCatalogClientMock = Substitute.For<IExternalProductCatalogClient>();
        _productoRepositoryMock = Substitute.For<IRepository<Producto, System.Guid>>();

        // 2. Instanciamos el AppService de forma pura, 100% aislada de base de datos
        _productoAppService = new ProductoAppService(_productoRepositoryMock, _externalProductCatalogClientMock);
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