/// Authorization / authentication functions
module JobsJobsJobs.Api.Auth

open System.Text.Json.Serialization

/// The variables we need from the account information we get from Mastodon
[<NoComparison; NoEquality; AllowNullLiteral>]
type MastodonAccount () =
  /// The user name (what we store as mastodonUser)
  [<JsonPropertyName "username">]
  member val Username = "" with get, set
  /// The account name; will generally be the same as username for local accounts, which is all we can verify
  [<JsonPropertyName "acct">]
  member val AccountName = "" with get, set
  /// The user's display name as it currently shows on Mastodon
  [<JsonPropertyName "display_name">]
  member val DisplayName = "" with get, set
  /// The user's profile URL
  [<JsonPropertyName "url">]
  member val Url = "" with get, set


open Microsoft.Extensions.Logging
open System
open System.Net.Http
open System.Net.Http.Headers
open System.Net.Http.Json
open System.Text.Json
open JobsJobsJobs.Domain.SharedTypes

/// HTTP client to use to communication with Mastodon
let private http = new HttpClient()

/// Verify the authorization code with Mastodon and get the user's profile
let verifyWithMastodon (authCode : string) (inst : MastodonInstance) rtnHost (log : ILogger) = task {

  // Function to create a URL for the given instance
  let apiUrl = sprintf "%s/api/v1/%s" inst.Url

  // Use authorization code to get an access token from Mastodon
  use! codeResult =
    http.PostAsJsonAsync($"{inst.Url}/oauth/token",
      {|  client_id     = inst.ClientId
          client_secret = inst.Secret
          redirect_uri  = $"{rtnHost}/citizen/{inst.Abbr}/authorized"
          grant_type    = "authorization_code"
          code          = authCode
          scope         = "read"
        |})
  match codeResult.IsSuccessStatusCode with
  | true ->
      let! responseBytes = codeResult.Content.ReadAsByteArrayAsync ()
      use  tokenResponse = JsonSerializer.Deserialize<JsonDocument> (ReadOnlySpan<byte> responseBytes)
      match tokenResponse with
      | null -> return Error "Could not parse authorization code result"
      | _ ->
          // Use access token to get profile from NAS
          use req = new HttpRequestMessage (HttpMethod.Get, apiUrl "accounts/verify_credentials")
          req.Headers.Authorization <- AuthenticationHeaderValue
            ("Bearer", tokenResponse.RootElement.GetProperty("access_token").GetString ())
          use! profileResult = http.SendAsync req
          
          match profileResult.IsSuccessStatusCode with
          | true ->
              let! profileBytes = profileResult.Content.ReadAsByteArrayAsync ()
              match JsonSerializer.Deserialize<MastodonAccount>(ReadOnlySpan<byte> profileBytes) with
              | null -> return Error "Could not parse profile result"
              | profile -> return Ok profile
          | false -> return Error $"Could not get profile ({profileResult.StatusCode:D}: {profileResult.ReasonPhrase})"
  | false ->
      let! err = codeResult.Content.ReadAsStringAsync ()
      log.LogError $"Could not get token result from Mastodon:\n  {err}"
      return Error $"Could not get token ({codeResult.StatusCode:D}: {codeResult.ReasonPhrase})"
  }


open JobsJobsJobs.Domain
open JobsJobsJobs.Domain.Types
open Microsoft.IdentityModel.Tokens
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text

/// Create a JSON Web Token for this citizen to use for further requests to this API
let createJwt (citizen : Citizen) (cfg : AuthOptions) =

  let tokenHandler = JwtSecurityTokenHandler ()
  let token =
    tokenHandler.CreateToken (
      SecurityTokenDescriptor (
        Subject = ClaimsIdentity [|
          Claim (ClaimTypes.NameIdentifier, CitizenId.toString citizen.id)
          Claim (ClaimTypes.Name, Citizen.name citizen)
          |],
        Expires  = DateTime.UtcNow.AddHours 2.,
        Issuer   = "https://noagendacareers.com",
        Audience = "https://noagendacareers.com",
        SigningCredentials = SigningCredentials (
          SymmetricSecurityKey (Encoding.UTF8.GetBytes cfg.ServerSecret), SecurityAlgorithms.HmacSha256Signature)
        )
      )
  tokenHandler.WriteToken token

