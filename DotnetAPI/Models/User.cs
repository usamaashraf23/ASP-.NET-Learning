namespace DotnetAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Active { get; set; }
    }
}

