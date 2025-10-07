using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
    }


    [HttpGet("GetAllUsers")]
    public IEnumerable<User> GetAllUsers()
    {
        string sql = @"
         SELECT [UserId],
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        FROM  TutorialAppSchema.Users";
        IEnumerable<User> users = _dapper.LoadData<User>(sql);
        return users;
    }

    [HttpGet("GetSingleUser/{id}")]

    public User GetSingleUser(int id)
    {
        string sql = @"SELECT [UserId],
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active]
        FROM  TutorialAppSchema.Users
        WHERE UserId = " + id.ToString();
        User user = _dapper.LoadDataSingle<User>(sql);
        return user;
    }

    [HttpPost]

    public IActionResult AddUser(UserToAddDTO user)
    {
        string sql = @"INSERT INTO TutorialAppSchema.Users (
          [FirstName]
        , [LastName]
        , [Email]
        , [Gender]
        , [Active]
        ) VALUES('" + user.FirstName +
            "','" + user.LastName +
            "','" + user.Email +
            "','" + user.Gender + 
            "','" + user.Active +
        "')";

        if (_dapper.Execute(sql))
        {
            return Ok();
        }

        throw new Exception("Failed to Add User");
    }

    [HttpPut("EditUser")]

    public IActionResult EditUser(User user) {
        string sql = @"
            UPDATE TutorialAppSchema.Users
                SET  [FirstName] = '" + user.FirstName + 
                "',[LastName] = '" + user.LastName +
                "',[Email] = '" + user.Email +
                "',[Gender] = '" + user.Gender +
                "',[Active] = '" + user.Active + 
                "' WHERE UserId = " + user.UserId;
        if (_dapper.Execute(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update User");
    }

    [HttpDelete("DeleteUser/{UserId}")]

    public IActionResult DeleteUser(int UserId)
    {
        string sql = @"
            DELETE FROM TutorialAppSchema.Users
            WHERE UserId = " + UserId;

        if (_dapper.Execute(sql))
        {
            return Ok();
        }

        throw new Exception("Unable to Delete User");
    }

    [HttpGet("GetUsersSalaryInfo")]

    public IEnumerable<UserSalaryInfoDTO> GetUsersSalaryInfo()
    {
        string sql = @"SELECT * FROM TutorialAppSchema.UserSalary 
                        AS US INNER JOIN TutorialAppSchema.Users AS U
                        ON US.UserId = U.UserId ;";

        IEnumerable<UserSalaryInfoDTO> usersSalary = _dapper.LoadData<UserSalaryInfoDTO>(sql);
        return usersSalary; 
    }
}