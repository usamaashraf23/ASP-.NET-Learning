using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DotnetAPI.Handler
{
    public class BasicAuthenticationHandler2 : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        public BasicAuthenticationHandler2(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration config) : base(options, logger, encoder, clock)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeaders = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialsByte = Convert.FromBase64String(authHeaders.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialsByte).Split(":", 2);
                var email = credentials[0];
                var password = credentials[1];


                string sql = $@"SELECT [PasswordHashed] AS PasswordHash,
                                [PasswordSalt] FROM TutorialAppSchema.Auth
                                Where Email = '{email}'";

                string sqlUserId = $@"SELECT UserId FROM TutorialAppSchema.Users
                                    Where Email = '{email}'";

                int userId = _dapper.LoadDataSingle<int>(sqlUserId);

                var userLogin = _dapper.LoadDataSingle<UserForLoginConfirmationDTO>(sql);

                byte[] passwordHash = _authHelper.GetPasswordHash(password, userLogin.PasswordSalt);

                if (passwordHash.Length != userLogin.PasswordHash.Length)
                {
                    return AuthenticateResult.Fail("Password Length is incorrect");
                }

                for(int i = 0; i < passwordHash.Length; i++)
                {
                    if (passwordHash[i] != userLogin.PasswordHash[i])
                    {
                        return AuthenticateResult.Fail("Password is incorrect");
                    }
                }

                if(userLogin == null)
                {
                    return AuthenticateResult.Fail("Email is incorrect");
                }

                var claim = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier,email),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("userId",userId.ToString())
                };

                var identity = new ClaimsIdentity(claim, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var token = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(token);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header"); 
            }
        }
    }
}
