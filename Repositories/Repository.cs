using Microsoft.EntityFrameworkCore;
using MiniBlogAPI.Data;
using System.Linq.Expressions;

namespace MiniBlogAPI.Repositories;

/// Generic Repository Implementation - implement các operations cơ bản.
/// Class này sẽ được inherit bởi specific repositories (UserRepository, PostRepository...).
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    // DbContext và DbSet được inject và store
    protected readonly BlogContext _context;
    protected readonly DbSet<T> _dbSet;

    /// Constructor - DI inject BlogContext.
    /// <param name="context">BlogContext instance từ DI</param>
    public Repository(BlogContext context)
    {
        _context = context;
        _dbSet = context.Set<T>(); // Get DbSet cho entity type T
    }

    // ==================== READ Operations ====================

    /// Lấy tất cả entities.
    /// Sử dụng AsNoTracking() cho read-only operations (performance tốt hơn).
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()  // Không track changes (read-only, faster)
            .ToListAsync();
    }

    /// Lấy entity theo ID.
    /// Sử dụng FindAsync() - optimized cho primary key lookup.
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        // FindAsync() tự động tìm theo primary key
        return await _dbSet.FindAsync(id);
    }

    /// Tìm entities theo điều kiện.
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(predicate)  // Apply filter
            .ToListAsync();
    }

    // ==================== CREATE Operation ====================
    /// Thêm entity mới.
    public virtual async Task<T> AddAsync(T entity)
    {
        // Add to DbSet (chỉ track, chưa save)
        await _dbSet.AddAsync(entity);

        // SaveChanges() - execute SQL INSERT
        await _context.SaveChangesAsync();

        // Return entity với ID đã được generate
        return entity;
    }

    // ==================== UPDATE Operation ====================
    /// Cập nhật entity.
    public virtual async Task<T> UpdateAsync(T entity)
    {
        // Attach entity và mark as Modified
        _dbSet.Update(entity);

        // SaveChanges() - execute SQL UPDATE
        await _context.SaveChangesAsync();

        // Return updated entity
        return entity;
    }

    // ==================== DELETE Operations ====================

    /// Xóa entity theo ID.
    public virtual async Task DeleteAsync(int id)
    {
        // Tìm entity theo ID
        var entity = await GetByIdAsync(id);

        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }
    /// Xóa entity.
    public virtual async Task DeleteAsync(T entity)
    {
        // Remove from DbSet
        _dbSet.Remove(entity);

        // SaveChanges() - execute SQL DELETE
        await _context.SaveChangesAsync();
    }

    // ==================== Utility Operations ====================

    /// Đếm số lượng entities.
    public virtual async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }

    /// Kiểm tra entity có tồn tại không.
    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.FindAsync(id) != null;
    }


}