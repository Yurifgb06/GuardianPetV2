namespace GuardianPet.DTOs.Response;

/// <summary>Link gerado a partir de uma rota GET existente.</summary>
public sealed record ApiLink(string Rel, string Href, string Method = "GET");

/// <summary>Representação opcional de recurso: dados originais e links.</summary>
public sealed record ResourceResponse<T>(T Data, IReadOnlyList<ApiLink> Links);

/// <summary>Representação opcional de coleção: recursos e links de navegação.</summary>
public sealed record CollectionResponse<T>(IReadOnlyList<ResourceResponse<T>> Items, IReadOnlyList<ApiLink> Links);
