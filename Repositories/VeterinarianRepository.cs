using GuardianPet.Data;
using GuardianPet.Enums;
using GuardianPet.Models;
using Microsoft.EntityFrameworkCore;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public class VeterinarianRepository : IVeterinarianRepository
    {
        private readonly AppDbContext _context;

        public VeterinarianRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Veterinarian> FilteredQuery(VeterinarianQueryDTO options)
        {
            var query = _context.Veterinarians.AsQueryable();
            if (options.Name is not null) query = query.Where(x => x.Name.ToLower().Contains(options.Name.ToLower()));
            if (options.Specialty.HasValue) query = query.Where(x => x.Specialty == options.Specialty.Value);
            return query;
        }

        public Task<long> CountAsync(VeterinarianQueryDTO options)
        {
            options.EnsureValid();
            return FilteredQuery(options).LongCountAsync();
        }

        public async Task<List<Veterinarian>> QueryAsync(VeterinarianQueryDTO options)
        {
            options.EnsureValid();
            var query = FilteredQuery(options);
            query = options.SortBy?.ToLowerInvariant() switch
            {
                "name" => CollectionQuery.Apply(query, options, x => x.Name, x => x.Id),
                "email" => CollectionQuery.Apply(query, options, x => x.Email, x => x.Id),
                "crmv" => CollectionQuery.Apply(query, options, x => x.Crmv, x => x.Id),
                "specialty" => CollectionQuery.Apply(query, options, x => x.Specialty, x => x.Id),
                "phone" => CollectionQuery.Apply(query, options, x => x.Phone, x => x.Id),
                "yearsexperience" => CollectionQuery.Apply(query, options, x => x.YearsExperience, x => x.Id),
                _ => CollectionQuery.Apply(query, options, x => x.Id, x => x.Id)
            };
            return await query.ToListAsync();
        }

        public async Task<List<Veterinarian>> FindAllAsync()
        {
            return await _context.Veterinarians.ToListAsync();
        }

        public async Task<Veterinarian?> FindByIdAsync(long id)
        {
            return await _context.Veterinarians.FindAsync(id);
        }

        public async Task<List<Veterinarian>> SearchByNameAsync(string name)
        {
            return await QueryAsync(new VeterinarianQueryDTO { Name = name });
        }

        public async Task<List<Veterinarian>> SearchBySpecialtyAsync(SpecialtyEnum specialty)
        {
            return await QueryAsync(new VeterinarianQueryDTO { Specialty = specialty });
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Veterinarians.AnyAsync(v => v.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ExistsByCrmvAsync(string crmv)
        {
            return await _context.Veterinarians.AnyAsync(v => v.Crmv == crmv);
        }

        public async Task<Veterinarian> CreateAsync(Veterinarian veterinarian)
        {
            _context.Veterinarians.Add(veterinarian);
            await _context.SaveChangesAsync();
            return veterinarian;
        }

        public async Task UpdateAsync(Veterinarian veterinarian)
        {
            _context.Veterinarians.Update(veterinarian);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Veterinarian veterinarian)
        {
            _context.Veterinarians.Remove(veterinarian);
            await _context.SaveChangesAsync();
        }
    }
}