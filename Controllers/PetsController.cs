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
    [Route("api/pets")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;

        public PetsController(IPetService petService)
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
        /// <param name="options">Filtros combináveis; page >= 1, pageSize 1..100 (padrões 1/20). sortBy: id, name, species, breed, age, weight, petSize; sortDirection asc/desc. Desempate por id; sem parâmetros mantém o array original.</param>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<PetResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll([FromQuery] PetQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            var pets = await _petService.QueryAsync(options);
            if (!includeLinks) return Ok(pets);
            return Ok(await Hateoas.CollectionAsync(this, pets, options, item => item.Id,
                () => _petService.CountAsync(options)));
        }

        /// <summary>Obtém um registro de pet pelo identificador.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="id">Identificador do registro de pet.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PetResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id, [FromQuery] bool includeLinks = false)
        {
            var pet = await _petService.FindByIdAsync(id);
            return includeLinks ? Ok(Hateoas.Resource(this, pet)) : Ok(pet);
        }

        /// <summary>Filtra pets por espécie e/ou porte; filtros omitidos não restringem a busca.</summary>
        /// <remarks>Exige autenticação Bearer JWT.</remarks>
        /// <param name="options">Espécie e porte opcionais, page/pageSize e sortBy (id, name, species, breed, age, weight, petSize)/sortDirection (asc/desc). Array JSON sem envelope.</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<PetResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Search([FromQuery] PetQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            var pets = await _petService.QueryAsync(options);
            if (!includeLinks) return Ok(pets);
            return Ok(await Hateoas.CollectionAsync(this, pets, options, item => item.Id,
                () => _petService.CountAsync(options)));
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
