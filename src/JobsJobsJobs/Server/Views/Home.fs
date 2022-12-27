module JobsJobsJobs.Views.Home

open Giraffe.ViewEngine

/// The home page
let home =
    article [] [
        p [] [ rawText "&nbsp;" ]
        p [] [
            rawText "Welcome to Jobs, Jobs, Jobs (AKA No Agenda Careers), where citizens of Gitmo Nation can assist "
            rawText "one another in finding employment. This will enable them to continue providing value-for-value to "
            rawText "Adam and John, as they continue their work deconstructing the misinformation that passes for news "
            rawText "on a day-to-day basis."
        ]
        p [] [
            rawText "Do you not understand the terms in the paragraph above? No worries; just head over to "
            a [ _href "https://noagendashow.net"; _target "_blank"; _rel "noopener" ] [
                rawText "The Best Podcast in the Universe"
            ]
            rawText " "; em [] [ audioClip "thats-true" (rawText "(that&rsquo;s true!)") ]
            rawText " and find out what you&rsquo;re missing."
        ]
    ]

/// The page for terms of service
let termsOfService =
    article [] [
        h3 [] [ rawText "Terms of Service" ]
        p [ _class "fst-italic" ] [ rawText "(as of August 30<sup>th</sup>, 2022)" ]
        h4 [] [ rawText "Acceptance of Terms" ]
        p [] [
            rawText "By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that "
            rawText "you are responsible to ensure that your use of this site complies with all applicable laws. Your "
            rawText "continued use of this site implies your acceptance of these terms."
        ]
        h4 [] [ rawText "Description of Service and Registration" ]
        p [] [
            rawText "Jobs, Jobs, Jobs is a service that allows individuals to enter and amend employment profiles and "
            rawText "job listings, restricting access to the details of these to other users of this site, unless the "
            rawText "individual specifies that this information should be visible publicly. See our "
            a [ _href "/privacy-policy" ] [ str "privacy policy" ]
            rawText " for details on the personal (user) information we maintain."
        ]
        h4 [] [ rawText "Liability" ]
        p [] [
            rawText "This service is provided &ldquo;as is&rdquo;, and no warranty (express or implied) exists. The "
            rawText "service and its developers may not be held liable for any damages that may arise through the use "
            rawText "of this service."
        ]
        h4 [] [ rawText "Updates to Terms" ]
        p [] [
            rawText "These terms and conditions may be updated at any time. When these terms are updated, users will "
            rawText "be notified via a notice on the dashboard page. Additionally, the date at the top of this page "
            rawText "will be updated, and any substantive updates will also be accompanied by a summary of those "
            rawText "changes."
        ]
        hr []
        p [] [
            rawText "You may also wish to review our "
            a [ _href "/privacy-policy" ] [ rawText "privacy policy" ]
            rawText " to learn how we handle your data."
        ]
        hr []
        p [ _class "fst-italic" ] [
            rawText "Change on August 30<sup>th</sup>, 2022 &ndash; added references to job listings, removed "
            rawText "references to Mastodon instances."
        ]
        p [ _class "fst-italic" ] [
            rawText "Change on September 6<sup>th</sup>, 2021 &ndash; replaced &ldquo;No Agenda Social&rdquo; with a "
            rawText "list of all No Agenda-affiliated Mastodon instances."
        ]
    ]
