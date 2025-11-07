using AutoMapper;
using Dapper;
using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Helper;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DotnetAPI.Controllers;


[ApiController]
[Route("[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly DataContextDapper _dapper;
    private readonly IConfiguration _config;
    private readonly AuthHelper authHelper;
    private readonly ReusableSQL _reusableSQL;
    private readonly IMapper _mapper;
    public AuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        authHelper = new AuthHelper(config);
        _reusableSQL = new ReusableSQL(config);
        _mapper = new Mapper(new MapperConfiguration(confg =>
        {
            confg.CreateMap<UserForRegistrationDTO, UserComplete>();
        }));
    }

    [AllowAnonymous]
    [HttpPost("Register")]

    public IActionResult Register(UserForRegistrationDTO user)
    {

        if (user.Password == user.PasswordConfirm)
        {
            string userExistsSql = $@"SELECT * FROM TutorialAppSchema.Auth
                          WHERE Email = '{user.Email}'";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(userExistsSql);
            if (existingUsers.Count() == 0)
            {

                UserForLoginDTO setPassword = new UserForLoginDTO()
                {
                    Email = user.Email,
                    Password = user.Password
                };

                if (authHelper.SetPassword(setPassword))
                {
                    UserComplete userComplete = _mapper.Map<UserComplete>(user);
                    userComplete.Active = true;
                    if (_reusableSQL.UpsertUser(userComplete))
                    {
                        return Ok();
                    }
                    throw new Exception("Unable to add user");
                }
                throw new Exception("Unable to register user");
            }
            throw new Exception("User with this email already exists");
        }
        throw new Exception("Password do not match");
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public IActionResult Login(UserForLoginDTO user)
    {
        string sql = $@"EXEC TutorialAppSchema.sp_User_Login 
                            @Email = @EmailParam";

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("@EmailParam", user.Email, DbType.String);

        UserForLoginConfirmationDTO userLogin = _dapper.LoadDataSingleWithParameters<UserForLoginConfirmationDTO>(sql, parameters);

        if (userLogin == null)
            return StatusCode(401, "User not found");

        byte[] passwordHash = authHelper.GetPasswordHash(user.Password, userLogin.PasswordSalt);
        
        if (passwordHash.Length != userLogin.PasswordHash.Length)
            return StatusCode(401, "Password length is incorrect");

        for (int i = 0; i < passwordHash.Length; i++)
        {
            if (passwordHash[i] != userLogin.PasswordHash[i])
            {
                return StatusCode(401, "Password is incorrect");
            }
        }

        string sqlUserId = $@"SELECT UserId From tutorialAppSchema.Users
                                WHERE Email = '{user.Email}'";
        int userId = _dapper.LoadDataSingle<int>(sqlUserId);
        return Ok( new Dictionary<string, string> {
            {"token", authHelper.CreateToken(userId)}
        });
    }

    //[Authorize]
    [AllowAnonymous]
    [HttpPut("ResetPassword")]
    public IActionResult ResetPassword(UserForLoginDTO resetPassword)
    {
        if (authHelper.SetPassword(resetPassword))
        {
            return Ok();
        }

        throw new Exception("Failed to reset password");
    }
}
