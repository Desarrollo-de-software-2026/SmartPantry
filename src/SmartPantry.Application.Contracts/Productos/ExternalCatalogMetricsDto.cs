using System;

namespace SmartPantry.Productos;

public class ExternalCatalogMetricsDto
{
    public long Requests { get; set; }
    public long Successes { get; set; }
    public long NotFound { get; set; }
    public long Errors { get; set; }
    public double AverageLatencyMilliseconds { get; set; }
    public string? LastErrorCode { get; set; }
    public DateTime? LastErrorAtUtc { get; set; }
}
