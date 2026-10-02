using System.Threading.Tasks;

namespace SmartPantry.Productos;

public interface IExternalProductCatalogClient
{
    Task<ExternalProductDto?> GetByBarcodeAsync(string barcode);
}