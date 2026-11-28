using System;
using System.Threading;

namespace SmartPantry.Productos;

public class ExternalCatalogMetrics : IExternalCatalogMetrics
{
    private long _requests;
    private long _successes;
    private long _notFound;
    private long _errors;
    private long _totalElapsedTicks;
    private long _lastErrorTicks;
    private string? _lastErrorCode;

    public void RecordSuccess(TimeSpan elapsed)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Increment(ref _successes);
        Interlocked.Add(ref _totalElapsedTicks, Math.Max(0, elapsed.Ticks));
    }

    public void RecordNotFound(TimeSpan elapsed)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Increment(ref _notFound);
        Interlocked.Add(ref _totalElapsedTicks, Math.Max(0, elapsed.Ticks));
    }

    public void RecordError(TimeSpan elapsed, string errorCode)
    {
        Interlocked.Increment(ref _requests);
        Interlocked.Increment(ref _errors);
        Interlocked.Add(ref _totalElapsedTicks, Math.Max(0, elapsed.Ticks));
        _lastErrorCode = errorCode;
        Interlocked.Exchange(ref _lastErrorTicks, DateTime.UtcNow.Ticks);
    }

    public ExternalCatalogMetricsDto Snapshot()
    {
        var requests = Interlocked.Read(ref _requests);
        var lastErrorTicks = Interlocked.Read(ref _lastErrorTicks);
        return new ExternalCatalogMetricsDto
        {
            Requests = requests,
            Successes = Interlocked.Read(ref _successes),
            NotFound = Interlocked.Read(ref _notFound),
            Errors = Interlocked.Read(ref _errors),
            AverageLatencyMilliseconds = requests == 0
                ? 0
                : TimeSpan.FromTicks(Interlocked.Read(ref _totalElapsedTicks) / requests).TotalMilliseconds,
            LastErrorCode = _lastErrorCode,
            LastErrorAtUtc = lastErrorTicks == 0 ? null : new DateTime(lastErrorTicks, DateTimeKind.Utc)
        };
    }
}
