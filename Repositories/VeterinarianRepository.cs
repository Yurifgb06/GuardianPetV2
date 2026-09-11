using GuardianPet.Data;
using GuardianPet.Enums;
using GuardianPet.Models;
using Microsoft.EntityFrameworkCore;

namespace GuardianPet.Repositories
{
    public class VeterinarianRepository : IVeterinarianRepository
    {
        private readonly AppDbContext _context;

        public VeterinarianRepository(AppDbContext context)
        {
            _context = context;
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
            return await _context.Veterinarians
                .Where(v => v.Name.ToLower().Contains(name.ToLower()))
                .ToListAsync();
        }

        public async Task<List<Veterinarian>> SearchBySpecialtyAsync(SpecialtyEnum specialty)
        {
            return await _context.Veterinarians
                .Where(v => v.Specialty == specialty)
                .ToListAsync();
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