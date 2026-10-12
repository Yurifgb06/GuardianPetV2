using GuardianPet.Audit;
using GuardianPet.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuardianPet.Controllers;

[ApiController]
[Authorize(Policy = "ManageUsers")]
[Route("api/audit-events")]
public sealed class AuditController(AuditService service) : ControllerBase
{
    /// <summary>Consulta eventos mínimos de auditoria armazenados no MongoDB.</summary>
    /// <remarks>Exige JWT e users.manage. Auditoria best-effort: não garante transação conjunta com Oracle. Retorna array ordenado por occurredAtUtc/id descendente.</remarks>
    /// <param name="query">Filtros exatos combináveis e paginação obrigatoriamente limitada (padrão 1/20, máximo 100).</param>
    /// <param name="cancellationToken">Cancelamento da requisição.</param>
    /// <param name="includeLinks">Opcional: true retorna items/links de navegação; false retorna array. Os itens não anunciam rotas inexistentes.</param>
    /// <returns>Eventos sem dados pessoais, credenciais ou conteúdo médico; 503 se a auditoria estiver indisponível.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditEvent>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> FindAll([FromQuery] AuditQuery query, CancellationToken cancellationToken,
        [FromQuery] bool includeLinks = false)
    {
        var events = await service.QueryAsync(query, cancellationToken);
        if (!includeLinks) return Ok(events);
        return Ok(AuditLinks.Create(this, events, query, await service.CountAsync(query, cancellationToken)));
    }
}
