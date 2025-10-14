namespace DotnetAPI.Models
{
    public class Post
    {
        public int PostId { get; set; }
        public int UserId { get;set; }
        public string PostTitle { get; set; } = string.Empty;
        public string PostDescription { get; set; } = string.Empty;
        public DateTime PostCreated { get; set; }
        public DateTime PostUpdated { get; set; }
    }
}
