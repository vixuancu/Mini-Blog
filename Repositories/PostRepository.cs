using Microsoft.EntityFrameworkCore;
using MiniBlogAPI.Data;
using MiniBlogAPI.Models.Entities;

namespace MiniBlogAPI.Repositories;

/// Post Repository Implementation.
public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(BlogContext context) : base(context)
    {
    }

    /// Lấy tất cả posts của 1 user.
    public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)  // Mới nhất trước
            .ToListAsync();
    }

    /// Lấy post với tất cả details (User, Comments).
    public async Task<Post?> GetWithDetailsAsync(int postId)
    {
        return await _dbSet
            .Include(p => p.User)           // Include User info
            .Include(p => p.Comments)       // Include Comments
                .ThenInclude(c => c.User)   // Include User của mỗi Comment
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    /// Search posts theo title (case-insensitive).
    public async Task<IEnumerable<Post>> SearchByTitleAsync(string searchTerm)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Title, $"%{searchTerm}%"))  // PostgreSQL ILIKE
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// Lấy posts với pagination.
    public async Task<IEnumerable<Post>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)  // Bỏ qua các trang trước
            .Take(pageSize)                      // Lấy số lượng pageSize
            .ToListAsync();
    }

    /// Lấy posts mới nhất.
    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}