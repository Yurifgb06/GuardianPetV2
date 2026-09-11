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
    [Route("api/pets")]
    public class PetsController : ControllerBase
    {
        private readonly PetService _petService;

        public PetsController(PetService petService)
        {
            _petService = petService;
        }

        /// <summary>Cadastra um novo pet.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="dto">Dados de pet para cadastro.</param>
        /// <returns>Registro criado e endereço para consultá-lo.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(PetResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] PetRequestDTO dto)
        {
            var pet = await _petService.CreateAsync(dto);
            return CreatedAtAction(nameof(FindById), new { id = pet.Id }, pet);
        }

        /// <summary>Lista os registros de pet.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<PetResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll()
        {
            var pets = await _petService.FindAllAsync();
            return Ok(pets);
        }

        /// <summary>Obtém um registro de pet pelo identificador.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de pet.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id)
        {
            var pet = await _petService.FindByIdAsync(id);
            return Ok(pet);
        }

        /// <summary>Filtra pets por espécie e/ou porte; filtros omitidos não restringem a busca.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="species">Espécie a filtrar; opcional.</param>
        /// <param name="petSize">Porte a filtrar; opcional.</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<PetResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search(
            [FromQuery] string? species,
            [FromQuery] PetSizeEnum? petSize)
        {
            var pets = await _petService.SearchAsync(species, petSize);
            return Ok(pets);
        }

        /// <summary>Atualiza os dados de um registro de pet.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de pet.</param>
        /// <param name="dto">Dados de pet para atualização.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] PetRequestDTO dto)
        {
            var pet = await _petService.UpdateAsync(id, dto);
            return Ok(pet);
        }

        /// <summary>Exclui um registro de pet.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de pet.</param>
        /// <returns>Sem conteúdo após a exclusão.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            await _petService.DeleteAsync(id);
            return NoContent();
        }
    }
}
