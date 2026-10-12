using GuardianPet.Data;
using GuardianPet.Models;
using Microsoft.EntityFrameworkCore;
using GuardianPet.DTOs.Query;

namespace GuardianPet.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<User> FilteredQuery(UserQueryDTO options)
        {
            var query = _context.Users.AsQueryable();
            if (options.Name is not null) query = query.Where(x => x.Name.ToLower().Contains(options.Name.ToLower()));
            if (options.Email is not null) query = query.Where(x => x.Email.ToLower() == options.Email.ToLower());
            return query;
        }

        public Task<long> CountAsync(UserQueryDTO options)
        {
            options.EnsureValid();
            return FilteredQuery(options).LongCountAsync();
        }

        public async Task<List<User>> QueryAsync(UserQueryDTO options)
        {
            options.EnsureValid();
            var query = FilteredQuery(options);
            query = options.SortBy?.ToLowerInvariant() switch
            {
                "name" => CollectionQuery.Apply(query, options, x => x.Name, x => x.Id),
                "email" => CollectionQuery.Apply(query, options, x => x.Email, x => x.Id),
                "phone" => CollectionQuery.Apply(query, options, x => x.Phone, x => x.Id),
                _ => CollectionQuery.Apply(query, options, x => x.Id, x => x.Id)
            };
            return await query.ToListAsync();
        }

        public async Task<List<User>> FindAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> FindByIdAsync(long id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<List<User>> SearchByNameAsync(string name)
        {
            return await QueryAsync(new UserQueryDTO { Name = name });
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> ExistsByCpfAsync(string cpf)
        {
            return await _context.Users.AnyAsync(u => u.Cpf == cpf);
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}