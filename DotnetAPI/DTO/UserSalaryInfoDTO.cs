namespace DotnetAPI.DTO
{
    public class UserSalaryInfoDTO
    {
        public int UserId { get; set; }
        public decimal Salary { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Gender { get; set; } = "";
        public bool Active { get; set; }
    }
}
