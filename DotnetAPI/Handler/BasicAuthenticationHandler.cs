using Azure.Core;
using DotnetAPI.Data1;
using DotnetAPI.DTO;
using DotnetAPI.Helper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace DotnetAPI.Handler
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        public BasicAuthenticationHandler(
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
            // check Authorization header
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                var email = credentials[0];
                var password = credentials[1];

                // ✅ Fetch stored hash and salt from DB
                string sql = $@"SELECT [PasswordHashed] AS PasswordHash,
                                   [PasswordSalt]
                            FROM TutorialAppSchema.Auth
                            WHERE Email = '{email}'";

                var userLogin = _dapper.LoadDataSingle<UserForLoginConfirmationDTO>(sql);

                if (userLogin == null)
                    return AuthenticateResult.Fail("Invalid Email");

                // ✅ Verify password
                byte[] passwordHash = _authHelper.GetPasswordHash(password, userLogin.PasswordSalt);

                if (passwordHash.Length != userLogin.PasswordHash.Length)
                    return AuthenticateResult.Fail("Invalid Password");

                for (int i = 0; i < passwordHash.Length; i++)
                {
                    if (passwordHash[i] != userLogin.PasswordHash[i])
                        return AuthenticateResult.Fail("Invalid Password");
                }

                // ✅ Create Claims
                var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Name, email)
            };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}
