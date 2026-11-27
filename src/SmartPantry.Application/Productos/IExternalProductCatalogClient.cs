using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace SmartPantry.Productos;

public interface IExternalProductCatalogClient
{
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);

    Task<PagedResultDto<ExternalProductDto>> SearchAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
