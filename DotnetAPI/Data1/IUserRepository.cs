using DotnetAPI.Models;

namespace DotnetAPI.Data1
{
    public interface IUserRepository
    {
        public bool SaveChanges();
        public void AddEntity<T>(T entityToAdd);
        public void RemoveEntity<T>(T entityToRemove);
        public IEnumerable<User> GetUsers();
        public IEnumerable<UserSalary> GetUsersSalary();
        public User GetSingleUser(int id);
        public UserSalary GetSingleUserSalary(int id);

    }
}
