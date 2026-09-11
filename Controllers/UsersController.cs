using GuardianPet.DTOs.Request;
using GuardianPet.Exceptions;
using GuardianPet.DTOs.Response;
using GuardianPet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GuardianPet.Controllers
{
    [ApiController]
    [Authorize(Policy = "ManageUsers")]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
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
        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindAll()
        {
            var users = await _userService.FindAllAsync();
            return Ok(users);
        }

        /// <summary>Obtém um registro de usuário pelo identificador.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="id">Identificador do registro de usuário.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindById(long id)
        {
            var user = await _userService.FindByIdAsync(id);
            return Ok(user);
        }

        /// <summary>Busca registros de usuário pelo nome.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="name">Nome utilizado na busca.</param>
        /// <returns>Lista de resultados, vazia quando não há correspondências.</returns>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<UserResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            var users = await _userService.SearchByNameAsync(name);
            return Ok(users);
        }

        /// <summary>Obtém o usuário associado ao e-mail informado.</summary>
        /// <remarks>Exige Bearer JWT e a permissão users.manage.</remarks>
        /// <param name="email">E-mail do usuário procurado.</param>
        /// <returns>Registro solicitado ou atualizado; 404 quando não encontrado.</returns>
        [HttpGet("email")]
        [ProducesResponseType(typeof(UserResponseDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FindByEmail([FromQuery] string email)
        {
            var user = await _userService.FindByEmailAsync(email);
            return Ok(user);
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
