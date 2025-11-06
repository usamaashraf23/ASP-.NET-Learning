using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DotnetAPI.Helper
{
    public class AuthHelper
    {
        private readonly IConfiguration _config;

        private readonly DataContextDapper _dapper;
        public AuthHelper(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _config = config;
        }

        public byte[] GetPasswordHash(string password, byte[] passwordSalt)
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

        public string CreateToken(int userId)
        {
            Claim[] claims = new Claim[]
            {
            new Claim("userId", userId.ToString())
            };

            SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _config.GetSection("AppSettings:TokenKey").Value
                    )
                );

            SigningCredentials credentials = new SigningCredentials(
                    tokenKey,
                    SecurityAlgorithms.HmacSha512Signature
             );


            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = credentials,
                Expires = DateTime.Now.AddDays(1)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken token = tokenHandler.CreateToken(descriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool SetPassword(UserForLoginDTO setPassword)
        {
            byte[] passwordSalt = new byte[16];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }
            //string passwordSaltPlusString = _config.GetSection("AppSettings:PasswordKey").Value + Convert.ToBase64String(passwordSalt);
            byte[] passwordHash = GetPasswordHash(setPassword.Password, passwordSalt);

            string sql = $@"EXEC TutorialAppSchema.spRegistration_Upsert
                                @Email = @EmailParameter,
                                @PasswordHash = @PasswordHashParameter, 
                                @PasswordSalt = @PasswordSaltParameter";

            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            SqlParameter emailParameter = new SqlParameter("@EmailParameter", SqlDbType.VarChar);
            emailParameter.Value = setPassword.Email;

            SqlParameter passwordHashParameter = new SqlParameter("@PasswordHashParameter", SqlDbType.VarBinary);
            passwordHashParameter.Value = passwordHash;

            SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSaltParameter", SqlDbType.VarBinary);
            passwordSaltParameter.Value = passwordSalt;

            sqlParameters.Add(emailParameter);
            sqlParameters.Add(passwordSaltParameter);
            sqlParameters.Add(passwordHashParameter);

            return _dapper.ExecuteWithParameters(sql, sqlParameters);
        }
    }
}
