module JobsJobsJobs.Email

open System.Net
open JobsJobsJobs.Domain
open MailKit.Net.Smtp
open MailKit.Security
open MimeKit

/// Private functions for sending e-mail
[<AutoOpen>]
module private Helpers =
    
    /// Create an SMTP client
    let smtpClient () = backgroundTask {
        let client = new SmtpClient ()
        do! client.ConnectAsync ("localhost", 25, SecureSocketOptions.None)
        return client
    }

    /// Create a message with to, from, and subject completed
    let createMessage citizen subject =
        let msg = new MimeMessage ()
        msg.From.Add (MailboxAddress ("Jobs, Jobs, Jobs", (* "daniel@bitbadger.solutions" *) "summersd@localhost"))
        msg.To.Add (MailboxAddress (Citizen.name citizen, (* citizen.Email *) "summersd@localhost"))
        msg.Subject <- subject
        msg


/// Send an account confirmation e-mail
let sendAccountConfirmation citizen security = backgroundTask {
    let  token  = WebUtility.UrlEncode security.Token.Value
    use! client = smtpClient ()
    use  msg    = createMessage citizen "Account Confirmation Request"
    
    let text =
        [   $"ITM, {Citizen.name citizen}!"
            ""
            "This e-mail address was recently used to establish an account on"
            "Jobs, Jobs, Jobs (noagendacareers.com). Before this account can be"
            "used, it needs to be verified. Please click the link below to do so;"
            "it will work for the next 72 hours (3 days)."
            ""
            $"https://noagendacareers.com/citizen/confirm/{token}"
            ""
            "If you did not take this action, you can do nothing, and the account"
            "will be deleted at the end of that time. If you wish to delete it"
            "immediately, use the link below (also valid for 72 hours)."
            ""
            $"https://noagendacareers.com/citizen/deny/{token}"
            ""
            "TYFYC!"
            ""
            "--"
            "Jobs, Jobs, Jobs"
            "https://noagendacareers.com"
        ] |> String.concat "\n"
    use msgText = new TextPart (Text = text)
    msg.Body <- msgText
    
    return! client.SendAsync msg
}

/// Send a password reset link
let sendPasswordReset citizen security = backgroundTask {
    let  token  = WebUtility.UrlEncode security.Token.Value
    use! client = smtpClient ()
    use  msg    = createMessage citizen "Reset Password for Jobs, Jobs, Jobs"
    
    let text =
        [   $"ITM, {Citizen.name citizen}!"
            ""
            "We recently receive a request to reset the password for your account"
            "on Jobs, Jobs, Jobs (noagendacareers.com). Use the link below to"
            "do so; it will work for the next 72 hours (3 days)."
            ""
            $"https://noagendacareers.com/citizen/reset-password/{token}"
            ""
            "If you did not take this action, you can do nothing, and the link"
            "will expire normally. If you wish to expire the token immediately,"
            "use the link below (also valid for 72 hours)."
            ""
            $"https://noagendacareers.com/citizen/cancel-reset/{token}"
            ""
            "TYFYC!"
            ""
            "--"
            "Jobs, Jobs, Jobs"
            "https://noagendacareers.com"
        ] |> String.concat "\n"
    use msgText = new TextPart (Text = text)
    msg.Body <- msgText
    
    return! client.SendAsync msg
}
