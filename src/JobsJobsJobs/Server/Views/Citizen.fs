/// Views for URLs beginning with /citizen
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Citizen

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
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
        form [ _class "row g-3 pb-3"; _hxPost "/citizen/log-on" ] [
            div [ _class "col-12 col-md-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type        "email"
                            _class       "form-control"
                            _id          (nameof m.Email)
                            _name        (nameof m.Email)
                            _placeholder "E-mail Address"
                            _value       m.Email
                            _required
                            _autofocus ]
                    label [ _class "jjj-required"; _for (nameof m.Email) ] [ rawText "E-mail Address" ]
                ]
            ]
            div [ _class "col-12 col-md-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type        "password"
                            _class       "form-control"
                            _id          (nameof m.Password)
                            _name        (nameof m.Password)
                            _placeholder "Password"
                            _required ]
                    label [ _class "jjj-required"; _for (nameof m.Password) ] [ rawText "Password" ]
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

/// The registration page
let register q1 q2 (m : RegisterViewModel) =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Register" ]
        form [ _class  "row g-3"; _hxPost "/citizen/register" ] [
            div [ _class "col-6 col-xl-4" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _class "form-control"; _id (nameof m.FirstName); _name (nameof m.FirstName)
                            _value m.FirstName; _placeholder "First Name"; _required; _autofocus ]
                    label [ _class "jjj-required"; _for (nameof m.FirstName) ] [ rawText "First Name" ]
                ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _class "form-control"; _id (nameof m.LastName); _name (nameof m.LastName)
                            _value m.LastName; _placeholder "Last Name"; _required ]
                    label [ _class "jjj-required"; _for (nameof m.LastName) ] [ rawText "Last Name" ]
                ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _class "form-control"; _id (nameof m.DisplayName)
                            _name (nameof m.DisplayName); _value (defaultArg m.DisplayName "")
                            _placeholder "Display Name" ]
                    label [ _for (nameof m.DisplayName) ] [ rawText "Display Name" ]
                    div [ _class "form-text" ] [ em [] [ rawText "Optional; overrides first/last for display" ] ]
                ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                div [ _class "form-floating" ] [
                    input [ _type "email"; _class "form-control"; _id (nameof m.Email); _name (nameof m.Email)
                            _value m.Email; _placeholder "E-mail Address"; _required ]
                    label [ _class "jjj-required"; _for (nameof m.Email) ] [ rawText "E-mail Address" ]
                ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                div [ _class "form-floating" ] [
                    input [ _type "password"; _class "form-control"; _id (nameof m.Password); _name (nameof m.Password)
                            _placeholder "Password"; _minlength "8"; _required ]
                    label [ _class "jjj-required"; _for (nameof m.Password) ] [ rawText "Password" ]
                ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                div [ _class "form-floating" ] [
                    input [ _type "password"; _class "form-control"; _id "ConfirmPassword"
                            _placeholder "Confirm Password"; _minlength "8"; _required ]
                    label [ _class "jjj-required"; _for "ConfirmPassword" ] [ rawText "Confirm Password" ]
                ]
            ]
            div [ _class "col-12" ] [
                hr []
                p [ _class "mb-0 text-muted fst-italic" ] [
                    rawText "Before your account request is through, you must answer these questions two&hellip;"
                ]
            ]
            div [ _class "col-12 col-xl-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _class "form-control"; _id (nameof m.Question1Answer)
                            _name (nameof m.Question1Answer); _value m.Question1Answer; _placeholder "Question 1"
                            _maxlength "30"; _required ]
                    label [ _class "jjj-required"; _for (nameof m.Question1Answer) ] [ str q1 ]
                ]
                input [ _type "hidden"; _name (nameof m.Question1Index); _value (string m.Question1Index ) ]
            ]
            div [ _class "col-12 col-xl-6" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _class "form-control"; _id (nameof m.Question2Answer)
                            _name (nameof m.Question2Answer); _value m.Question2Answer; _placeholder "Question 2"
                            _maxlength "30"; _required ]
                    label [ _class "jjj-required"; _for (nameof m.Question2Answer) ] [ str q2 ]
                ]
                input [ _type "hidden"; _name (nameof m.Question2Index); _value (string m.Question2Index ) ]
            ]
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] []; rawText "&nbsp; Save"
                ]
            ]
            script [] [ rawText """
                const pw = document.getElementById("Password")
                const pwConfirm = document.getElementById("ConfirmPassword")
                pwConfirm.addEventListener("input", () => {
                    if (!pw.validity.valid) {
                        pwConfirm.setCustomValidity("")
                    } else if (!pwConfirm.validity.valueMissing && pw.value !== pwConfirm.value) {
                        pwConfirm.setCustomValidity("Confirmation password does not match")
                    } else {
                        pwConfirm.setCustomValidity("")
                    }
                })"""
            ]
        ]
    ]
