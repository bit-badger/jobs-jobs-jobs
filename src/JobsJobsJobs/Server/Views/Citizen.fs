/// Views for URLs beginning with /citizen
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Citizen

open Giraffe.ViewEngine
open JobsJobsJobs.ViewModels

/// The log on page
let logOn (m : LogOnViewModel) =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Log On" ]
        match m.ErrorMessage with
        | Some msg ->
            p [ _class "pb-3 text-center" ] [
                span [ _class "text-danger" ] [ str msg ]; br []
                if msg.IndexOf("ocked") > -1 then
                    rawText "If this is a new account, it must be confirmed before it can be used; otherwise, you need "
                    rawText "to "
                    a [ _href "/citizen/forgot-password" ] [ rawText "request an unlock code" ]
                    rawText " before you may log on."
            ]
        | None -> ()
        form [ _class "row g-3 pb-3"] [
            div [ _class "col-12 col-md-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type        "email"
                            _id          "email"
                            _class       "form-control"
                            _name        (nameof m.Email)
                            _placeholder "E-mail Address"
                            _value       m.Email
                            _required
                            _autofocus ]
                    label [ _class "jjj-required"; _for "email" ] [ rawText "E-mail Address" ]
                ]
            ]
            div [ _class "col-12 col-md-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type        "password"
                            _id          "password"
                            _class       "form-control"
                            _name        (nameof m.Password)
                            _placeholder "Password"
                            _required ]
                    label [ _class "jjj-required"; _for "password" ] [ rawText "Password" ]
                ]
            ]
            div [ _class "col-12" ] [
                button [ _class "btn btn-primary"; _type "submit" ] [
                    i [ _class "mdi mdi-login" ] []; rawText "&nbsp; Log On"
                ]
            ]
        ]
        p [ _class "text-center" ] [
            rawText "Need an account? "; a [ _href "/citizen/register" ] [ rawText "Register for one!" ]
        ]
        p [ _class "text-center" ] [
            rawText "Forgot your password? "; a [ _href "/citizen/forgot-password" ] [ rawText "Request a reset." ]
        ]
    ]
