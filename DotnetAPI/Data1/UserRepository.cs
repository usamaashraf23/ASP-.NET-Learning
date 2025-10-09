using DotnetAPI.Models;
using System.Linq;

namespace DotnetAPI.Data1
{
    public class UserRepository : IUserRepository
    {
        DataContextEF _entityFramework;

        public UserRepository(IConfiguration config)
        {
            _entityFramework = new DataContextEF(config);
        }

        public bool SaveChanges()
        {
            return _entityFramework.SaveChanges() > 0;
        }

        public void AddEntity<T>(T entityToAdd)
        {
            if(entityToAdd != null)
            {
                _entityFramework.Add(entityToAdd);
            }
        }

        public void RemoveEntity<T>(T entityToRemove)
        {
            if(entityToRemove != null)
            {
                _entityFramework.Remove(entityToRemove);
            }
        }

        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _entityFramework.Users.ToList<User>();
            return users;
        }
        public IEnumerable<UserSalary> GetUsersSalary()
        {
            IEnumerable<UserSalary> usersSalary = _entityFramework.UserSalary.ToList<UserSalary>();
            return usersSalary;
        }

        public User GetSingleUser(int id)
        {
            User? user = _entityFramework.Users
                        .Where( u => u.UserId == id)
                        .FirstOrDefault<User>();

            if(user != null)
            {
                return user;
            }
            throw new Exception("No user found");
        }

        public UserSalary GetSingleUserSalary(int id)
        {
            UserSalary? userSalary = _entityFramework.UserSalary
                                    .Where( u => u.UserId == id)
                                    .FirstOrDefault<UserSalary>();
            if(userSalary != null)
            {
                return userSalary;
            }
            throw new Exception("No Salary found for user");
        }
    }
}
