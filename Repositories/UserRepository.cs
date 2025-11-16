using Microsoft.EntityFrameworkCore;
using MiniBlogAPI.Data;
using MiniBlogAPI.Models.Entities;

namespace MiniBlogAPI.Repositories ;

// User Repository Implementation.
// Kế thừa Repository<User> và implement IUserRepository.

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(BlogContext context) : base(context)
    {
        // call base constructor
    }
    // tim user theo username
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    // tim user theo email
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    // kiem tra username da ton tai chua
    public async Task<bool> UsernameExistAsync(string username)
    {
        return await _dbSet
            .AnyAsync(u => u.Username == username);
    }

    // kiem tra email da ton tai chua
    public async Task<bool> EmailExistAsync(string email)
    {
        return await _dbSet
            .AnyAsync(u => u.Email == email);
    }

    //lay user voi posts (eager loading)
    // include() load related entities trong 1 query
    public async Task<User?> GetWithPostsAsync(int userId)
    {
        return await _dbSet
            .Include(u => u.Posts)  // Eager load Posts navigation property
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

}