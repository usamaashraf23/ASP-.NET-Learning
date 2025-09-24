using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    [HttpGet("GetUsers/{testValue}")]
    public string[] GetUser(string testValue)
    {
        string[] responseArray = new string[]{
            "test1","test2",testValue
        };
        return responseArray;
    }

}