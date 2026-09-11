using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GuardianPet.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponseDTO> CreateAsync(UserRequestDTO dto)
        {
            if (await _userRepository.ExistsByEmailAsync(dto.Email))
                throw new ApiException("Email já cadastrado", 400);

            if (await _userRepository.ExistsByCpfAsync(dto.Cpf))
                throw new ApiException("CPF já cadastrado", 400);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Cpf = dto.Cpf,
                Phone = dto.Phone
            };

            user.Password = _passwordHasher.HashPassword(user, dto.Password);
            var savedUser = await _userRepository.CreateAsync(user);
            return ToResponse(savedUser);
        }

        public async Task<List<UserResponseDTO>> FindAllAsync()
        {
            var users = await _userRepository.FindAllAsync();
            return users.Select(ToResponse).ToList();
        }

        public async Task<UserResponseDTO> FindByIdAsync(long id)
        {
            var user = await _userRepository.FindByIdAsync(id)
                ?? throw new ApiException("Usuário não encontrado", 404);

            return ToResponse(user);
        }

        public async Task<UserResponseDTO> FindByEmailAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email)
                ?? throw new ApiException("Email não encontrado", 404);

            return ToResponse(user);
        }

        public async Task<List<UserResponseDTO>> SearchByNameAsync(string name)
        {
            var users = await _userRepository.SearchByNameAsync(name);
            return users.Select(ToResponse).ToList();
        }

        public async Task<UserResponseDTO> UpdateAsync(long id, UserRequestDTO dto)
        {
            var user = await _userRepository.FindByIdAsync(id)
                ?? throw new ApiException("Usuário não encontrado", 404);

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Password = _passwordHasher.HashPassword(user, dto.Password);
            user.Cpf = dto.Cpf;
            user.Phone = dto.Phone;

            await _userRepository.UpdateAsync(user);
            return ToResponse(user);
        }

        public async Task DeleteAsync(long id)
        {
            var user = await _userRepository.FindByIdAsync(id)
                ?? throw new ApiException("Usuário não encontrado", 404);

            await _userRepository.DeleteAsync(user);
        }

        private static UserResponseDTO ToResponse(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone
            };
        }
    }
}
