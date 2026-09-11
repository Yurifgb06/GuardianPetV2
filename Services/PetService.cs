using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Enums;
using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Observability;
using GuardianPet.Repositories;

namespace GuardianPet.Services
{
    public class PetService
    {
        private readonly IPetRepository _petRepository;

        public PetService(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task<PetResponseDTO> CreateAsync(PetRequestDTO dto)
        {
            using var activity = GuardianPetTelemetry.StartActivity("service", "create", "pet");
            var pet = new Pet
            {
                Name = dto.Name,
                Species = dto.Species,
                Breed = dto.Breed,
                AgeType = dto.AgeType,
                Age = dto.Age,
                Weight = dto.Weight,
                PetSize = dto.PetSize
            };

            var savedPet = await _petRepository.CreateAsync(pet);
            return ToResponse(savedPet);
        }

        public async Task<List<PetResponseDTO>> FindAllAsync()
        {
            using var activity = GuardianPetTelemetry.StartActivity("service", "list", "pet");
            var pets = await _petRepository.FindAllAsync();
            return pets.Select(ToResponse).ToList();
        }

        public async Task<PetResponseDTO> FindByIdAsync(long id)
        {
            using var activity = GuardianPetTelemetry.StartActivity("service", "get_by_id", "pet");
            var pet = await _petRepository.FindByIdAsync(id)
                ?? throw new ApiException("Pet não encontrado", 404);

            return ToResponse(pet);
        }

        public async Task<List<PetResponseDTO>> SearchAsync(string? species, PetSizeEnum? petSize)
        {
            using var activity = GuardianPetTelemetry.StartActivity("service", "search", "pet");
            var pets = await _petRepository.SearchAsync(species, petSize);
            return pets.Select(ToResponse).ToList();
        }

        public async Task<PetResponseDTO> UpdateAsync(long id, PetRequestDTO dto)
        {
            using var activity = GuardianPetTelemetry.StartActivity("service", "update", "pet");
            var pet = await _petRepository.FindByIdAsync(id)
                ?? throw new ApiException("Pet não encontrado", 404);

            pet.Name = dto.Name;
            pet.Species = dto.Species;
            pet.Breed = dto.Breed;
            pet.AgeType = dto.AgeType;
            pet.Age = dto.Age;
            pet.Weight = dto.Weight;
            pet.PetSize = dto.PetSize;

            await _petRepository.UpdateAsync(pet);
            return ToResponse(pet);
        }

        public async Task DeleteAsync(long id)
        {
            using var activity = GuardianPetTelemetry.StartActivity("service", "delete", "pet");
            var pet = await _petRepository.FindByIdAsync(id)
                ?? throw new ApiException("Pet não encontrado", 404);

            await _petRepository.DeleteAsync(pet);
        }

        private static PetResponseDTO ToResponse(Pet pet)
        {
            return new PetResponseDTO
            {
                Id = pet.Id,
                Name = pet.Name,
                Species = pet.Species,
                Breed = pet.Breed,
                AgeType = pet.AgeType,
                Age = pet.Age,
                Weight = pet.Weight,
                PetSize = pet.PetSize
            };
        }
    }
}