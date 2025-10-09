using AutoMapper;
using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserEFController : ControllerBase
{
    IUserRepository _userRepository;
    Mapper _mapper;
    public UserEFController(IConfiguration config, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _mapper = new Mapper( new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<UserToAddDTO, User>();
        }) );
    }


    [HttpGet("GetAllUsers")]
    public IEnumerable<User> GetAllUsers()
    {
        IEnumerable<User> users = _userRepository.GetUsers();
        return users;
    }

    [HttpGet("GetSingleUser/{id}")]

    public User GetSingleUser(int id)
    {
        
        return _userRepository.GetSingleUser(id);
    }

    [HttpPost]

    public IActionResult AddUser(UserToAddDTO user)
    {
        User userDB = _mapper.Map<User>(user);
        _userRepository.AddEntity<User>(userDB);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpPut("EditUser")]

    public IActionResult EditUser(User user) 
    {
        User? userDb = _userRepository.GetSingleUser(user.UserId);

        if (userDb != null)
        {
            userDb.Active = user.Active;
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender;

            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
            throw new Exception("Failed to Update User");
        }
        return NotFound("User not found");
    }

    [HttpDelete("DeleteUser/{id}")]

    public IActionResult DeleteUser(int id)
    {
        User? user = _userRepository.GetSingleUser(id);
        if (user != null)
        {
            _userRepository.RemoveEntity(user);
            if(_userRepository.SaveChanges()) { 
                return Ok(); 
            }
        }

        throw new Exception("Unable to Delete User");
    }

    [HttpGet("GetUsersSalaryInfo")]

    public IEnumerable<UserSalary> GetUsersSalaryInfo()
    {
        IEnumerable<UserSalary> usersSalary = _userRepository.GetUsersSalary();
        return usersSalary;
         
    }

    [HttpGet("GetUserSalaryInfo/{id}")]
    public UserSalary GetUserSalaryInfo(int id)
    {

        return _userRepository.GetSingleUserSalary(id);

    }

    [HttpPost("AddUserSalary")]
    public IActionResult AddUserSalary(UserSalary userSalary)
    {
        
        _userRepository.AddEntity<UserSalary>(userSalary);
        if (_userRepository.SaveChanges())
        {
            return Ok();
        }

        throw new Exception("Unable to add salary for user");
    }

    [HttpPut("UpdateUserSalary")]

    public IActionResult UpdateUserSalary(UserSalary userSalary)
    {
        UserSalary? userSalaryDb = _userRepository.GetSingleUserSalary(userSalary.UserId);
        if (userSalaryDb != null) {
            userSalaryDb.Salary = userSalary.Salary;
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
        }

        throw new Exception("Unable to update user salary");
    }

    [HttpDelete("RemoveUserSalary/{id}")]
    public IActionResult RemoveUserSalary(int id)
    {
        UserSalary? userSalaryDb = _userRepository.GetSingleUserSalary(id);
        if(userSalaryDb != null)
        {
            _userRepository.RemoveEntity<UserSalary>(userSalaryDb);
            if (_userRepository.SaveChanges())
            {
                return Ok();
            }
        }
        throw new Exception("Unable to delete user salary");
    }
}