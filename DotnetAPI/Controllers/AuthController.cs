using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace DotnetAPI.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly IConfiguration _config;
        public AuthController(IConfiguration congfig)
        {
            _dapper = new DataContextDapper(congfig);
            _config = congfig;
        }

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
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }
                    //string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);
                    byte[] passwordHash = GetPasswordHash(user.Password, passwordSalt);

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
                        return Ok();
                    }
                    throw new Exception("Unable to add user");
                }
                throw new Exception("User with this email already exists");
            }
            throw new Exception("Password do not match");
        }

        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDTO user)
        {
            string sql = $@"SELECT [PasswordHashed] AS PasswordHash,
                            [PasswordSalt] FROM TutorialAppSchema.Auth
                            WHERE Email = '{user.Email}'";

            UserForLoginConfirmationDTO userLogin = _dapper.LoadDataSingle<UserForLoginConfirmationDTO>(sql);

            if (userLogin == null)
                return StatusCode(401, "User not found");

            byte[] passwordHash = GetPasswordHash(user.Password, userLogin.PasswordSalt);
            
            if (passwordHash.Length != userLogin.PasswordHash.Length)
                return StatusCode(401, "Password length is incorrect");

            for (int i = 0; i < passwordHash.Length; i++)
            {
                if (passwordHash[i] != userLogin.PasswordHash[i])
                {
                    return StatusCode(401, "Password is incorrect");
                }
            }
            return Ok();
        }

        private byte[] GetPasswordHash(string password, byte[] passwordSalt)
        {
            string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value +
                Convert.ToBase64String(passwordSalt);
            return KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.ASCII.GetBytes(passwordSaltPlusString),
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
            );
        }
    }
}
