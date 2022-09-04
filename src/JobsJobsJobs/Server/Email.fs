module JobsJobsJobs.Api.Email

open System.Net
open JobsJobsJobs.Domain
open MailKit.Net.Smtp
open MailKit.Security
open MimeKit

/// Send an account confirmation e-mail
let sendAccountConfirmation citizen security = backgroundTask {
    let name   = Citizen.name citizen
    let token  = WebUtility.UrlEncode security.Token.Value
    use client = new SmtpClient ()
    do! client.ConnectAsync ("localhost", 25, SecureSocketOptions.None)
    
    use msg = new MimeMessage ()
    msg.From.Add (MailboxAddress ("Jobs, Jobs, Jobs", "daniel@bitbadger.solutions" (* "summersd@localhost" *) ))
    msg.To.Add (MailboxAddress (name, citizen.Email (* "summersd@localhost" *) ))
    msg.Subject <- "Account Confirmation Request"
    
    let text =
        [   $"ITM, {name}!"
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
