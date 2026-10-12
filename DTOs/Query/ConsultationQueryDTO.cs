using System.ComponentModel.DataAnnotations;
using GuardianPet.Enums;

namespace GuardianPet.DTOs.Query;

/// <summary>Filtros combináveis com paginação e ordenação. sortBy: id, consultationDate, status, userId, petId, veterinarianId.</summary>
public sealed class ConsultationQueryDTO : CollectionQueryDTO
{
    protected override string[] AllowedSortFields => ["id", "consultationDate", "status", "userId", "petId", "veterinarianId"];
    /// <summary>Status: SCHEDULED, IN_PROGRESS, COMPLETED ou CANCELED.</summary>
    [EnumDataType(typeof(StatusConsultationEnum))]
    public StatusConsultationEnum? Status { get; set; }
    /// <summary>Identificador do usuário relacionado.</summary>
    public long? UserId { get; set; }
    /// <summary>Identificador do pet relacionado.</summary>
    public long? PetId { get; set; }
    /// <summary>Identificador do veterinário relacionado.</summary>
    public long? VeterinarianId { get; set; }
    /// <summary>Data/hora inicial inclusiva (ISO 8601).</summary>
    public DateTime? DateFrom { get; set; }
    /// <summary>Data/hora final inclusiva (ISO 8601); não pode preceder dateFrom.</summary>
    public DateTime? DateTo { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        foreach (var error in base.Validate(context)) yield return error;
        if (DateFrom.HasValue && DateTo.HasValue && DateFrom > DateTo)
            yield return new ValidationResult("dateFrom deve ser anterior ou igual a dateTo.", [nameof(DateFrom), nameof(DateTo)]);
    }
}
