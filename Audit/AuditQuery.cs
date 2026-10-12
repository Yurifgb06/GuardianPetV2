using System.ComponentModel.DataAnnotations;

namespace GuardianPet.Audit;

public sealed class AuditQuery : IValidatableObject
{
    /// <summary>Pet, User, Veterinarian ou Consultation.</summary>
    public string? EntityType { get; set; }
    /// <summary>Identificador Oracle do recurso.</summary>
    public long? EntityId { get; set; }
    /// <summary>Created, Updated ou Deleted.</summary>
    public string? Action { get; set; }
    /// <summary>Identificador de correlação exato.</summary>
    [StringLength(128)]
    public string? CorrelationId { get; set; }
    /// <summary>Data/hora UTC inicial inclusiva.</summary>
    public DateTime? FromUtc { get; set; }
    /// <summary>Data/hora UTC final inclusiva.</summary>
    public DateTime? ToUtc { get; set; }
    /// <summary>Página a partir de 1 (padrão 1).</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    /// <summary>Itens por página: 1 a 100 (padrão 20).</summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EntityType is not null && EntityType is not ("Pet" or "User" or "Veterinarian" or "Consultation"))
            yield return new("entityType inválido.", [nameof(EntityType)]);
        if (Action is not null && Action is not ("Created" or "Updated" or "Deleted"))
            yield return new("action inválida.", [nameof(Action)]);
        if (FromUtc.HasValue && ToUtc.HasValue && FromUtc > ToUtc)
            yield return new("fromUtc deve preceder toUtc.", [nameof(FromUtc), nameof(ToUtc)]);
        if (((long)Page - 1) * PageSize > int.MaxValue)
            yield return new("Página excede o limite suportado.", [nameof(Page)]);
    }

    public void EnsureValid()
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(this, new ValidationContext(this), results, true))
            throw new GuardianPet.Exceptions.ApiException("Parâmetros de auditoria inválidos.", 400);
    }
}
