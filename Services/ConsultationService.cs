using GuardianPet.DTOs.Request;
using GuardianPet.DTOs.Response;
using GuardianPet.Exceptions;
using GuardianPet.Models;
using GuardianPet.Repositories;

namespace GuardianPet.Services
{
    public class ConsultationService
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPetRepository _petRepository;
        private readonly IVeterinarianRepository _veterinarianRepository;

        public ConsultationService(
            IConsultationRepository consultationRepository,
            IUserRepository userRepository,
            IPetRepository petRepository,
            IVeterinarianRepository veterinarianRepository)
        {
            _consultationRepository = consultationRepository;
            _userRepository = userRepository;
            _petRepository = petRepository;
            _veterinarianRepository = veterinarianRepository;
        }

        public async Task<ConsultationResponseDTO> CreateAsync(ConsultationRequestDTO dto)
        {
            await ValidateRelationshipsAsync(dto);

            var consultation = new Consultation
            {
                UserId = dto.UserId,
                PetId = dto.PetId,
                VeterinarianId = dto.VeterinarianId,
                ConsultationDate = dto.ConsultationDate,
                Symptoms = dto.Symptoms,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                Observations = dto.Observations,
                Status = dto.Status
            };

            var savedConsultation = await _consultationRepository.CreateAsync(consultation);
            return ToResponse(savedConsultation);
        }

        public async Task<List<ConsultationResponseDTO>> FindAllAsync()
        {
            var consultations = await _consultationRepository.FindAllAsync();
            return consultations.Select(ToResponse).ToList();
        }

        public async Task<ConsultationResponseDTO> FindByIdAsync(long id)
        {
            var consultation = await _consultationRepository.FindByIdAsync(id)
                ?? throw new ApiException("Consulta não encontrada", 404);

            return ToResponse(consultation);
        }

        public async Task<ConsultationResponseDTO> UpdateAsync(long id, ConsultationRequestDTO dto)
        {
            var consultation = await _consultationRepository.FindByIdAsync(id)
                ?? throw new ApiException("Consulta não encontrada", 404);

            await ValidateRelationshipsAsync(dto);

            consultation.UserId = dto.UserId;
            consultation.PetId = dto.PetId;
            consultation.VeterinarianId = dto.VeterinarianId;
            consultation.ConsultationDate = dto.ConsultationDate;
            consultation.Symptoms = dto.Symptoms;
            consultation.Diagnosis = dto.Diagnosis;
            consultation.Treatment = dto.Treatment;
            consultation.Observations = dto.Observations;
            consultation.Status = dto.Status;

            await _consultationRepository.UpdateAsync(consultation);
            return ToResponse(consultation);
        }

        public async Task DeleteAsync(long id)
        {
            var consultation = await _consultationRepository.FindByIdAsync(id)
                ?? throw new ApiException("Consulta não encontrada", 404);

            await _consultationRepository.DeleteAsync(consultation);
        }

        private async Task ValidateRelationshipsAsync(ConsultationRequestDTO dto)
        {
            if (await _userRepository.FindByIdAsync(dto.UserId) is null)
                throw new ApiException("Usuário informado não encontrado", 400);

            if (await _petRepository.FindByIdAsync(dto.PetId) is null)
                throw new ApiException("Pet informado não encontrado", 400);

            if (await _veterinarianRepository.FindByIdAsync(dto.VeterinarianId) is null)
                throw new ApiException("Veterinário informado não encontrado", 400);
        }

        private static ConsultationResponseDTO ToResponse(Consultation consultation)
        {
            return new ConsultationResponseDTO
            {
                Id = consultation.Id,
                ConsultationDate = consultation.ConsultationDate,
                Symptoms = consultation.Symptoms,
                Diagnosis = consultation.Diagnosis,
                Treatment = consultation.Treatment,
                Observations = consultation.Observations,
                Status = consultation.Status
            };
        }
    }
}
