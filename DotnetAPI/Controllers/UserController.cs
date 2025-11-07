using Dapper;
using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Helper;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    DataContextDapper _dapper;
    private readonly ReusableSQL _resuableSQL;
    public UserController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _resuableSQL = new ReusableSQL(config);
    }


    [HttpGet("GetUsers/{userId}/{isActive}")]
    public IEnumerable<UserComplete> GetUsers(int userId, bool isActive)
    {
        string sql = @"EXEC TutorialAppSchema.spUser_Get";
        string parameters = "";

        DynamicParameters sqlParameters = new DynamicParameters();

        if(userId != 0)
        {
            sqlParameters.Add("@UserIdParam", userId, DbType.Int32);
            parameters += $", @UserId=@UserIdParam";
        }
        if(isActive)
        {
            sqlParameters.Add("@IsActiveParam", isActive, DbType.Boolean);
            parameters += $", @Active=@IsActiveParam";
        }
        if (parameters.Length > 0)
        {
            sql += parameters.Substring(1);
        }

        IEnumerable<UserComplete> users = _dapper.LoadDataWithParameters<UserComplete>(sql, sqlParameters);
        return users;
    }


    [HttpPost("UpsertUser")]

    public IActionResult UpsertUser(UserComplete user) {

        if (_resuableSQL.UpsertUser(user))
        {
            return Ok();
        }
        throw new Exception("Failed to Update User");
    }

    [HttpDelete("DeleteUser/{id}")]

    public IActionResult DeleteUser(int id)
    {
        DynamicParameters sqlParameter = new DynamicParameters();
        sqlParameter.Add("@UserIdParam", id, DbType.Int16);
        string sql = $@"EXEC TutorialAppSchema.spUser_Delete
                        @UserId = @UserIdParam";

        if (_dapper.Execute(sql))
        {
            return Ok();
        }

        throw new Exception("Unable to Delete User");
    }
}