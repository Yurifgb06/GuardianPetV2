using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Query;

/// <summary>Filtros combináveis com paginação e ordenação. sortBy: id, name, email, crmv, specialty, phone, yearsExperience.</summary>
public sealed class VeterinarianQueryDTO : CollectionQueryDTO
{
    protected override string[] AllowedSortFields => ["id", "name", "email", "crmv", "specialty", "phone", "yearsExperience"];
    /// <summary>Nome parcial, sem diferenciar maiúsculas/minúsculas.</summary>
    public string? Name { get; set; }
    /// <summary>Especialidade; aceita somente valores definidos em SpecialtyEnum.</summary>
    [EnumDataType(typeof(SpecialtyEnum))]
    public SpecialtyEnum? Specialty { get; set; }
}
