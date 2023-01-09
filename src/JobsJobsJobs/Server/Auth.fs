/// Authorization / authentication functions
module JobsJobsJobs.Server.Auth

open System
open System.Text
open JobsJobsJobs.Domain

/// Create a confirmation or password reset token for a user
let createToken (citizen : Citizen) =
    Convert.ToBase64String (Guid.NewGuid().ToByteArray () |> Array.append (Encoding.UTF8.GetBytes citizen.Email))


/// Password hashing and verification
module Passwords =
    
    open Microsoft.AspNetCore.Identity

    /// The password hasher to use for the application
    let private hasher = PasswordHasher<Citizen> ()

    /// Hash a password for a user
    let hash citizen password =
        hasher.HashPassword (citizen, password)

    /// Verify a password (returns true if the password needs to be rehashed)
    let verify citizen password =
        match hasher.VerifyHashedPassword (citizen, citizen.PasswordHash, password) with
        | PasswordVerificationResult.Success -> Some false
        | PasswordVerificationResult.SuccessRehashNeeded -> Some true
        | _ -> None


open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open Microsoft.IdentityModel.Tokens
open JobsJobsJobs.Domain.SharedTypes

/// Create a JSON Web Token for this citizen to use for further requests to this API
let createJwt (citizen : Citizen) (cfg : AuthOptions) =

    let tokenHandler = JwtSecurityTokenHandler ()
    let token =
        tokenHandler.CreateToken (
            SecurityTokenDescriptor (
                Subject = ClaimsIdentity [|
                    Claim (ClaimTypes.NameIdentifier, CitizenId.toString citizen.Id)
                    Claim (ClaimTypes.Name, Citizen.name citizen)
                    |],
                Expires  = DateTime.UtcNow.AddHours 2.,
                Issuer   = "https://noagendacareers.com",
                Audience = "https://noagendacareers.com",
                SigningCredentials = SigningCredentials (
                    SymmetricSecurityKey (
                        Encoding.UTF8.GetBytes cfg.ServerSecret), SecurityAlgorithms.HmacSha256Signature)
            )
        )
    tokenHandler.WriteToken token

