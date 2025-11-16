using Microsoft.EntityFrameworkCore;
using MiniBlogAPI.Data;
using MiniBlogAPI.Models.Entities;

namespace MiniBlogAPI.Repositories ;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(BlogContext context) : base(context)
    {
    }

    // lấy tất cả comment của một post
    public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
    {
        return await _dbSet
            .Include(c => c.User)  // Include user info
            .AsNoTracking() // Tối ưu cho đọc dữ liệu
            .Where(c => c.PostId == postId) // Lọc theo postId
            .OrderBy(c => c.CreatedAt)  // Cũ nhất trước (chronological order)
            .ToListAsync();
    }

    // lấy tất cả comment của một user
    public async Task<IEnumerable<Comment>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(c => c.Post)  // Include post info
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)  // Mới nhất trước
            .ToListAsync();
    }

    //lấy comment với user và post info
    public async Task<Comment?> GetWithDetailsAsync(int commentId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Post)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }
}