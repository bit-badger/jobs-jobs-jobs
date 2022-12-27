module JobsJobsJobs.Views.Layout

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open Giraffe.ViewEngine.Htmx

/// Data items needed to render a view
type PageRenderContext =
    {   /// Whether a user is logged on
        IsLoggedOn : bool

        /// The current URL
        CurrentUrl : string

        /// The title of this page
        PageTitle : string

        /// The page content
        Content : XmlNode
    }

/// Generate the HTML head tag
let private htmlHead ctx =
    let pageTitle =
        seq {
            if ctx.PageTitle <> "" then
                ctx.PageTitle; " | "
            "Jobs, Jobs, Jobs"
        } |> Seq.reduce (+)
    head [] [
        meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
        title [] [ str pageTitle ]
        link [ _href        "https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/css/bootstrap.min.css"
               _rel         "stylesheet"
               _integrity   "sha384-gH2yIJqKdNHPEq0n4Mqa/HGKIhSkIHeL5AyhkYV8i59U5AR6csBvApHHNl/vI1Bx"
               _crossorigin "anonymous" ]
        link [ _href "https://cdn.jsdelivr.net/npm/@mdi/font@6.9.96/css/materialdesignicons.min.css"
               _rel "stylesheet" ]
        link [ _href "/style.css"; _rel "stylesheet" ]
    ]

/// Display the links available to the current user
let private links ctx =
    let navLink url icon text =
        a [ _href url
            _onclick "jjj.hideMenu()"
            if url = ctx.CurrentUrl then _class "jjj-current-page"
        ] [ i [ _class $"mdi mdi-{icon}"; _ariaHidden "true" ] []; rawText text ]
    nav [ _class "jjj-nav" ] [
        if ctx.IsLoggedOn then
            navLink "/citizen/dashboard"  "view-dashboard-variant"             "Dashboard"
            navLink "/help-wanted"        "newspaper-variant-multiple-outline" "Help Wanted!"
            navLink "/profile/search"     "view-list-outline"                  "Employment Profiles"
            navLink "/success-story/list" "thumb-up"                           "Success Stories"
            div [ _class "separator" ] []
            navLink "/citizen/account" "mdiAccountEdit" "My Account"
            navLink "/listings/mine"   "mdiSignText"    "My Job Listings"
            navLink "/profile/edit"    "mdiPencil"      "My Employment Profile"
            div [ _class "separator" ] []
            navLink "/citizen/log-off" "mdiLogoutVariant" "Log Off"
        else
            navLink "/"                "home"              "Home"
            navLink "/profile/seeking" "view-list-outline" "Job Seekers"
            navLink "/citizen/log-on"  "login-variant"     "Log On"
        navLink "/how-it-works" "help-circle-outline" "How It Works"
    ]

/// Generate mobile and desktop side navigation areas
let private sideNavs ctx = [
    div [ _id "mobileMenu"
          _class "jjj-mobile-menu offcanvas offcanvas-end"
          _tabindex "-1"
          _ariaLabelledBy "mobileMenuLabel" ] [
        div [ _class "offcanvas-header" ] [
            h5 [ _id "mobileMenuLabel" ] [ rawText "Menu" ]
            button [
                _class "btn-close text-reset"; _type "button"; _data "bs-dismiss" "offcanvas"; _ariaLabel "Close"
            ] []
        ]
        div [ _class "offcanvas-body" ] [ links ctx ]
    ]
    aside [ _class "jjj-full-menu d-none d-md-block p-3" ] [
        p [ _class "home-link pb-3" ] [ a [ _href "/" ] [ rawText "Jobs, Jobs, Jobs" ] ]
        p [] [ rawText "&nbsp;" ]
        links ctx
    ]
]

/// Title bars for mobile and desktop
let private titleBars = [
    nav [ _class "d-flex d-md-none navbar navbar-dark" ] [
        span [ _class "navbar-text" ] [ a [ _href "/" ] [ rawText "Jobs, Jobs, Jobs" ] ]
        button [ _class "btn"
                 _data "bs-toggle" "offcanvas"
                 _data "bs-target" "#mobileMenu"
                 _ariaControls "mobileMenu" ] [ i [ _class "mdi mdi-menu" ] [] ]
    ]
    nav [ _class "d-none d-md-flex navbar navbar-light bg-light"] [
        span [] [ rawText "&nbsp;" ]
        span [ _class "navbar-text" ] [
            rawText "(&hellip;and Jobs &ndash; "
            audioClip "pelosi-jobs" (rawText "Let&rsquo;s Vote for Jobs!")
            rawText ")"
        ]
    ]
]

/// The HTML footer for the page
let private htmlFoot =
    let v = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
    let version =
        seq {
            string v.Major
            if v.Minor > 0 then
                "."; string v.Minor
                if v.Build > 0 then
                    "."; string v.Build
        } |> Seq.reduce (+)
    footer [] [
        p [ _class "text-muted" ] [
            str "Jobs, Jobs, Jobs v"; str version; rawText " &bull; "
            a [ _href "/privacy-policy" ] [ str "Privacy Policy" ]; rawText " &bull; "
            a [ _href "/terms-of-service" ] [ str "Terms of Service" ]
        ]
    ]

/// Create a full view
let view ctx =
    html [ _lang "en" ] [
        htmlHead ctx
        body [] [
            div [ _class "jjj-app" ] [
                yield! sideNavs ctx
                //otherSideNav ctx
                div [ _class "jjj-main" ] [
                    yield! titleBars
                    main [ _class "jjj-content container-fluid" ] [ ctx.Content ]
                    htmlFoot
                ]
            ]
            Script.minified
            script [ _async
                     _src         "https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.bundle.min.js"
                     _integrity   "sha384-A3rJD856KowSb7dwlZdYEkO39Gagi7vIsF0jrRAoQmDKKtQBHUuLZ9AsSv4jD4Xa"
                     _crossorigin "anonymous" ] []
            script [ _src "/script.js" ] []
        ]
    ]
