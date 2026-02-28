namespace NewsFeed.Entities
{
    public class Comments
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int NewsId { get; set; }
        public DateTime CreatedDate { get; set; }
        public News? News { get; set; }
    }
}
