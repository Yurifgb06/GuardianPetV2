using GuardianPet.Data;
using GuardianPet.Enums;
using GuardianPet.Models;
using GuardianPet.Observability;
using Microsoft.EntityFrameworkCore;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public class PetRepository : IPetRepository
    {
        private readonly AppDbContext _context;

        public PetRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Pet> FilteredQuery(PetQueryDTO options)
        {
            var query = _context.Pets.AsQueryable();
            if (!string.IsNullOrWhiteSpace(options.Species)) query = query.Where(x => x.Species.ToLower().Contains(options.Species.ToLower()));
            if (options.PetSize.HasValue) query = query.Where(x => x.PetSize == options.PetSize.Value);
            return query;
        }

        public Task<long> CountAsync(PetQueryDTO options)
        {
            options.EnsureValid();
            return FilteredQuery(options).LongCountAsync();
        }

        public async Task<List<Pet>> QueryAsync(PetQueryDTO options)
        {
            options.EnsureValid();
            using var activity = GuardianPetTelemetry.StartActivity("repository", "search", "pet");
            var query = FilteredQuery(options);
            query = options.SortBy?.ToLowerInvariant() switch
            {
                "name" => CollectionQuery.Apply(query, options, x => x.Name, x => x.Id),
                "species" => CollectionQuery.Apply(query, options, x => x.Species, x => x.Id),
                "breed" => CollectionQuery.Apply(query, options, x => x.Breed, x => x.Id),
                "age" => CollectionQuery.Apply(query, options, x => x.Age, x => x.Id),
                "weight" => CollectionQuery.Apply(query, options, x => x.Weight, x => x.Id),
                "petsize" => CollectionQuery.Apply(query, options, x => x.PetSize, x => x.Id),
                _ => CollectionQuery.Apply(query, options, x => x.Id, x => x.Id)
            };
            return await query.ToListAsync();
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
            return await QueryAsync(new PetQueryDTO { Species = species, PetSize = petSize });
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