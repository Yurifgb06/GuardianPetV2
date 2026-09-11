using GuardianPet.Data;
using GuardianPet.Models;
using Microsoft.EntityFrameworkCore;

namespace GuardianPet.Repositories
{
    public class ConsultationRepository : IConsultationRepository
    {
        private readonly AppDbContext _context;

        public ConsultationRepository(AppDbContext context)
        {
            _context = context;
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