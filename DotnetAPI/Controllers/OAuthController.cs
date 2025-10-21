using DotnetAPI.Data1;
using DotnetAPI.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DotnetAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class OAuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly AuthHelper _authHelper;
    private readonly IConfiguration _config;

    public OAuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        _authHelper = new AuthHelper(config);
        _config = config;
    }

    [HttpGet("Login")]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties { RedirectUri="/"},"oidc");
    }

    [AllowAnonymous]
    [HttpGet("Login/{provider}")]
    public IActionResult Login(string provider)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = "https://localhost:5001/OAuth/Callback"
        };
        return Challenge(properties, provider);
    }

    [AllowAnonymous]
    [HttpGet("Callback")]
    public async Task<IActionResult> Callback([FromQuery] string? returnUrl = null)
    {
        // Authenticate the user via cookie (from the Google middleware)
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!result.Succeeded || result.Principal == null)
        {
            return BadRequest("External authentication failed");
        }

        // Extract email and name claims
        var claims = result.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                    ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                    ?? claims.FirstOrDefault(c => c.Type == "name")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email not provided by external provider");
        }

        // Check if user exists in your DB
        string findSql = $@"SELECT UserId FROM TutorialAppSchema.Users WHERE Email = '{email}'";
        string userExists = _dapper.LoadDataSingle<string>(findSql);
        int userId;

        var first = name?.Split(' ').FirstOrDefault() ?? email;
        var last = name?.Split(' ').Skip(1).FirstOrDefault() ?? "";

        if (userExists == null)
        {
            string sqlAddUser = $@"
            INSERT INTO TutorialAppSchema.Users(FirstName, LastName, Email)
            VALUES('{first}','{last}', '{email}')";
            if (_dapper.Execute(sqlAddUser))
            {
                return Ok();
            }
            throw new Exception("Failed to register user.");
        }

        userId = _dapper.LoadDataSingle<int>(findSql);

        //if (userExists == null)
        //{
        //    try
        //    {
        //        userId = _dapper.LoadDataSingle<int>(findSql);
        //    }
        //    catch
        //    {
        //        var first = name?.Split(' ').FirstOrDefault() ?? email;
        //        var last = name?.Split(' ').Skip(1).FirstOrDefault() ?? "";
        //        // Create new user
        //        string insertSql = $@"
        //    INSERT INTO TutorialAppSchema.Users (FirstName, LastName, Email)
        //    VALUES ('{first}','{last}', '{email}');
        //    SELECT CAST(SCOPE_IDENTITY() as int);
        //";

        //        userId = _dapper.LoadDataSingle<int>(insertSql);
        //    }
        //}

        // Optionally store a record in Auth table if you want linking later

        // Generate your JWT
        string jwt = _authHelper.CreateToken(userId);

            // Clean up the cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Return JWT to client
        return Ok(new { token = jwt});
    }
}
