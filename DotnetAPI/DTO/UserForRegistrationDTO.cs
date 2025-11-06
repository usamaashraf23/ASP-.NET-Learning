namespace DotnetAPI.DTO
{
    public partial class UserForRegistrationDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Password { get; set; } = "";
        public string PasswordConfirm { get; set; } = "";
        public bool Active { get; set; }
        public decimal Salary { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Department { get; set; } = "";
    }
}
