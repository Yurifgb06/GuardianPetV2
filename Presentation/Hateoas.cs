using System.Globalization;
using GuardianPet.DTOs.Query;
using GuardianPet.DTOs.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace GuardianPet.Presentation;

public static class Hateoas
{
    public static ResourceResponse<T> Resource<T>(ControllerBase controller, T data)
        => new(data, [new ApiLink("self", Current(controller)),
            new ApiLink("collection", Action(controller, "FindAll", new { includeLinks = true }))]);

    public static async Task<CollectionResponse<T>> CollectionAsync<T>(ControllerBase controller,
        IReadOnlyList<T> items, CollectionQueryDTO options, Func<T, long> id, Func<Task<long>> count)
    {
        var links = new List<ApiLink> { new("self", Current(controller)) };
        if (options.Page.HasValue || options.PageSize.HasValue)
        {
            // The count uses the same filters, without sorting or pagination.
            var total = await count();
            var page = options.Page ?? 1;
            var size = options.PageSize ?? 20;
            var last = Math.Max(1L, (total + size - 1) / size);
            links.Add(new("first", Current(controller, 1, size)));
            if (page > 1) links.Add(new("prev", Current(controller, page - 1L, size)));
            if (page < last) links.Add(new("next", Current(controller, page + 1L, size)));
            links.Add(new("last", Current(controller, last, size)));
        }
        var resources = items.Select(item => new ResourceResponse<T>(item,
            [new ApiLink("self", Action(controller, "FindById", new { id = id(item), includeLinks = true }))])).ToList();
        return new(resources, links);
    }

    private static string Current(ControllerBase controller, long? page = null, int? size = null)
    {
        var values = new RouteValueDictionary(controller.RouteData.Values);
        var url = Action(controller, controller.ControllerContext.ActionDescriptor.ActionName, values);
        var query = new Dictionary<string, StringValues>(controller.Request.Query, StringComparer.OrdinalIgnoreCase)
        {
            ["includeLinks"] = "true"
        };
        if (page.HasValue) query["page"] = page.Value.ToString(CultureInfo.InvariantCulture);
        if (size.HasValue) query["pageSize"] = size.Value.ToString(CultureInfo.InvariantCulture);
        return url + QueryString.Create(query.SelectMany(pair =>
            pair.Value.Select(value => new KeyValuePair<string, string?>(pair.Key, value))));
    }

    private static string Action(ControllerBase controller, string action, object values)
        => controller.Url.ActionLink(action, controller.ControllerContext.ActionDescriptor.ControllerName, values)
            ?? throw new InvalidOperationException("A rota GET não pôde ser resolvida.");
}
