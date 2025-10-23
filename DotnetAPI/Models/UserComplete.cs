namespace DotnetAPI.Models
{
    public class UserComplete
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Gender { get; set; } = "";
        public bool Active { get; set; }
        public decimal Salary { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Department { get; set; } = "";
        public decimal AvgSalary { get; set; }

    }
}
