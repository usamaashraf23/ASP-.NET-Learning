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
    public AuthController(IConfiguration config)
    {
        _dapper = new DataContextDapper(config);
        authHelper = new AuthHelper(config);
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
                byte[] passwordSalt = new byte[16];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetNonZeroBytes(passwordSalt);
                }
                //string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);
                byte[] passwordHash = authHelper.GetPasswordHash(user.Password, passwordSalt);

                string sql = $@"INSERT INTO TutorialAppSchema.Auth(Email, PasswordHashed, PasswordSalt)
                                VALUES('{user.Email}',@PasswordHashed, @PasswordSalt)";

                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                passwordSaltParameter.Value = passwordSalt;

                SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashed", SqlDbType.VarBinary);
                passwordHashParameter.Value = passwordHash;

                sqlParameters.Add(passwordSaltParameter);
                sqlParameters.Add(passwordHashParameter);

                if (_dapper.ExecuteWithParameters(sql, sqlParameters))
                {

                    string sqlAdd = $@"INSERT INTO TutorialAppSchema.Users (
                                      [FirstName]
                                    , [LastName]
                                    , [Email]
                                    , [Gender]
                                    ) VALUES('{user.FirstName}' 
                                        , '{user.LastName}' 
                                        , '{user.Email}'
                                        , '{user.Gender}' 
                                    )";
                    if (_dapper.Execute(sqlAdd))
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
        string sql = $@"SELECT [PasswordHashed] AS PasswordHash,
                            [PasswordSalt] FROM TutorialAppSchema.Auth
                            WHERE Email = '{user.Email}'";

        UserForLoginConfirmationDTO userLogin = _dapper.LoadDataSingle<UserForLoginConfirmationDTO>(sql);

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
}
