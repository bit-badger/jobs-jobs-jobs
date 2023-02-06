/// Views for URLs beginning with /citizen
module JobsJobsJobs.Citizens.Views

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Citizens.Domain
open JobsJobsJobs.Common.Views
open JobsJobsJobs.Domain

/// The form to add or edit a means of contact
let contactEdit (contacts : OtherContactForm array) =
    let mapToInputs (idx : int) (contact : OtherContactForm) =
        div [ _id $"contactRow{idx}"; _class "row pb-3" ] [
            div [ _class "col-2 col-md-1" ] [
                button [ _type "button"; _class "btn btn-sm btn-outline-danger rounded-pill mt-3"; _title "Delete"
                         _onclick $"jjj.citizen.removeContact({idx})" ] [ txt " &minus; " ]
            ]
            div [ _class "col-10 col-md-4 col-xl-3" ] [
                div [ _class "form-floating" ] [
                    select [ _id $"contactType{idx}"; _name $"Contacts[{idx}].ContactType"; _class "form-control"
                             _value contact.ContactType; _placeholder "Type"; _required ] [
                        let optionFor value label =
                            let typ = ContactType.toString value
                            option [ _value typ; if contact.ContactType = typ then _selected ] [ txt label ]
                        optionFor Website "Website"
                        optionFor Email   "E-mail Address"
                        optionFor Phone   "Phone Number"
                    ]
                    label [ _class "jjj-required"; _for $"contactType{idx}" ] [ txt "Type" ]
                ]
            ]
            div [ _class "col-12 col-md-4 col-xl-3" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"contactName{idx}"; _name $"Contacts[{idx}].Name"; _class "form-control"
                            _maxlength "1000"; _value contact.Name; _placeholder "Name" ]
                    label [ _class "jjj-label"; _for $"contactName{idx}" ] [ txt "Name" ]
                ]
                if idx < 1 then
                    div [ _class "form-text" ] [ txt "Optional; will link sites and e-mail, qualify phone numbers" ]
            ]
            div [ _class "col-12 col-md-7 offset-md-1 col-xl-4 offset-xl-0" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"contactValue{idx}"; _name $"Contacts[{idx}].Value"
                            _class "form-control"; _maxlength "1000"; _value contact.Value; _placeholder "Contact"
                            _required ]
                    label [ _class "jjj-required"; _for "contactValue{idx}" ] [ txt "Contact" ]
                ]
                if idx < 1 then div [ _class "form-text"] [ txt "The URL, e-mail address, or phone number" ]
            ]
            div [ _class "col-12 col-md-3 offset-md-1 col-xl-1 offset-xl-0" ] [
                div [ _class "form-check mt-3" ] [
                    input [ _type "checkbox"; _id $"contactIsPublic{idx}"; _name $"Contacts[{idx}].IsPublic";
                            _class "form-check-input"; _value "true"; if contact.IsPublic then _checked ]
                    label [ _class "form-check-label"; _for $"contactIsPublic{idx}" ] [ txt "Public" ]
                ]
            ]
        ]
    template [ _id "newContact" ] [
        mapToInputs -1 { ContactType = "Website"; Name = ""; Value = ""; IsPublic = false }
    ]
    :: (contacts |> Array.mapi mapToInputs |> List.ofArray)

/// The account edit page
let account (m : AccountProfileForm) isHtmx csrf =
    pageWithTitle "Account Profile" [
        p [] [
            txt "This information is visible to all fellow logged-on citizens. For publicly-visible employment "
            txt "profiles and job listings, the &ldquo;Display Name&rdquo; fields and any public contacts will be "
            txt "displayed."
        ]
        form [ _class "row g-3"; _method "POST"; _action "/citizen/save-account" ] [
            antiForgery csrf
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text"; _autofocus ] (nameof m.FirstName) m.FirstName "First Name" true
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text" ] (nameof m.LastName) m.LastName "Last Name" true
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text" ] (nameof m.DisplayName) m.DisplayName "Display Name" false
                div [ _class "form-text" ] [ em [] [ txt "Optional; overrides first/last for display" ] ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] (nameof m.NewPassword) "" "New Password" false
                div [ _class "form-text" ] [ txt "Leave blank to keep your current password" ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] (nameof m.NewPasswordConfirm) "" "Confirm New Password"
                        false
                div [ _class "form-text" ] [ txt "Leave blank to keep your current password" ]
            ]
            div [ _class "col-12" ] [
                hr []
                h4 [ _class "pb-2" ] [
                    txt "Ways to Be Contacted &nbsp; "
                    button [ _type "button"; _class "btn btn-sm btn-outline-primary rounded-pill"
                             _onclick "jjj.citizen.addContact()" ] [ txt "Add a Contact Method" ]
                ]
            ]
            yield! contactEdit m.Contacts
            div [ _class "col-12" ] [ submitButton "content-save-outline" "Save" ]
        ]
        hr []
        p [ _class "text-muted fst-italic" ] [
            txt "(If you want to delete your profile, or your entire account, "
            a [ _href "/citizen/so-long" ] [ rawText "see your deletion options here" ]; txt ".)"
        ]
        jsOnLoad $"
            jjj.citizen.nextIndex = {m.Contacts.Length}
            jjj.citizen.validatePasswords('{nameof m.NewPassword}', '{nameof m.NewPasswordConfirm}', false)" isHtmx
    ]


/// The account confirmation page
let confirmAccount isConfirmed =
    pageWithTitle "Account Confirmation" [
        p [] [
            if isConfirmed then
                txt "Your account was confirmed successfully! You may "
                a [ _href "/citizen/log-on" ] [ rawText "log on here" ]; txt "."
            else
                txt "The confirmation token did not match any pending accounts. Confirmation tokens are only valid for "
                txt "3 days; if the token expired, you will need to re-register, which "
                a [ _href "/citizen/register" ] [ txt "you can do here" ]; txt "."
        ]
    ]

/// The citizen's dashboard page
let dashboard (citizen : Citizen) (profile : Profile option) profileCount tz =
    article [ _class "container" ] [
        h3 [ _class "pb-4" ] [ str $"ITM, {citizen.FirstName}!" ]
        div [ _class "row row-cols-1 row-cols-md-2" ] [
            div [ _class "col" ] [
                div [ _class "card h-100" ] [
                    h5 [ _class "card-header" ] [ txt "Your Profile" ]
                    div [ _class "card-body" ] [
                        match profile with
                        | Some prfl ->
                            h6 [ _class "card-subtitle mb-3 text-muted fst-italic" ] [
                                str $"Last updated {fullDateTime prfl.LastUpdatedOn tz}"
                            ]
                            p [ _class "card-text" ] [
                                txt $"Your profile currently lists {List.length prfl.Skills} skill"
                                txt (if List.length prfl.Skills <> 1 then "s" else ""); txt "."
                                if prfl.IsSeekingEmployment then
                                    br []; br []
                                    txt "Your profile indicates that you are seeking employment. Once you find it, "
                                    a [ _href "/success-story/add" ] [ txt "tell your fellow citizens about it!" ]
                            ]
                        | None ->
                            p [ _class "card-text" ] [
                                txt "You do not have an employment profile established; click below (or &ldquo;Edit "
                                txt "Profile&rdquo; in the menu) to get started!"
                            ]
                    ]
                    div [ _class "card-footer" ] [
                        match profile with
                        | Some _ ->
                            a [ _href $"/profile/{CitizenId.toString citizen.Id}/view"
                                _class "btn btn-outline-secondary" ] [ txt "View Profile" ]; txt "&nbsp; &nbsp;"
                            a [ _href "/profile/edit"; _class "btn btn-outline-secondary" ] [ txt "Edit Profile" ]
                        | None ->
                            a [ _href "/profile/edit"; _class "btn btn-primary" ] [ txt "Create Profile" ]
                    ]
                ]
            ]
            div [ _class "col" ] [
                div [ _class "card h-100" ] [
                    h5 [ _class "card-header" ] [ txt "Other Citizens" ]
                    div [ _class "card-body" ] [
                        h6 [ _class "card-subtitle mb-3 text-muted fst-italic" ] [
                            txt (if profileCount = 0L then "No" else $"{profileCount} Total")
                            txt " Employment Profile"; txt (if profileCount <> 1 then "s" else "")
                        ]
                        p [ _class "card-text" ] [
                            if profileCount = 1 && Option.isSome profile then
                                "It looks like, for now, it&rsquo;s just you&hellip;"
                            else if profileCount > 0 then "Take a look around and see if you can help them find work!"
                            else "You can click below, but you will not find anything&hellip;"
                            |> txt
                        ]
                    ]
                    div [ _class "card-footer" ] [
                        a [ _href "/profile/search"; _class "btn btn-outline-secondary" ] [ txt "Search Profiles" ]
                    ]
                ]
            ]
        ]
        emptyP
        p [] [
            txt "To see how this application works, check out &ldquo;How It Works&rdquo; in the sidebar (last updated "
            txt "February 2<sup>nd</sup>, 2023)."
        ]
    ]


/// The account deletion success page
let deleted =
    pageWithTitle "Account Deletion Success" [
        emptyP; p [] [ txt "Your account has been successfully deleted." ]
        emptyP; p [] [ txt "Thank you for participating, and thank you for your courage. #GitmoNation" ]
    ]


/// The profile or account deletion page
let deletionOptions csrf =
    pageWithTitle "Account Deletion Options" [
        h4 [ _class "pb-3" ] [ txt "Option 1 &ndash; Delete Your Profile" ]
        p [] [
            txt "Utilizing this option will remove your current employment profile and skills. This will preserve any "
            txt "job listings you may have posted, or any success stories you may have written, and preserves this "
            txt "this application&rsquo;s knowledge of you. This is what you want to use if you want to clear out your "
            txt "profile and start again (and remove the current one from others&rsquo; view)."
        ]
        form [ _class "text-center"; _method "POST"; _action "/profile/delete" ] [
            antiForgery csrf
            button [ _type "submit"; _class "btn btn-danger" ] [ txt "Delete Your Profile" ]
        ]
        hr []
        h4 [ _class "pb-3" ] [ txt "Option 2 &ndash; Delete Your Account" ]
        p [] [
            txt "This option will make it like you never visited this site. It will delete your profile, skills, job "
            txt "listings, success stories, and account. This is what you want to use if you want to disappear from "
            txt "this application."
        ]
        form [ _class "text-center"; _method "POST"; _action "/citizen/delete" ] [
            antiForgery csrf
            button [ _type "submit"; _class "btn btn-danger" ] [ txt "Delete Your Entire Account" ]
        ]
    ]


/// The account denial page
let denyAccount wasDeleted =
    pageWithTitle "Account Deletion" [
        p [] [
            if wasDeleted then txt "The account was deleted successfully; sorry for the trouble."
            else
                txt "The confirmation token did not match any pending accounts; if this was an inadvertently created "
                txt "account, it has likely already been deleted."
        ]
    ]


/// The forgot / reset password page
let forgotPassword csrf =
    let m = { Email = "" }
    pageWithTitle "Forgot Password" [
        p [] [
            txt "Enter your e-mail address below; if it matches the e-mail address of an account, we will send a "
            txt "password reset link."
        ]
        form [ _class "row g-3 pb-3"; _method "POST"; _action "/citizen/forgot-password" ] [
            antiForgery csrf
            div [ _class "col-12 col-md-6 offset-md-3" ] [
                textBox [ _type "email"; _autofocus ] (nameof m.Email) m.Email "E-mail Address" true
            ]
            div [ _class "col-12" ] [ submitButton "send-lock-outline" "Send Reset Link" ]
        ]
    ]


/// The page displayed after a forgotten / reset request has been processed
let forgotPasswordSent (m : ForgotPasswordForm) =
    pageWithTitle "Reset Request Processed" [
        p [] [
            txt $"The reset link request has been processed. If the e-mail address {m.Email} matched an account, "
            txt "further instructions were sent to that address."
        ]
    ]


/// The log on page
let logOn (m : LogOnForm) csrf =
    pageWithTitle "Log On" [
        match m.ErrorMessage with
        | Some msg ->
            p [ _class "pb-3 text-center" ] [
                span [ _class "text-danger" ] [ txt msg ]; br []
                if msg.IndexOf("ocked") > -1 then
                    txt "If this is a new account, it must be confirmed before it can be used; otherwise, you need to "
                    a [ _href "/citizen/forgot-password" ] [ txt "request an unlock code" ]
                    txt " before you may log on."
            ]
        | None -> ()
        form [ _class "row g-3 pb-3"; _hxPost "/citizen/log-on" ] [
            antiForgery csrf
            match m.ReturnTo with
            | Some returnTo -> input [ _type "hidden"; _name (nameof m.ReturnTo); _value returnTo ]
            | None -> ()
            div [ _class "col-12 col-md-6" ] [
                textBox [ _type "email"; _autofocus ] (nameof m.Email) m.Email "E-mail Address" true
            ]
            div [ _class "col-12 col-md-6" ] [
                textBox [ _type "password" ] (nameof m.Password) "" "Password" true
            ]
            div [ _class "col-12" ] [ submitButton "login" "Log On" ]
        ]
        p [ _class "text-center" ] [
            txt "Need an account? "; a [ _href "/citizen/register" ] [ txt "Register for one!" ]
        ]
        p [ _class "text-center" ] [
            txt "Forgot your password? "; a [ _href "/citizen/forgot-password" ] [ txt "Request a reset." ]
        ]
    ]

/// The registration page
let register q1 q2 (m : RegisterForm) isHtmx csrf =
    pageWithTitle "Register" [
        form [ _class  "row g-3"; _hxPost "/citizen/register" ] [
            antiForgery csrf
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text"; _autofocus ] (nameof m.FirstName) m.FirstName "First Name" true
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text" ] (nameof m.LastName) m.LastName "Last Name" true
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text" ] (nameof m.DisplayName) (defaultArg m.DisplayName "") "Display Name" false
                div [ _class "form-text fst-italic" ] [ txt "Optional; overrides first/last for display" ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "text" ] (nameof m.Email) m.Email "E-mail Address" true
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] (nameof m.Password) "" "Password" true
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] "ConfirmPassword" "" "Confirm Password" true
            ]
            div [ _class "col-12" ] [
                hr []
                p [ _class "mb-0 text-muted fst-italic" ] [
                    txt "Before your account request is through, you must answer these questions two&hellip;"
                ]
            ]
            div [ _class "col-12 col-xl-6" ] [
                textBox [ _type "text"; _maxlength "30" ] (nameof m.Question1Answer) m.Question1Answer q1 true
                input [ _type "hidden"; _name (nameof m.Question1Index); _value (string m.Question1Index ) ]
            ]
            div [ _class "col-12 col-xl-6" ] [
                textBox [ _type "text"; _maxlength "30" ] (nameof m.Question2Answer) m.Question2Answer q2 true
                input [ _type "hidden"; _name (nameof m.Question2Index); _value (string m.Question2Index ) ]
            ]
            div [ _class "col-12" ] [ submitButton "content-save-outline" "Save" ]
            jsOnLoad $"jjj.citizen.validatePasswords('{nameof m.Password}', 'ConfirmPassword', true)" isHtmx
        ]
    ]

/// The confirmation page for user registration
let registered =
    pageWithTitle "Registration Successful" [
        p [] [
            txt "You have been successfully registered with Jobs, Jobs, Jobs. Check your e-mail for a confirmation "
            txt "link; it will be valid for the next 72 hours (3 days). Once you confirm your account, you will be "
            txt "able to log on using the e-mail address and password you provided."
        ]
        p [] [
            txt "If the account is not confirmed within the 72-hour window, it will be deleted, and you will need to "
            txt "register again."
        ]
        p [] [
            txt "If you encounter issues, feel free to reach out to @danieljsummers on No Agenda Social for assistance."
        ]
    ]

/// The confirmation page for canceling a reset request
let resetCanceled wasCanceled =
    let pgTitle = if wasCanceled then "Password Reset Request Canceled" else "Reset Request Not Found"
    pageWithTitle pgTitle [
        p [] [
            if wasCanceled then txt "Your password reset request has been canceled."
            else txt "There was no active password reset request found; it may have already expired."
        ]
    ]


/// The password reset page
let resetPassword (m : ResetPasswordForm) isHtmx csrf =
    pageWithTitle "Reset Password" [
        p [] [ txt "Enter your new password in the fields below" ]
        form [ _class "row g-3"; _method "POST"; _action "/citizen/reset-password" ] [
            antiForgery csrf
            input [ _type "hidden"; _name (nameof m.Id); _value m.Id ]
            input [ _type "hidden"; _name (nameof m.Token); _value m.Token ]
            div [ _class "col-12 col-md-6 col-xl-4 offset-xl-2" ] [
                textBox [ _type "password"; _minlength "8"; _autofocus ] (nameof m.Password) "" "New Password" true
            ]
            div [ _class "col-12 col-md-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] "ConfirmPassword" "" "Confirm New Password" true
            ]
            div [ _class "col-12" ] [ submitButton "lock-reset" "Reset Password" ]
            jsOnLoad $"jjj.citizen.validatePasswords('{nameof m.Password}', 'ConfirmPassword', true)" isHtmx
        ]
    ]

// ~~~ LEGACY MIGRATION ~~~ //

let legacy (current : Citizen list) (legacy : Citizen list) csrf =
    form [ _class "container"; _hxPost "/citizen/legacy/migrate" ] [
        antiForgery csrf
        let canProcess = not (List.isEmpty current)
        div [ _class "row" ] [
            if canProcess then
                div [ _class "col-12 col-lg-6 col-xxl-4" ] [
                    div [ _class "form-floating" ] [
                        select [ _id "current"; _name "Id"; _class "form-control" ] [
                            option [ _value "" ] [ txt "&ndash; Select &ndash;" ]
                            yield!
                                current
                                |> List.sortBy Citizen.name
                                |> List.map (fun it ->
                                    option [ _value (CitizenId.toString it.Id) ] [
                                        str (Citizen.name it); txt " ("; str it.Email; txt ")"
                                    ])
                        ]
                        label [ _for "current" ] [ txt "Current User" ]
                    ]
                ]
            else p [] [ txt "There are no current accounts to which legacy accounts can be migrated" ]
            div [ _class "col-12 col-lg-6 offset-xxl-2"] [
                table [ _class "table table-sm table-hover" ] [
                    thead [] [
                        tr [] [
                            th [ _scope "col" ] [ txt "Select" ]
                            th [ _scope "col" ] [ txt "NAS Profile" ]
                        ]
                    ]
                    legacy |> List.map (fun it ->
                        let theId = CitizenId.toString it.Id
                        tr [] [
                            td [] [
                                if canProcess then
                                    input [ _type "radio"; _id $"legacy_{theId}"; _name "LegacyId"; _value theId ]
                                else txt "&nbsp;"
                            ]
                            td [] [ label [ _for $"legacy_{theId}" ] [ str it.Email ] ]
                        ])
                    |> tbody []
                ]
            ]
        ]
        submitButton "content-save-outline" "Migrate Account"
    ]
    |> List.singleton
    |> pageWithTitle "Migrate Legacy Account"
