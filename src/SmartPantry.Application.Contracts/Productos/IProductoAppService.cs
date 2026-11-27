using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SmartPantry.Productos;

public interface IProductoAppService : ICrudAppService<
    ProductoDto,
    Guid,
    GetProductosInput,
    CreateProductoDto,
    UpdateProductoDto>
{
    Task<ExternalProductDto?> GetExternalAsync(GetExternalProductInput input);

    Task<PagedResultDto<ExternalProductDto>> SearchExternalAsync(
        SearchExternalProductsInput input);

    Task<SaveExternalProductResultDto> SaveExternalAsync(
        GetExternalProductInput input);
}
