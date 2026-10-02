using Shouldly;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Xunit;
using SmartPantry.EntityFrameworkCore;

namespace SmartPantry.Productos;

public class ProductoAppService_Tests: SmartPantryApplicationTestBase<SmartPantryEntityFrameworkCoreTestModule>
{
    private readonly IProductoAppService _productoAppService;

    public ProductoAppService_Tests()
    {
        _productoAppService = GetRequiredService<IProductoAppService>();
    }

    [Fact]
    public async Task Should_Perform_Complete_Crud_Flow()
    {
        // CREATE
        var createInput = new CreateProductoDto
        {
            Nombre = "Arroz Integral",
            Marca = "Marolio",
            Categoria = "Almacenes",
            UrlImagen = "https://example.com/arroz.png"
        };

        var created = await _productoAppService.CreateAsync(createInput);

        created.Id.ShouldNotBe(Guid.Empty);
        created.Nombre.ShouldBe("Arroz Integral");

        // GET
        var fetched = await _productoAppService.GetAsync(created.Id);

        fetched.ShouldNotBeNull();
        fetched.Id.ShouldBe(created.Id);

        // LIST
        var pagedList = await _productoAppService.GetListAsync(
            new GetProductosInput
            {
                MaxResultCount = 10
            });

        pagedList.TotalCount.ShouldBeGreaterThan(0);
        pagedList.Items.ShouldContain(p => p.Id == created.Id);

        // UPDATE
        var updateInput = new UpdateProductoDto
        {
            Nombre = "Arroz Doble Carolina",
            Marca = "Marolio",
            Categoria = "Almacenes",
            UrlImagen = "https://example.com/arroz2.png"
        };

        await _productoAppService.UpdateAsync(created.Id, updateInput);

        var updated = await _productoAppService.GetAsync(created.Id);

        updated.Nombre.ShouldBe("Arroz Doble Carolina");

        // DELETE
        await _productoAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(
            async () =>
            {
                await _productoAppService.GetAsync(created.Id);
            });
    }

    [Fact]
    public void Should_Normalize_Data_When_Updating()
    {
        var producto = new Producto(
            Guid.NewGuid(),
            "Leche",
            "Marca",
            "Lacteos",
            null
        );

        producto.Actualizar(
            "  Leche Entera  ",
            "  Marca Nueva  ",
            "  Lacteos  ",
            null
        );

        producto.Nombre.ShouldBe("Leche Entera");
        producto.Marca.ShouldBe("Marca Nueva");
        producto.Categoria.ShouldBe("Lacteos");
    }

    [Fact]
    public void Should_Reject_Invalid_Update()
    {
        var producto = new Producto(
            Guid.NewGuid(),
            "Leche",
            "Marca",
            "Lacteos",
            null
        );

        Should.Throw<ArgumentException>(() =>
        {
            producto.Actualizar(
                "",
                "Marca",
                "Lacteos",
                null
            );
        });
    }
}
