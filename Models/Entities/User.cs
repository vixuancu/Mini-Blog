namespace MiniBlogAPI.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? ProfileImagePath { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// NAVIGATION PROPERTY - Collection của Posts thuộc User này.
        /// Đây là phía "One" trong relationship One-to-Many (1 User → nhiều Posts).
        /// EF Core sẽ tự động tạo Foreign Key "UserId" trong bảng Posts.
        public ICollection<Post> Posts { get; set; } = new List<Post>();


    }
}