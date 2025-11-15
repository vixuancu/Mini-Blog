namespace MiniBlogAPI.Models.Entities
{
    public class Post
    {
        public int Id {get; set; }
        public string Title {get; set; } = string.Empty;
        public string Content {get; set; } = string.Empty;
        // Ảnh thumbnail cho bài viết (nếu có)
        public string? ImagePath {get; set; }
        public DateTime CreatedAt {get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt {get; set; }
        public int UserId {get; set; } // Foreign Key liên kết đến User
        /// NAVIGATION PROPERTY - Reference đến User sở hữu post này.
        /// Đây là phía "Many" trong relationship (nhiều Posts → 1 User).
        public User User {get; set; } = null!; 
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}