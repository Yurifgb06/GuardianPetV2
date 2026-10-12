using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Query;

/// <summary>Filtros combináveis com paginação e ordenação. sortBy: id, name, species, breed, age, weight, petSize.</summary>
public sealed class PetQueryDTO : CollectionQueryDTO
{
    protected override string[] AllowedSortFields => ["id", "name", "species", "breed", "age", "weight", "petSize"];
    /// <summary>Espécie parcial, sem diferenciar maiúsculas/minúsculas.</summary>
    public string? Species { get; set; }
    /// <summary>Porte: SMALL, MEDIUM, BIG ou GIANT.</summary>
    [EnumDataType(typeof(PetSizeEnum))]
    public PetSizeEnum? PetSize { get; set; }
}
