using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Enums;
using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Repositories;

namespace GuardianPet.Services
{
    public class VeterinarianService
    {
        private readonly IVeterinarianRepository _veterinarianRepository;

        public VeterinarianService(IVeterinarianRepository veterinarianRepository)
        {
            _veterinarianRepository = veterinarianRepository;
        }

        public async Task<VeterinarianResponseDTO> CreateAsync(VeterinarianRequestDTO dto)
        {
            if (await _veterinarianRepository.ExistsByEmailAsync(dto.Email))
                throw new ApiException("Email já cadastrado", 400);

            if (await _veterinarianRepository.ExistsByCrmvAsync(dto.Crmv))
                throw new ApiException("CRMV já cadastrado", 400);

            var veterinarian = new Veterinarian
            {
                Name = dto.Name,
                Email = dto.Email,
                Crmv = dto.Crmv,
                Specialty = dto.Specialty,
                Phone = dto.Phone,
                YearsExperience = dto.YearsExperience
            };

            var savedVeterinarian = await _veterinarianRepository.CreateAsync(veterinarian);
            return ToResponse(savedVeterinarian);
        }

        public async Task<List<VeterinarianResponseDTO>> FindAllAsync()
        {
            var veterinarians = await _veterinarianRepository.FindAllAsync();
            return veterinarians.Select(ToResponse).ToList();
        }

        public async Task<VeterinarianResponseDTO> FindByIdAsync(long id)
        {
            var veterinarian = await _veterinarianRepository.FindByIdAsync(id)
                ?? throw new ApiException("Veterinário não encontrado", 404);

            return ToResponse(veterinarian);
        }

        public async Task<List<VeterinarianResponseDTO>> SearchByNameAsync(string name)
        {
            var veterinarians = await _veterinarianRepository.SearchByNameAsync(name);
            return veterinarians.Select(ToResponse).ToList();
        }

        public async Task<List<VeterinarianResponseDTO>> SearchBySpecialtyAsync(SpecialtyEnum specialty)
        {
            var veterinarians = await _veterinarianRepository.SearchBySpecialtyAsync(specialty);
            return veterinarians.Select(ToResponse).ToList();
        }

        public async Task<VeterinarianResponseDTO> UpdateAsync(long id, VeterinarianRequestDTO dto)
        {
            var veterinarian = await _veterinarianRepository.FindByIdAsync(id)
                ?? throw new ApiException("Veterinário não encontrado", 404);

            veterinarian.Name = dto.Name;
            veterinarian.Email = dto.Email;
            veterinarian.Crmv = dto.Crmv;
            veterinarian.Specialty = dto.Specialty;
            veterinarian.Phone = dto.Phone;
            veterinarian.YearsExperience = dto.YearsExperience;

            await _veterinarianRepository.UpdateAsync(veterinarian);
            return ToResponse(veterinarian);
        }

        public async Task DeleteAsync(long id)
        {
            var veterinarian = await _veterinarianRepository.FindByIdAsync(id)
                ?? throw new ApiException("Veterinário não encontrado", 404);

            await _veterinarianRepository.DeleteAsync(veterinarian);
        }

        private static VeterinarianResponseDTO ToResponse(Veterinarian veterinarian)
        {
            return new VeterinarianResponseDTO
            {
                Id = veterinarian.Id,
                Name = veterinarian.Name,
                Email = veterinarian.Email,
                Crmv = veterinarian.Crmv,
                Specialty = veterinarian.Specialty,
                Phone = veterinarian.Phone,
                YearsExperience = veterinarian.YearsExperience
            };
        }
    }
}