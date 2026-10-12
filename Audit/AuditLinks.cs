using System.Globalization;
using GuardianPet.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace GuardianPet.Audit;

// No item route is advertised: only the administrative collection GET exists.
public static class AuditLinks
{
    public static CollectionResponse<AuditEvent> Create(ControllerBase controller, List<AuditEvent> events,
        AuditQuery query, long total)
    {
        var last = Math.Max(1L, (total + query.PageSize - 1) / query.PageSize);
        var links = new List<ApiLink> { new("self", Url(controller)), new("first", Url(controller, 1)) };
        if (query.Page > 1) links.Add(new("prev", Url(controller, query.Page - 1)));
        if (query.Page < last) links.Add(new("next", Url(controller, query.Page + 1)));
        links.Add(new("last", Url(controller, last)));
        return new(events.Select(item => new ResourceResponse<AuditEvent>(item, [])).ToList(), links);
    }

    private static string Url(ControllerBase controller, long? page = null)
    {
        var url = controller.Url.ActionLink("FindAll", "Audit")
            ?? throw new InvalidOperationException("Rota de auditoria não encontrada.");
        var query = new Dictionary<string, StringValues>(controller.Request.Query, StringComparer.OrdinalIgnoreCase)
        { ["includeLinks"] = "true" };
        if (page.HasValue) query["page"] = page.Value.ToString(CultureInfo.InvariantCulture);
        return url + QueryString.Create(query.SelectMany(pair =>
            pair.Value.Select(value => new KeyValuePair<string, string?>(pair.Key, value))));
    }
}
