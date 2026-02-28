using NewsFeed.Entities;

namespace NewsFeed.Interfaces
{
    public interface INewsFeedService
    {
        public Task<IEnumerable<News>> GetAllNewsAsync();
        public Task<News> GetNewsByIdAsync(int id);
        public Task<News> CreateNewsAsync(News news);
        public Task<bool> UpdateNewsAsync(int id, News news);
        public Task<bool> DeleteNewsAsync(int id);
        public Task<Comments> AddCommentAsync(int newsId, Comments comment);
        public Task<IEnumerable<News>> GetAllCommentsByNewsId(int newsId);
    }
}
