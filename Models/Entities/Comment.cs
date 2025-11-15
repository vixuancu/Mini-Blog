namespace MiniBlogAPI.Models.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        /// FOREIGN KEY - ID của Post mà comment này thuộc về.
        public int PostId {get; set; }
        /// NAVIGATION PROPERTY - Reference đến Post chứa comment này.
        public Post Post {get; set; } = null!;// bỏ qua cảnh null
        /// FOREIGN KEY - ID của User tạo comment này.
        public int UserId { get; set; }
        /// NAVIGATION PROPERTY - Reference đến User tạo comment này.
        public User User { get; set; } = null!;
    }
}