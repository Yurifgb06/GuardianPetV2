using GuardianPet.DTOs.Request;
using GuardianPet.Enums;
using GuardianPet.Exceptions;
using GuardianPet.DTOs.Response;
using GuardianPet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GuardianPet.DTOs.Query;
using GuardianPet.Presentation;

namespace GuardianPet.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/veterinarians")]
    public class VeterinariansController : ControllerBase
    {
        private readonly IVeterinarianService _veterinarianService;

        public VeterinariansController(IVeterinarianService veterinarianService)
        {
            _veterinarianService = veterinarianService;
        }

        /// <summary>Cadastra um novo veterinário.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="dto">Dados de veterinário para cadastro.</param>
        /// <returns>Registro criado e endereço para consultá-lo.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(VeterinarianResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] VeterinarianRequestDTO dto)
        {
            var veterinarian = await _veterinarianService.CreateAsync(dto);
            return CreatedAtAction(nameof(FindById), new { id = veterinarian.Id }, veterinarian);
        }

        /// <summary>Lista os registros de veterinário.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        /// <param name="options">Filtros combináveis; page >= 1, pageSize 1..100 (padrões 1/20). sortBy: id, name, email, crmv, specialty, phone, yearsExperience; sortDirection asc/desc. Desempate por id; sem parâmetros mantém o array original.</param>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<VeterinarianResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll([FromQuery] VeterinarianQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            var veterinarians = await _veterinarianService.QueryAsync(options);
            if (!includeLinks) return Ok(veterinarians);
            return Ok(await Hateoas.CollectionAsync(this, veterinarians, options, item => item.Id,
                () => _veterinarianService.CountAsync(options)));
        }

        /// <summary>Obtém um registro de veterinário pelo identificador.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de veterinário.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VeterinarianResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id, [FromQuery] bool includeLinks = false)
        {
            var veterinarian = await _veterinarianService.FindByIdAsync(id);
            return includeLinks ? Ok(Hateoas.Resource(this, veterinarian)) : Ok(veterinarian);
        }

        /// <summary>Busca registros de veterinário pelo nome.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="name">Nome utilizado na busca.</param>
        /// <param name="options">Especialidade opcional; page/pageSize e sortBy (id, name, email, crmv, specialty, phone, yearsExperience)/sortDirection (asc/desc).</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("search/name")]
        [ProducesResponseType(typeof(List<VeterinarianResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByName([FromQuery] string name, [FromQuery] VeterinarianQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            options.Name = name;
            var veterinarians = await _veterinarianService.QueryAsync(options);
            if (!includeLinks) return Ok(veterinarians);
            return Ok(await Hateoas.CollectionAsync(this, veterinarians, options, item => item.Id,
                () => _veterinarianService.CountAsync(options)));
        }

        /// <summary>Lista veterinários da especialidade informada.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="specialty">Especialidade veterinária a filtrar.</param>
        /// <param name="options">Nome opcional; page/pageSize e sortBy (id, name, email, crmv, specialty, phone, yearsExperience)/sortDirection (asc/desc).</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("search/specialty")]
        [ProducesResponseType(typeof(List<VeterinarianResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchBySpecialty([FromQuery] SpecialtyEnum specialty, [FromQuery] VeterinarianQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            options.Specialty = specialty;
            var veterinarians = await _veterinarianService.QueryAsync(options);
            if (!includeLinks) return Ok(veterinarians);
            return Ok(await Hateoas.CollectionAsync(this, veterinarians, options, item => item.Id,
                () => _veterinarianService.CountAsync(options)));
        }

        /// <summary>Atualiza os dados de um registro de veterinário.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de veterinário.</param>
        /// <param name="dto">Dados de veterinário para atualização.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(VeterinarianResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] VeterinarianRequestDTO dto)
        {
            var veterinarian = await _veterinarianService.UpdateAsync(id, dto);
            return Ok(veterinarian);
        }

        /// <summary>Exclui um registro de veterinário.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de veterinário.</param>
        /// <returns>Sem conteúdo após a exclusão.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            await _veterinarianService.DeleteAsync(id);
            return NoContent();
        }
    }
}
