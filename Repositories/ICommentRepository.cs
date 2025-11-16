using MiniBlogAPI.Models.Entities;
namespace MiniBlogAPI.Repositories
{
    public interface ICommentRepository : IRepository<Comment>
    {
        // lấy tất cả comment của một post
        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
        
        // lấy tất cả comment của một user
        Task<IEnumerable<Comment>> GetByUserIdAsync(int userId);

        //lấy comment với user và post info
        Task<Comment?> GetWithDetailsAsync(int commentId);
    }
}