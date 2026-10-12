using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Query;

/// <summary>Filtros combináveis com paginação e ordenação. sortBy: id, name, email, phone.</summary>
public sealed class UserQueryDTO : CollectionQueryDTO
{
    protected override string[] AllowedSortFields => ["id", "name", "email", "phone"];
    /// <summary>Nome parcial, sem diferenciar maiúsculas/minúsculas.</summary>
    public string? Name { get; set; }
    /// <summary>Email exato, sem diferenciar maiúsculas/minúsculas.</summary>
    public string? Email { get; set; }
}
