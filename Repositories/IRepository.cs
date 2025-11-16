namespace MiniBlogAPI.Repositories;

/// <summary>
/// Generic Repository Interface - định nghĩa các operations cơ bản cho tất cả entities.
/// T là generic type constraint - phải là class (reference type).
/// </summary>
/// <typeparam name="T">Entity type (User, Post, Comment...)</typeparam>
public interface IRepository<T> where T : class
{
    // ==================== READ Operations ====================

    /// Lấy tất cả entities (async).
    /// <returns>IEnumerable của entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// Lấy entity theo ID.
    /// <param name="id">Primary key value</param>
    /// <returns>Entity hoặc null nếu không tìm thấy</returns>
    Task<T?> GetByIdAsync(int id);

    /// Tìm entities theo predicate (điều kiện).
    /// Ví dụ: Find(u => u.Email == "test@test.com")
    /// <param name="predicate">Lambda expression điều kiện</param>
    /// <returns>IEnumerable của entities thỏa điều kiện</returns>
    Task<IEnumerable<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate);

    // ==================== CREATE Operation ====================

    /// Thêm entity mới vào database.
    /// <param name="entity">Entity cần thêm</param>
    /// <returns>Entity đã được thêm (với ID đã generate)</returns>
    Task<T> AddAsync(T entity);

    // ==================== UPDATE Operation ====================

    /// Cập nhật entity trong database.
    /// <param name="entity">Entity cần update (phải có ID hợp lệ)</param>
    Task<T> UpdateAsync(T entity);

    // ==================== DELETE Operations ====================

    /// Xóa entity theo ID.
    /// <param name="id">ID của entity cần xóa</param>
   
    Task DeleteAsync(int id);

    /// Xóa entity.
    /// <param name="entity">Entity cần xóa</param>
    Task DeleteAsync(T entity);

    // ==================== Utility Operations ====================

    /// Đếm số lượng entities.
    /// <returns>Tổng số entities</returns>
    Task<int> CountAsync();

    /// Kiểm tra entity có tồn tại không.
    /// <param name="id">ID cần kiểm tra</param>
    /// <returns>True nếu tồn tại, false nếu không</returns>
    Task<bool> ExistsAsync(int id);
}