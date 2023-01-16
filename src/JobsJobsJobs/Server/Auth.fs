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
