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


    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Get";
        string parameters = "";

        if(userId != 0)
        {
            parameters += $", @UserId={userId}";
        }
        if(isActive)
        {
            parameters += $", @Active={isActive}";
        }

        IEnumerable<UserComplete> users = _dapper.LoadData<UserComplete>(sql);
        return users;
    }


    [HttpPost("UpsertUser")]

    public IActionResult UpsertUser(UserComplete user) {
        string sql = $@"EXEC TutorialAppSchema.spUser_Upsert
                         @FirstName = '{user.FirstName}', 
                         @LastName = '{user.LastName}', 
                         @Email = '{user.Email}', 
                         @Gender = '{user.Gender}', 
                         @Active = {user.Active},
                         @Salary = {user.Salary},
                         @Department = '{user.Department}',
                         @JobTitle = '{user.JobTitle}',
                         @UserId = {user.UserId}";
        Console.WriteLine(sql);
        if (_dapper.Execute(sql))
        {
            return Ok();
        }
        throw new Exception("Failed to Update User");
    }

    [HttpDelete("DeleteUser/{id}")]

    public IActionResult DeleteUser(int id)
    {
        string sql = $@"EXEC TutorialAppSchema.spUser_Delete
                        @UserId = {id}";

        if (_dapper.Execute(sql))
        {
            return Ok();
        }

        throw new Exception("Unable to Delete User");
    }
}