using Microsoft.EntityFrameworkCore;
using MiniBlogAPI.Models.Entities;
namespace MiniBlogAPI.Data;

public class BlogContext : DbContext
{
    /// Constructor nhận DbContextOptions từ DI (Dependency Injection).
    /// Options chứa connection string và database provider config.
    /// <param name="options">Configuration options (connection string, provider...)</param>
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
        // Constructor này được gọi bởi DI container
        // Options đã được configure trong Program.cs
    }
    // ==================== DbSets (Tables) ====================
    /// DbSet đại diện cho bảng Users trong database.
    /// Tên DbSet thường là số nhiều (Users), tên entity là số ít (User).
    /// EF Core sẽ tạo bảng tên "Users" trong database.
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    // ==================== Model Configuration ====================

    /// Method này được EF Core gọi khi tạo model.
    /// Dùng Fluent API để configure entities, relationships, constraints...
    /// /// <param name="modelBuilder">Builder để configure model</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ========== Configure User Entity ==========
        modelBuilder.Entity<User>(entity =>
        {
            // Đặt tên bảng (optional, mặc định là "Users")
            entity.ToTable("Users");
            // Configure Primary Key (optional - EF corre tự nhận diện "Id)
            entity.HasKey(u => u.Id);
            //Configure Properties (column types, constraints)
            entity.Property(u => u.Username)
            .IsRequired()   // NOT NULL
            .HasMaxLength(50); // VARCHAR (50)

            entity.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

            entity.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

            entity.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

            entity.Property(u => u.ProfileImagePath)
            .HasMaxLength(500);

            entity.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default value trong DB

            // config unique Contraints
            entity.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("IX_Users_Username");

            entity.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");
        });
        // ========== Configure Post Entity ==========
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

            entity.Property(p => p.Content)
            .IsRequired()
            .HasMaxLength(10000);

            entity.Property(p => p.ImagePath)
            .HasMaxLength(500);

            entity.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(p => p.UpdatedAt); // nullable

            // ========== Configure Relationship: Post -> User (Many-to-One) ==========
            entity.HasOne(p => p.User) // post có 1 user
            .WithMany(u => u.Posts) //user có nhiều posts
            .HasForeignKey(p => p.UserId) // foreign key là UserId
            .OnDelete(DeleteBehavior.Cascade) // xóa User -> xóa tất cả Posts
            .HasConstraintName("FK_Posts_Users"); // Tên constraint

            // create index on Foreign Key (performance)
            entity.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_Posts_UserId");

            // create index for search by title (performance)
            entity.HasIndex(p => p.Title)
            .HasDatabaseName("IX_Posts_Title");
        }
       );

        // ========== Configure Comment Entity ==========
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ========== Configure Relationship: Comment -> Post (Many-to-One) ==========
            entity.HasOne(c => c.Post)           // Comment có 1 Post
                .WithMany(p => p.Comments)       // Post có nhiều Comments
                .HasForeignKey(c => c.PostId)    // Foreign key là PostId
                .OnDelete(DeleteBehavior.Cascade) // Xóa Post → xóa tất cả Comments
                .HasConstraintName("FK_Comments_Posts");

            // ========== Configure Relationship: Comment -> User (Many-to-One) ==========
            entity.HasOne(c => c.User)           // Comment có 1 User
                .WithMany()                      // User có nhiều Comments (không cần navigation property)
                .HasForeignKey(c => c.UserId)    // Foreign key là UserId
                .OnDelete(DeleteBehavior.Restrict) // Xóa User → KHÔNG xóa Comments (giữ lại)
                .HasConstraintName("FK_Comments_Users");

            // Create indexes on Foreign Keys
            entity.HasIndex(c => c.PostId)
                .HasDatabaseName("IX_Comments_PostId");

            entity.HasIndex(c => c.UserId)
                .HasDatabaseName("IX_Comments_UserId");
        }
        );
    }
}

