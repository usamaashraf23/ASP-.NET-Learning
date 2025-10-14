namespace DotnetAPI.DTO
{
    public class PostToUpdateDTO
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; } = string.Empty;
        public string PostDescription { get; set; } = string.Empty;
    }
}
