using GuardianPet.DTOs.Request;
using GuardianPet.Enums;
using GuardianPet.Exceptions;
using GuardianPet.DTOs.Response;
using GuardianPet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GuardianPet.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/veterinarians")]
    public class VeterinariansController : ControllerBase
    {
        private readonly VeterinarianService _veterinarianService;

        public VeterinariansController(VeterinarianService veterinarianService)
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
        [HttpGet]
        [ProducesResponseType(typeof(List<VeterinarianResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll()
        {
            var veterinarians = await _veterinarianService.FindAllAsync();
            return Ok(veterinarians);
        }

        /// <summary>Obtém um registro de veterinário pelo identificador.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de veterinário.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VeterinarianResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id)
        {
            var veterinarian = await _veterinarianService.FindByIdAsync(id);
            return Ok(veterinarian);
        }

        /// <summary>Busca registros de veterinário pelo nome.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="name">Nome utilizado na busca.</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        [HttpGet("search/name")]
        [ProducesResponseType(typeof(List<VeterinarianResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var veterinarians = await _veterinarianService.SearchByNameAsync(name);
            return Ok(veterinarians);
        }

        /// <summary>Lista veterinários da especialidade informada.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="specialty">Especialidade veterinária a filtrar.</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        [HttpGet("search/specialty")]
        [ProducesResponseType(typeof(List<VeterinarianResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchBySpecialty([FromQuery] SpecialtyEnum specialty)
        {
            var veterinarians = await _veterinarianService.SearchBySpecialtyAsync(specialty);
            return Ok(veterinarians);
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
