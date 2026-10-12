using GuardianPet.Data;
using GuardianPet.Models;
using Microsoft.EntityFrameworkCore;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public class ConsultationRepository : IConsultationRepository
    {
        private readonly AppDbContext _context;

        public ConsultationRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Consultation> FilteredQuery(ConsultationQueryDTO options)
        {
            var query = _context.Consultations.AsQueryable();
            if (options.Status.HasValue) query = query.Where(x => x.Status == options.Status.Value);
            if (options.UserId.HasValue) query = query.Where(x => x.UserId == options.UserId.Value);
            if (options.PetId.HasValue) query = query.Where(x => x.PetId == options.PetId.Value);
            if (options.VeterinarianId.HasValue) query = query.Where(x => x.VeterinarianId == options.VeterinarianId.Value);
            if (options.DateFrom.HasValue) query = query.Where(x => x.ConsultationDate >= options.DateFrom.Value);
            if (options.DateTo.HasValue) query = query.Where(x => x.ConsultationDate <= options.DateTo.Value);
            return query;
        }

        public Task<long> CountAsync(ConsultationQueryDTO options)
        {
            options.EnsureValid();
            return FilteredQuery(options).LongCountAsync();
        }

        public async Task<List<Consultation>> QueryAsync(ConsultationQueryDTO options)
        {
            options.EnsureValid();
            var query = FilteredQuery(options);
            query = options.SortBy?.ToLowerInvariant() switch
            {
                "consultationdate" => CollectionQuery.Apply(query, options, x => x.ConsultationDate, x => x.Id),
                "status" => CollectionQuery.Apply(query, options, x => x.Status, x => x.Id),
                "userid" => CollectionQuery.Apply(query, options, x => x.UserId, x => x.Id),
                "petid" => CollectionQuery.Apply(query, options, x => x.PetId, x => x.Id),
                "veterinarianid" => CollectionQuery.Apply(query, options, x => x.VeterinarianId, x => x.Id),
                _ => CollectionQuery.Apply(query, options, x => x.Id, x => x.Id)
            };
            return await query.ToListAsync();
        }

        public async Task<List<Consultation>> FindAllAsync()
        {
            return await _context.Consultations
                .Include(c => c.User)
                .Include(c => c.Pet)
                .Include(c => c.Veterinarian)
                .ToListAsync();
        }

        public async Task<Consultation?> FindByIdAsync(long id)
        {
            return await _context.Consultations
                .Include(c => c.User)
                .Include(c => c.Pet)
                .Include(c => c.Veterinarian)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Consultation> CreateAsync(Consultation consultation)
        {
            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();
            return consultation;
        }

        public async Task UpdateAsync(Consultation consultation)
        {
            _context.Consultations.Update(consultation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Consultation consultation)
        {
            _context.Consultations.Remove(consultation);
            await _context.SaveChangesAsync();
        }
    }
}