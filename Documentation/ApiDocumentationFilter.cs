using GuardianPet.Exceptions;
using GuardianPet.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GuardianPet.Documentation;

// Documentation only: authorization remains controlled by endpoint metadata.
public sealed class ApiDocumentationFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (var description in context.ApiDescriptions)
        {
            var path = "/" + description.RelativePath?.Split('?')[0];
            if (!document.Paths.TryGetValue(path, out var item)) continue;
            var operation = item.Operations?.FirstOrDefault(pair =>
                string.Equals(pair.Key.Method, description.HttpMethod, StringComparison.OrdinalIgnoreCase)).Value;
            if (operation is null) continue;

            if (description.HttpMethod == "GET" && description.ParameterDescriptions.Any(p => p.Name == "includeLinks"))
            {
                var legacyType = description.SupportedResponseTypes.First(r => r.StatusCode == 200).Type!;
                var isCollection = legacyType.IsGenericType && legacyType.GetGenericTypeDefinition() == typeof(List<>);
                var itemType = isCollection ? legacyType.GetGenericArguments()[0] : legacyType;
                var linkedType = (isCollection ? typeof(CollectionResponse<>) : typeof(ResourceResponse<>)).MakeGenericType(itemType);
                var example = isCollection
                    ? """{"items":[{"data":{"id":1},"links":[{"rel":"self","href":"URL da rota por ID","method":"GET"}]}],"links":[{"rel":"self","href":"URL da coleção","method":"GET"}]}"""
                    : """{"data":{"id":1},"links":[{"rel":"self","href":"URL do recurso","method":"GET"},{"rel":"collection","href":"URL da coleção","method":"GET"}]}""";
                operation.Description += "\nincludeLinks=false (padrão): JSON original. Exemplo estrutural de includeLinks=true (data mantém todos os campos do DTO): " + example
                    + "\nNa paginação, first/last estão disponíveis; prev/next apenas quando aplicáveis. Filtros e ordenação são preservados. Nenhum link de escrita é exposto.";
                operation.Responses!["200"].Content!["application/json"].Schema = new OpenApiSchema
                {
                    OneOf = [context.SchemaGenerator.GenerateSchema(legacyType, context.SchemaRepository),
                        context.SchemaGenerator.GenerateSchema(linkedType, context.SchemaRepository)]
                };
            }

            var metadata = description.ActionDescriptor.EndpointMetadata;
            var protectedEndpoint = metadata.OfType<IAuthorizeData>().Any()
                && !metadata.OfType<IAllowAnonymous>().Any();
            operation.Security = protectedEndpoint
                ? [new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                }]
                : [];

            // Business validation and automatic model validation use different bodies.
            var businessValidation = (description.HttpMethod == "POST"
                && path is "/api/users" or "/api/veterinarians" or "/api/consultations")
                || (description.HttpMethod == "PUT" && path == "/api/consultations/{id}");
            if (businessValidation && operation.Responses?.ContainsKey("400") == true)
            {
                operation.Responses!["400"] = new OpenApiResponse
                {
                    Description = "Dados inválidos: validação do corpo ou regra de negócio.",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new()
                        {
                            Schema = new OpenApiSchema
                            {
                                OneOf = [context.SchemaGenerator.GenerateSchema(typeof(ErrorResponse), context.SchemaRepository),
                                    context.SchemaGenerator.GenerateSchema(typeof(ValidationProblemDetails), context.SchemaRepository)]
                            }
                        }
                    }
                };
            }
        }

        AddHealthPath(document, "/health", "Verifica se a API está ativa, sem consultar o Oracle.", false);
        AddHealthPath(document, "/health/ready", "Verifica a conectividade real com o Oracle usando AppDbContext.Database.CanConnectAsync().", true);
    }

    private static void AddHealthPath(OpenApiDocument document, string path, string summary, bool readiness)
    {
        var operation = new OpenApiOperation
        {
            Summary = summary,
            Description = "Acesso anônimo; não exige token JWT.",
            Security = [],
            Responses = new OpenApiResponses { ["200"] = HealthResponse("Healthy") }
        };
        if (readiness) operation.Responses.Add("503", HealthResponse("Unhealthy: Oracle indisponível."));
        var item = new OpenApiPathItem();
        item.AddOperation(HttpMethod.Get, operation);
        document.Paths[path] = item;
    }

    private static OpenApiResponse HealthResponse(string description) => new()
    {
        Description = description,
        Content = new Dictionary<string, OpenApiMediaType>
        {
            ["text/plain"] = new() { Schema = new OpenApiSchema { Type = JsonSchemaType.String } }
        }
    };
}
