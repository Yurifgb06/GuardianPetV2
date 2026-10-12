using System.ComponentModel.DataAnnotations;
using GuardianPet.Exceptions;

namespace GuardianPet.DTOs.Query;

public abstract class CollectionQueryDTO : IValidatableObject
{
    /// <summary>Página a partir de 1; padrão 1 quando pageSize é informado.</summary>
    [Range(1, int.MaxValue)]
    public int? Page { get; set; }
    /// <summary>Itens por página (1 a 100); padrão 20 quando page é informado.</summary>
    [Range(1, 100)]
    public int? PageSize { get; set; }
    /// <summary>Campo permitido pela entidade; padrão id quando há paginação ou sortDirection.</summary>
    public string? SortBy { get; set; }
    /// <summary>asc (padrão) ou desc.</summary>
    public string? SortDirection { get; set; }
    protected abstract string[] AllowedSortFields { get; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SortBy is not null && !AllowedSortFields.Contains(SortBy, StringComparer.OrdinalIgnoreCase))
            yield return new ValidationResult("sortBy inválido. Permitidos: " + string.Join(", ", AllowedSortFields), [nameof(SortBy)]);
        if (SortDirection is not null && !string.Equals(SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult("sortDirection deve ser asc ou desc.", [nameof(SortDirection)]);
        if (((long)(Page ?? 1) - 1) * (PageSize ?? 20) > int.MaxValue)
            yield return new ValidationResult("Página excede o limite suportado.", [nameof(Page)]);
    }

    public void EnsureValid()
    {
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(this, new ValidationContext(this), errors, true))
            throw new ApiException(string.Join(" ", errors.Select(e => e.ErrorMessage)), 400);
    }
}
