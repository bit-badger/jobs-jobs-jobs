using JobsJobsJobs.Server.Models;
using JobsJobsJobs.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Server
{
    /// <summary>
    /// Authentication / authorization utility methods
    /// </summary>
    public static class Auth
    {
        /// <summary>
        /// Verify the authorization code with Mastodon and get the user's profile
        /// </summary>
        /// <param name="authCode">The code from the authorization flow</param>
        /// <param name="config">The authorization configuration section</param>
        /// <returns>The Mastodon account (or an error if one is encountered)</returns>
        public static async Task<Result<MastodonAccount>> VerifyWithMastodon(string authCode,
            IConfigurationSection config)
        {
            using var http = new HttpClient();

            // Use authorization code to get an access token from NAS
            using var codeResult = await http.PostAsJsonAsync("https://noagendasocial.com/oauth/token", new
            {
                client_id = config["ClientId"],
                client_secret = config["Secret"],
                redirect_uri = $"{config["ReturnHost"]}/citizen/authorized",
                grant_type = "authorization_code",
                code = authCode,
                scope = "read"
            });
            if (!codeResult.IsSuccessStatusCode)
            {
                Console.WriteLine($"ERR: {await codeResult.Content.ReadAsStringAsync()}");
                return Result<MastodonAccount>.AsError(
                    $"Could not get token ({codeResult.StatusCode:D}: {codeResult.ReasonPhrase})");
            }

            using var tokenResponse = JsonSerializer.Deserialize<JsonDocument>(
                new ReadOnlySpan<byte>(await codeResult.Content.ReadAsByteArrayAsync()));
            if (tokenResponse == null)
            {
                return Result<MastodonAccount>.AsError("Could not parse authorization code result");
            }

            var accessToken = tokenResponse.RootElement.GetProperty("access_token").GetString();

            // Use access token to get profile from NAS
            using var req = new HttpRequestMessage(HttpMethod.Get, $"{config["ApiUrl"]}accounts/verify_credentials");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var profileResult = await http.SendAsync(req);
            if (!profileResult.IsSuccessStatusCode)
            {
                return Result<MastodonAccount>.AsError(
                    $"Could not get profile ({profileResult.StatusCode:D}: {profileResult.ReasonPhrase})");
            }

            var profileResponse = JsonSerializer.Deserialize<MastodonAccount>(
                new ReadOnlySpan<byte>(await profileResult.Content.ReadAsByteArrayAsync()));
            if (profileResponse == null)
            {
                return Result<MastodonAccount>.AsError("Could not parse profile result");
            }

            if (profileResponse.Username != profileResponse.AccountName)
            {
                return Result<MastodonAccount>.AsError(
                    $"Profiles must be from noagendasocial.com; yours is {profileResponse.AccountName}");
            }

            // If the user hasn't filled in a display name, use the username as the display name.
            //   (this is what Mastodon does)
            if (string.IsNullOrWhiteSpace(profileResponse.DisplayName))
            {
                profileResponse.DisplayName = profileResponse.Username;
            }

            return Result<MastodonAccount>.AsOk(profileResponse);
        }

        /// <summary>
        /// Create a JSON Web Token for this citizen to use for further requests to this API
        /// </summary>
        /// <param name="citizen">The citizen for which the token should be generated</param>
        /// <param name="config">The authorization configuration section</param>
        /// <returns>The JWT</returns>
        public static string CreateJwt(Citizen citizen, IConfigurationSection config)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, citizen.Id.ToString()),
                    new Claim(ClaimTypes.Name, citizen.DisplayName),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = "https://noagendacareers.com",
                Audience = "https://noagendacareers.com",
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["ServerSecret"])),
                    SecurityAlgorithms.HmacSha256Signature)
            });
            return tokenHandler.WriteToken(token);
        }
    }
}
