using MiniBlogAPI.Models.Entities;

namespace MiniBlogAPI.Repositories;

/// User-specific repository interface.
/// Kế thừa IRepository<User> và thêm methods đặc thù cho User.
public interface  IUserRepository: IRepository<User>
{
    // tìm user theo username
    Task<User?> GetByUsernameAsync(string username);

    // tìm user theo email
    Task<User?> GetByEmailAsync(string email);

    // kiểm tra username đã tồn tại chưa
    Task<bool> UsernameExistAsync(string username);

    // kiem tra xem email ton tai chua
    Task<bool> EmailExistAsync(string email);

    // Lấy user với tất cả posts của user đó (include navigation).
    Task<User?> GetWithPostsAsync(int userId);
}