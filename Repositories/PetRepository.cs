using GuardianPet.Data;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Observability;
using Microsoft.EntityFrameworkCore;

namespace GuardianPet.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;

        public PetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Pet>> FindAllAsync()
        {
            using var activity = GuardianPetTelemetry.StartActivity("repository", "list", "pet");
            return await _context.Pets.ToListAsync();
        }

        public async Task<Pet?> FindByIdAsync(long id)
        {
            using var activity = GuardianPetTelemetry.StartActivity("repository", "get_by_id", "pet");
            return await _context.Pets.FindAsync(id);
        }

        public async Task<List<Pet>> SearchAsync(string? species, PetSizeEnum? petSize)
        {
            using var activity = GuardianPetTelemetry.StartActivity("repository", "search", "pet");
            var query = _context.Pets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(species))
                query = query.Where(p => p.Species.ToLower().Contains(species.ToLower()));

            if (petSize.HasValue)
                query = query.Where(p => p.PetSize == petSize.Value);

            return await query.ToListAsync();
        }

        public async Task<Pet> CreateAsync(Pet pet)
        {
            using var activity = GuardianPetTelemetry.StartActivity("repository", "create", "pet");
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();
            return pet;
        }

        public async Task UpdateAsync(Pet pet)
        {
            using var activity = GuardianPetTelemetry.StartActivity("repository", "update", "pet");
            _context.Pets.Update(pet);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Pet pet)
        {
            using var activity = GuardianPetTelemetry.StartActivity("repository", "delete", "pet");
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();
        }
    }
}