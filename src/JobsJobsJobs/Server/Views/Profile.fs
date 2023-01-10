/// Views for /profile URLs
[<RequireQualifiedAccess>]
module JobsJobsJobs.Views.Profile

open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open JobsJobsJobs.ViewModels

/// The profile edit page
let edit (m : EditProfileViewModel) continents isNew csrf =
    article [] [
        h3 [ _class "pb-3" ] [ rawText "My Employment Profile" ]
        form [ _class "row g-3"; _action "/profile/edit"; _hxPost "/profile/edit" ] [
            antiForgery csrf
            div [ _class "col-12" ] [
                div [ _class "form-check" ] [
                    input [ _type "checkbox"; _id (nameof m.IsSeekingEmployment); _name (nameof m.IsSeekingEmployment)
                            _class "form-check-input"; if m.IsSeekingEmployment then _checked ]
                    label [ _class "form-check-label"; _for (nameof m.IsSeekingEmployment) ] [
                        rawText "I am currently seeking employment"
                    ]
                ]
                if m.IsSeekingEmployment then
                    p [] [
                        em [] [
                            rawText "If you have found employment, consider "
                            a [ _href "/success-story/new/edit" ] [ rawText "telling your fellow citizens about it!" ]
                        ]
                    ]
            ]
            div [ _class "col-12 col-sm-6 col-md-4" ] [
                continentList [ _required ] (nameof m.ContinentId) continents None m.ContinentId
            ]
            div [ _class "col-12 col-sm-6 col-md-8" ] [
                div [ _class "form-floating" ] [
                    input [ _type "text"; _id (nameof m.Region); _name (nameof m.Region); _class "form-control"
                            _maxlength "255"; _placeholder "Country, state, geographic area, etc."; _required ]
                    label [ _class "jjj-required"; _for (m.Region) ] [ rawText "Region" ]
                ]
                div [ _class "form-text" ] [ rawText "Country, state, geographic area, etc." ]
            ]
            markdownEditor [ _required ] (nameof m.Biography) m.Biography "Professional Biography"
            div [ _class "col-12 col-offset-md-2 col-md-4" ] [
                div [ _class "form-check" ] [
                    input [ _type "checkbox"; _id (nameof m.RemoteWork); _name (nameof m.RemoteWork)
                            _class "form-check-input"; if m.RemoteWork then _checked ]
                    label [ _class "form-check-label"; _for (nameof m.RemoteWork) ] [
                        rawText "I am looking for remote work"
                    ]
                ]
            ]
            div [ _class "col-12 col-md-4" ] [
                div [ _class "form-check" ] [
                    input [ _type "checkbox"; _id (nameof m.FullTime); _name (nameof m.FullTime)
                            _class "form-check-input"; if m.FullTime then _checked ]
                    label [ _class "form-check-label"; _for (nameof m.FullTime) ] [
                        rawText "I am looking for full-time work"
                    ]
                ]
            ]
            div [ _class "col-12" ] [
                hr []
                h4 [ _class "pb-2" ] [
                    rawText "Skills &nbsp; "
                    button [ _class "btn btn-sm btn-outline-primary rounded-pill"; _onclick "jjj.addSkill" ] [
                        rawText "Add a Skill"
                    ]
                ]
            ]
            //<profile-skill-edit v-for="(skill, idx) in profile.skills" :key="skill.id" v-model="profile.skills[idx]"
            //                    @remove="removeSkill(skill.id)" @input="v$.skills.$touch" />
            div [ _class "col-12" ] [
                hr []
                h4 [] [ rawText "Experience" ]
                p [] [
                    rawText "This application does not have a place to individually list your chronological job "
                    rawText "history; however, you can use this area to list prior jobs, their dates, and anything "
                    rawText "else you want to include that&rsquo;s not already a part of your Professional Biography "
                    rawText "above."
                ]
            ]
            markdownEditor [] (nameof m.Experience) (defaultArg m.Experience "") "Experience"
            div [ _class "col-12" ] [
                div [ _class "form-check" ] [
                    input [ _type "checkbox"; _id (nameof m.IsPublic); _name (nameof m.IsPublic)
                            _class "form-check-input"; if m.IsPublic then _checked ]
                    label [ _class "form-check-label"; _for (nameof m.IsPublic) ] [
                        rawText "Allow my profile to be searched publicly"
                    ]
                ]
            ]
            div [ _class "col-12" ] [
                button [ _type "submit"; _class "btn btn-primary" ] [
                    i [ _class "mdi mdi-content-save-outline" ] []; rawText "&nbsp; Save"
                ]
                if not isNew then
                    rawText "&nbsp; &nbsp; "
                    a [ _class "btn btn-outline-secondary"; _href "`/profile/${user.citizenId}/view`" ] [
                        i [ _color "#6c757d"; _class "mdi mdi-file-account-outline" ] []
                        rawText "&nbsp; View Your User Profile"
                    ]
            ]
        ]
        hr []
        p [ _class "text-muted fst-italic" ] [
            rawText "(If you want to delete your profile, or your entire account, "
            a [ _href "/so-long/options" ] [ rawText "see your deletion options here" ]; rawText ".)"
        ]
    ]
