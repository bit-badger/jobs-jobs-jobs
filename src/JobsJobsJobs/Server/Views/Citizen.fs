/// Views for URLs beginning with /citizen
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Citizen

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.Domain
open JobsJobsJobs.ViewModels

/// The form to add or edit a means of contact
let contactEdit (contacts : OtherContactForm array) =
    let mapToInputs (idx : int) (contact : OtherContactForm) =
        div [ _id $"contactRow{idx}"; _class "row pb-3" ] [
            div [ _class "col-2 col-md-1" ] [
                button [ _type "button"; _class "btn btn-sm btn-outline-danger rounded-pill mt-3"; _title "Delete"
                         _onclick $"jjj.citizen.removeContact({idx})" ] [
                    rawText " &minus; "
                ]
            ]
            div [ _class "col-10 col-md-4 col-xl-3" ] [
                div [ _class "form-floating" ] [
                    select [ _id $"contactType{idx}"; _name $"Contacts[{idx}].ContactType"; _class "form-control"
                             _value contact.ContactType; _placeholder "Type"; _required ] [
                        option [ _value "Website" ] [ rawText "Website" ]
                        option [ _value "Email" ] [ rawText "E-mail Address" ]
                        option [ _value "Phone" ] [ rawText "Phone Number" ]
                    ]
                    label [ _class "jjj-required"; _for $"contactType{idx}" ] [ rawText "Type" ]
                ]
            ]
            div [ _class "col-12 col-md-4 col-xl-3" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"contactName{idx}"; _name $"Contacts[{idx}].Name"; _class "form-control"
                            _maxlength "1000"; _value contact.Name; _placeholder "Name" ]
                    label [ _class "jjj-label"; _for $"contactName{idx}" ] [ rawText "Name" ]
                ]
                if idx < 1 then
                    div [ _class "form-text" ] [ rawText "Optional; will link sites and e-mail, qualify phone numbers" ]
            ]
            div [ _class "col-12 col-md-7 offset-md-1 col-xl-4 offset-xl-0" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id $"contactValue{idx}"; _name $"Contacts[{idx}].Value"
                            _class "form-control"; _maxlength "1000"; _value contact.Value; _placeholder "Contact"
                            _required ]
                    label [ _class "jjj-required"; _for "contactValue{idx}" ] [ rawText "Contact" ]
                ]
                if idx < 1 then div [ _class "form-text"] [ rawText "The URL, e-mail address, or phone number" ]
            ]
            div [ _class "col-12 col-md-3 offset-md-1 col-xl-1 offset-xl-0" ] [
                div [ _class "form-check mt-3" ] [
                    input [ _type "checkbox"; _id $"contactIsPublic{idx}"; _name $"Contacts[{idx}].IsPublic";
                            _class "form-check-input"; _value "true"; if contact.IsPublic then _checked ]
                    label [ _class "form-check-label"; _for $"contactIsPublic{idx}" ] [ rawText "Public" ]
                ]
            ]
        ]
    template [ _id "newContact" ] [
        mapToInputs -1 { ContactType = "Website"; Name = ""; Value = ""; IsPublic = false }
    ]
    :: (contacts |> Array.mapi mapToInputs |> List.ofArray)

/// The account edit page
let account (m : AccountProfileForm) csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Account Profile" ]
        p [] [
            rawText "This information is visible to all fellow logged-on citizens. For publicly-visible employment "
            rawText "profiles and job listings, the &ldquo;Display Name&rdquo; fields and any public contacts will be "
            rawText "displayed."
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
                div [ _class "form-text" ] [ em [] [ rawText "Optional; overrides first/last for display" ] ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] (nameof m.NewPassword) "" "New Password" false
                div [ _class "form-text" ] [ rawText "Leave blank to keep your current password" ]
            ]
            div [ _class "col-6 col-xl-4" ] [
                textBox [ _type "password"; _minlength "8" ] (nameof m.NewPasswordConfirm) "" "Confirm New Password"
                        false
                div [ _class "form-text" ] [ rawText "Leave blank to keep your current password" ]
            ]
            div [ _class "col-12" ] [
                hr []
                h4 [ _class "pb-2" ] [
                    rawText "Ways to Be Contacted &nbsp; "
                    button [ _type "button"; _class "btn btn-sm btn-outline-primary rounded-pill"
                             _onclick "jjj.citizen.addContact()" ] [
                        rawText "Add a Contact Method"
                    ]
                ]
            ]
            yield! contactEdit m.Contacts
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] [ rawText "&nbsp; Save" ]
                ]
            ]
        ]
        hr []
        p [ _class "text-muted fst-italic" ] [
            rawText "(If you want to delete your profile, or your entire account, "
            a [ _href "/citizen/so-long" ] [ rawText "see your deletion options here" ]; rawText ".)"
        ]
        jsOnLoad $"
            jjj.citizen.nextIndex = {m.Contacts.Length}
            jjj.citizen.validatePasswords('{nameof m.NewPassword}', '{nameof m.NewPasswordConfirm}', false)"
    ]


/// The account confirmation page
let confirmAccount isConfirmed =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Account Confirmation" ]
        p [] [
            if isConfirmed then
                rawText "Your account was confirmed successfully! You may "
                a [ _href "/citizen/log-on" ] [ rawText "log on here" ]; rawText "."
            else
                rawText "The confirmation token did not match any pending accounts. Confirmation tokens are only valid "
                rawText "for 3 days; if the token expired, you will need to re-register, which "
                a [ _href "/citizen/register" ] [ rawText "you can do here" ]; rawText "."
        ]
    ]

/// The citizen's dashboard page
let dashboard (citizen : Citizen) (profile : Profile option) profileCount tz =
    article [ _class "container" ] [
        h3 [ _class "pb-4" ] [ rawText "ITM, "; str citizen.FirstName; rawText "!" ]
        div [ _class "row row-cols-1 row-cols-md-2" ] [
            div [ _class "col" ] [
                div [ _class "card h-100" ] [
                    h5 [ _class "card-header" ] [ rawText "Your Profile" ]
                    div [ _class "card-body" ] [
                        match profile with
                        | Some prfl ->
                            h6 [ _class "card-subtitle mb-3 text-muted fst-italic" ] [
                                rawText "Last updated "; str (fullDateTime prfl.LastUpdatedOn tz)
                            ]
                            p [ _class "card-text" ] [
                                rawText "Your profile currently lists "; str $"{List.length prfl.Skills}"
                                rawText " skill"; rawText (if List.length prfl.Skills <> 1 then "s" else "")
                                rawText "."
                                if prfl.IsSeekingEmployment then
                                    br []; br []
                                    rawText "Your profile indicates that you are seeking employment. Once you find it, "
                                    a [ _href "/success-story/add" ] [ rawText "tell your fellow citizens about it!" ]
                            ]
                        | None ->
                            p [ _class "card-text" ] [
                                rawText "You do not have an employment profile established; click below (or "
                                rawText "&ldquo;Edit Profile&rdquo; in the menu) to get started!"
                            ]
                    ]
                    div [ _class "card-footer" ] [
                        match profile with
                        | Some p ->
                            a [ _href $"/profile/{CitizenId.toString citizen.Id}/view"
                                _class "btn btn-outline-secondary" ] [
                                rawText "View Profile"
                            ]; rawText "&nbsp; &nbsp;"
                            a [ _href "/profile/edit"; _class "btn btn-outline-secondary" ] [ rawText "Edit Profile" ]
                        | None ->
                            a [ _href "/profile/edit"; _class "btn btn-primary" ] [ rawText "Create Profile" ]
                    ]
                ]
            ]
            div [ _class "col" ] [
                div [ _class "card h-100" ] [
                    h5 [ _class "card-header" ] [ rawText "Other Citizens" ]
                    div [ _class "card-body" ] [
                        h6 [ _class "card-subtitle mb-3 text-muted fst-italic" ] [
                            rawText (if profileCount = 0L then "No" else $"{profileCount} Total")
                            rawText " Employment Profile"; rawText (if profileCount <> 1 then "s" else "")
                        ]
                        p [ _class "card-text" ] [
                            if profileCount = 1 && Option.isSome profile then
                                "It looks like, for now, it&rsquo;s just you&hellip;"
                            else if profileCount > 0 then "Take a look around and see if you can help them find work!"
                            else "You can click below, but you will not find anything&hellip;"
                            |> rawText
                        ]
                    ]
                    div [ _class "card-footer" ] [
                        a [ _href "/profile/search"; _class "btn btn-outline-secondary" ] [ rawText "Search Profiles" ]
                    ]
                ]
            ]
        ]
        p [] [ rawText "&nbsp;" ]
        p [] [
            rawText "To see how this application works, check out &ldquo;How It Works&rdquo; in the sidebar (last "
            rawText "updated August 29<sup>th</sup>, 2021)."
        ]
    ]


/// The account deletion success page
let deleted =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Account Deletion Success" ]
        p [] [ rawText "&nbsp;" ]
        p [] [ rawText "Your account has been successfully deleted." ]
        p [] [ rawText "&nbsp;" ]
        p [] [ rawText "Thank you for participating, and thank you for your courage. #GitmoNation" ]
    ]


/// The profile or account deletion page
let deletionOptions csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Account Deletion Options" ]
        h4 [ _class "pb-3" ] [ rawText "Option 1 &ndash; Delete Your Profile" ]
        p [] [
            rawText "Utilizing this option will remove your current employment profile and skills. This will preserve "
            rawText "any job listings you may have posted, or any success stories you may have written, and preserves "
            rawText "this application&rsquo;s knowledge of you. This is what you want to use if you want to clear out "
            rawText "your profile and start again (and remove the current one from others&rsquo; view)."
        ]
        form [ _class "text-center"; _method "POST"; _action "/profile/delete" ] [
            antiForgery csrf
            button [ _type "submit"; _class "btn btn-danger" ] [ rawText "Delete Your Profile" ]
        ]
        hr []
        h4 [ _class "pb-3" ] [ rawText "Option 2 &ndash; Delete Your Account" ]
        p [] [
            rawText "This option will make it like you never visited this site. It will delete your profile, skills, "
            rawText "job listings, success stories, and account. This is what you want to use if you want to disappear "
            rawText "from this application."
        ]
        form [ _class "text-center"; _method "POST"; _action "/citizen/delete" ] [
            antiForgery csrf
            button [ _type "submit"; _class "btn btn-danger" ] [ rawText "Delete Your Entire Account" ]
        ]
    ]


/// The account denial page
let denyAccount wasDeleted =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Account Deletion" ]
        p [] [
            if wasDeleted then
                rawText "The account was deleted successfully; sorry for the trouble."
            else
                rawText "The confirmation token did not match any pending accounts; if this was an inadvertently "
                rawText "created account, it has likely already been deleted."
        ]
    ]

/// The log on page
let logOn (m : LogOnViewModel) csrf =
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
let register q1 q2 (m : RegisterViewModel) csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Register" ]
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
                div [ _class "form-text" ] [ em [] [ rawText "Optional; overrides first/last for display" ] ]
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
                    rawText "Before your account request is through, you must answer these questions two&hellip;"
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
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] []; rawText "&nbsp; Save"
                ]
            ]
            jsOnLoad $"jjj.citizen.validatePasswords('{nameof m.Password}', 'ConfirmPassword', true)"
        ]
    ]

/// The confirmation page for user registration
let registered =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "Registration Successful" ]
        p [] [
            rawText "You have been successfully registered with Jobs, Jobs, Jobs. Check your e-mail for a confirmation "
            rawText "link; it will be valid for the next 72 hours (3 days). Once you confirm your account, you will be "
            rawText "able to log on using the e-mail address and password you provided."
        ]
        p [] [
            rawText "If the account is not confirmed within the 72-hour window, it will be deleted, and you will need "
            rawText "to register again."
        ]
        p [] [
            rawText "If you encounter issues, feel free to reach out to @danieljsummers on No Agenda Social for "
            rawText "assistance."
        ]
    ]
