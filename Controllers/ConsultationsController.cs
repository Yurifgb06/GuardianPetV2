using GuardianPet.DTOs.Request;
using GuardianPet.Exceptions;
using GuardianPet.DTOs.Response;
using GuardianPet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GuardianPet.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/consultations")]
    public class ConsultationsController : ControllerBase
    {
        private readonly ConsultationService _consultationService;

        public ConsultationsController(ConsultationService consultationService)
        {
            _consultationService = consultationService;
        }

        /// <summary>Cadastra uma nova consulta.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="dto">Dados de consulta para cadastro.</param>
        /// <returns>Registro criado e endereço para consultá-lo.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ConsultationResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ConsultationRequestDTO dto)
        {
            var consultation = await _consultationService.CreateAsync(dto);
            return CreatedAtAction(nameof(FindById), new { id = consultation.Id }, consultation);
        }

        /// <summary>Lista os registros de consulta.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ConsultationResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll()
        {
            var consultations = await _consultationService.FindAllAsync();
            return Ok(consultations);
        }

        /// <summary>Obtém um registro de consulta pelo identificador.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de consulta.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ConsultationResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id)
        {
            var consultation = await _consultationService.FindByIdAsync(id);
            return Ok(consultation);
        }

        /// <summary>Atualiza os dados de um registro de consulta.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de consulta.</param>
        /// <param name="dto">Dados de consulta para atualização.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ConsultationResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] ConsultationRequestDTO dto)
        {
            var consultation = await _consultationService.UpdateAsync(id, dto);
            return Ok(consultation);
        }

        /// <summary>Exclui um registro de consulta.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de consulta.</param>
        /// <returns>Sem conteúdo após a exclusão.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            await _consultationService.DeleteAsync(id);
            return NoContent();
        }
    }
}
