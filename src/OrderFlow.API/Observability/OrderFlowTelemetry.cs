using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OrderFlow.API.Observability;

public static class OrderFlowTelemetry
{
    public const string ServiceName = "OrderFlow.Api";
    public static readonly ActivitySource Activity = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Counter<long> OrdersCreated = Meter.CreateCounter<long>("orderflow.orders.created");
    public static readonly Counter<long> AccountingSyncs = Meter.CreateCounter<long>("orderflow.accounting.syncs");
}
