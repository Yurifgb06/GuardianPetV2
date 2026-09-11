using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace GuardianPet.Observability;

public static class GuardianPetTelemetry
{
    public const string ServiceName = "GuardianPet";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Histogram<double> RequestDuration = Meter.CreateHistogram<double>(
        "guardianpet.http.request.duration", "ms", "HTTP request pipeline duration.");
    public static readonly Counter<long> HttpErrors = Meter.CreateCounter<long>(
        "guardianpet.http.errors", "{request}", "HTTP responses with status 400 through 599.");

    public static Activity? StartActivity(string layer, string operation, string entity)
    {
        return ActivitySource.StartActivity($"{entity}.{layer}.{operation}",
            ActivityKind.Internal,
            parentContext: Activity.Current?.Context ?? default,
            tags: new ActivityTagsCollection
            {
                { "guardianpet.layer", layer },
                { "guardianpet.operation", operation },
                { "guardianpet.entity", entity }
            });
    }
}
