using System;

namespace SmartPantry.Productos;

public interface IExternalCatalogMetrics
{
    void RecordSuccess(TimeSpan elapsed);
    void RecordNotFound(TimeSpan elapsed);
    void RecordError(TimeSpan elapsed, string errorCode);
    ExternalCatalogMetricsDto Snapshot();
}
