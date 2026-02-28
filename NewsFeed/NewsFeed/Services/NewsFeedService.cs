using NewsFeed.Data;
using NewsFeed.Entities;
using NewsFeed.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace NewsFeed.Services
{
    public class NewsFeedService : INewsFeedService
    {
        private readonly NewsDbContext _context;
        public NewsFeedService(NewsDbContext context)
        {
            _context = context;
        }
        public async Task<Comments> AddCommentAsync(int newsId, Comments comment)
        {
            News? news = await GetNewsByIdAsync(newsId);
            if (news is News)
            {
                await _context.Comments.AddAsync(comment);
                await _context.SaveChangesAsync();
                return comment;
            }
            else
            {
                return null;
            }
        }

        public async Task<News> CreateNewsAsync(News news)
        {
            await _context.News.AddAsync(news);
            await _context.SaveChangesAsync();
            return news;
        }

        public async Task<bool> DeleteNewsAsync(int id)
        {
            News? news = await GetNewsByIdAsync(id);
            if (news is News)
            {
                var comments = _context.Comments.Where(x => x.NewsId == news.Id);
                if (await comments.AnyAsync())
                {
                    _context.Comments.RemoveRange(comments);
                }
                _context.Remove(news);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<News>> GetAllCommentsByNewsId(int newsId)
        {
            News? news = await GetNewsByIdAsync(newsId);
            if (news is News)
            {
                return await _context.News.Where(x => x.Id == newsId).Include(x => x.Comments).AsNoTracking().ToListAsync();
            }
            else
            {
                return new List<News>();
            }
        }

        public async Task<IEnumerable<News>> GetAllNewsAsync()
        {
            return await _context.News.AsNoTracking().ToListAsync();
        }

        public async Task<News> GetNewsByIdAsync(int id)
        {
            return await _context.News.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> UpdateNewsAsync(int id, News news)
        {
            News? record = await GetNewsByIdAsync(id);
            if (record is News)
            {
                record.Title = news.Title;
                record.AuthorName = news.AuthorName;
                record.Body = news.Body;

                foreach (Comments comment in news.Comments)
                {
                    Comments? commentRecord = await _context.Comments.FirstOrDefaultAsync(x => x.Id == comment.Id);
                    if (commentRecord is Comments)
                    {
                        commentRecord.Name = comment.Name;
                        commentRecord.Content = comment.Content;
                    }
                    else
                    {
                        await _context.Comments.AddAsync(comment);
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
