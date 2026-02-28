namespace NewsFeed.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string Body { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Comments> Comments { get; set; } = new List<Comments>();

    }
}
