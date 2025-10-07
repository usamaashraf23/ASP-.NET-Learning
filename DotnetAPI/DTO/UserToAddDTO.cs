namespace DotnetAPI.DTO
{
    public class UserToAddDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Gender { get; set; } = "";
        public bool Active { get; set; }
    }
}
