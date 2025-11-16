using MiniBlogAPI.Models.Entities;

namespace MiniBlogAPI.Repositories;

/// Post-specific repository interface.
public interface IPostRepository : IRepository<Post>
{
    /// Lấy tất cả posts của 1 user.
    Task<IEnumerable<Post>> GetByUserIdAsync(int userId);

    /// Lấy post với comments và user info.
    Task<Post?> GetWithDetailsAsync(int postId);

    /// Search posts theo title.
    Task<IEnumerable<Post>> SearchByTitleAsync(string searchTerm);

    /// Lấy posts với pagination.
    Task<IEnumerable<Post>> GetPagedAsync(int pageNumber, int pageSize);

    /// Lấy posts mới nhất.
    Task<IEnumerable<Post>> GetRecentPostsAsync(int count);
}