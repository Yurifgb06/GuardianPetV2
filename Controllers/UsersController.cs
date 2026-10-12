using GuardianPet.DTOs.Request;
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
    [Authorize(Policy = "ManageUsers")]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Cadastra um novo usuário.</summary>
        /// <remarks>Acesso anônimo; não exige token JWT.</remarks>
        /// <param name="dto">Dados de usuário para cadastro.</param>
        /// <returns>Registro criado e endereço para consultá-lo.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] UserRequestDTO dto)
        {
            var user = await _userService.CreateAsync(dto);
            return CreatedAtAction(nameof(FindById), new { id = user.Id }, user);
        }

        /// <summary>Lista os registros de usuário.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        /// <param name="options">Filtros combináveis; page >= 1, pageSize 1..100 (padrões 1/20). sortBy: id, name, email, phone; sortDirection asc/desc. Desempate por id; sem parâmetros mantém o array original.</param>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll([FromQuery] UserQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            var users = await _userService.QueryAsync(options);
            if (!includeLinks) return Ok(users);
            return Ok(await Hateoas.CollectionAsync(this, users, options, item => item.Id,
                () => _userService.CountAsync(options)));
        }

        /// <summary>Obtém um registro de usuário pelo identificador.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="id">Identificador do registro de usuário.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id, [FromQuery] bool includeLinks = false)
        {
            var user = await _userService.FindByIdAsync(id);
            return includeLinks ? Ok(Hateoas.Resource(this, user)) : Ok(user);
        }

        /// <summary>Busca registros de usuário pelo nome.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="name">Nome utilizado na busca.</param>
        /// <param name="options">Filtro adicional de email; page/pageSize e sortBy (id, name, email, phone)/sortDirection (asc/desc). Array JSON sem envelope.</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByName([FromQuery] string name, [FromQuery] UserQueryDTO options, [FromQuery] bool includeLinks = false)
        {
            options.Name = name;
            var users = await _userService.QueryAsync(options);
            if (!includeLinks) return Ok(users);
            return Ok(await Hateoas.CollectionAsync(this, users, options, item => item.Id,
                () => _userService.CountAsync(options)));
        }

        /// <summary>Obtém o usuário associado ao e-mail informado.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="email">E-mail do usuário procurado.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        /// <param name="includeLinks">Opcional: true retorna recurso com data/links ou coleção com items/links; false preserva o JSON original.</param>
        [HttpGet("email")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindByEmail([FromQuery] string email, [FromQuery] bool includeLinks = false)
        {
            var user = await _userService.FindByEmailAsync(email);
            return includeLinks ? Ok(Hateoas.Resource(this, user)) : Ok(user);
        }

        /// <summary>Atualiza os dados de um registro de usuário.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="id">Identificador do registro de usuário.</param>
        /// <param name="dto">Dados de usuário para atualização.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] UserRequestDTO dto)
        {
            var user = await _userService.UpdateAsync(id, dto);
            return Ok(user);
        }

        /// <summary>Exclui um registro de usuário.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="id">Identificador do registro de usuário.</param>
        /// <returns>Sem conteúdo após a exclusão.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            await _userService.DeleteAsync(id);
            return NoContent();
        }
    }
}
