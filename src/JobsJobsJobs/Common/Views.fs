/// Common functions for views
module JobsJobsJobs.Common.Views

open Giraffe.ViewEngine
open Giraffe.ViewEngine.Accessibility
open Microsoft.AspNetCore.Antiforgery
open JobsJobsJobs.Domain

/// Create an audio clip with the specified text node
let audioClip clip text =
    span [ _class "jjj-audio-clip"; _onclick "jjj.playFile(this)" ] [
        text; audio [ _id clip ] [ source [ _src $"/audio/{clip}.mp3" ] ]
    ]

/// Create an anti-forgery hidden input
let antiForgery (csrf : AntiforgeryTokenSet) =
    input [ _type "hidden"; _name csrf.FormFieldName; _value csrf.RequestToken ]

/// Alias for rawText
let txt = rawText

/// Create a page with a title displayed on the page
let pageWithTitle title content =
    article [] [
        h3 [ _class "pb-3" ] [ txt title ]
        yield! content
    ]

/// Create a floating-label text input box
let textBox attrs name value fieldLabel isRequired =
    div [ _class "form-floating" ] [
        List.append attrs [
            _id name; _name name; _class "form-control"; _placeholder fieldLabel; _value value
            if isRequired then _required
        ] |> input
        label [ _class (if isRequired then "jjj-required" else "jjj-label"); _for name ] [ txt fieldLabel ]
    ]

/// Create a checkbox that will post "true" if checked
let checkBox attrs name isChecked checkLabel =
    div [ _class "form-check" ] [
        List.append attrs
            [ _type "checkbox"; _id name; _name name; _class "form-check-input"; _value "true"
              if isChecked then _checked ]
        |> input
        label [ _class "form-check-label"; _for name ] [ txt checkLabel ]
    ]

/// Create a select list of continents
let continentList attrs name (continents : Continent list) emptyLabel selectedValue isRequired =
    div [ _class "form-floating" ] [
        select (List.append attrs [ _id name; _name name; _class "form-select"; if isRequired then _required ]) (
            option [ _value ""; if selectedValue = "" then _selected ] [
                rawText $"""&ndash; {defaultArg emptyLabel "Select"} &ndash;""" ]
            :: (continents
                |> List.map (fun c ->
                    let theId = ContinentId.toString c.Id
                    option [ _value theId; if theId = selectedValue then _selected ] [ str c.Name ])))
        label [ _class (if isRequired then "jjj-required" else "jjj-label"); _for name ] [ txt "Continent" ]
    ]

/// Create a submit button with the given icon and text
let submitButton icon text =
    button [ _type "submit"; _class "btn btn-primary" ] [ i [ _class $"mdi mdi-%s{icon}" ] []; txt $"&nbsp; %s{text}" ]

/// An empty paragraph
let emptyP =
    p [] [ txt "&nbsp;" ]

/// Register JavaScript code to run in the DOMContentLoaded event on the page
let jsOnLoad js isHtmx =
    script [] [
        let (target, event) = if isHtmx then "document.body", "htmx:afterSettle" else "document", "DOMContentLoaded"
        txt (sprintf """%s.addEventListener("%s", () => { %s }, { once: true })""" target event js)
    ]

/// Create a Markdown editor
let markdownEditor attrs name value editorLabel isHtmx =
    div [ _class "col-12"; _id $"{name}EditRow" ] [
        nav [ _class "nav nav-pills pb-1" ] [
            button [ _type "button"; _id $"{name}EditButton"; _class "btn btn-primary btn-sm rounded-pill" ] [
                txt "Markdown"
            ]
            rawText " &nbsp; "
            button [ _type "button"; _id $"{name}PreviewButton"
                     _class "btn btn-outline-secondary btn-sm rounded-pill" ] [
                txt "Preview"
            ]
        ]
        section [ _id $"{name}Preview"; _class "jjj-not-shown jjj-markdown-preview px-2 pt-2"
                  _ariaLabel "Rendered Markdown preview" ] []
        div [ _id $"{name}Edit"; _class "form-floating jjj-shown" ] [
            textarea (List.append attrs
                                  [ _id name; _name name; _class "form-control jjj-markdown-editor"; _rows "10" ]) [
                txt value
            ]
            label [ _for name ] [ txt editorLabel ]
        ]
        jsOnLoad $"jjj.markdownOnLoad('{name}')" isHtmx
    ]

/// Wrap content in a collapsing panel
let collapsePanel header isShown content =
    let showClass = if isShown then " show" else ""
    div [ _class "card" ] [
        div [ _class "card-header" ] [
            h6 [ _class "mb-0 card-title" ] [
                a [ _href "#jjjCollapse"; _data "bs-toggle" "collapse"; _roleButton; _ariaControls "#jjjCollapse"
                    _ariaExpanded (isShown.ToString().ToLowerInvariant ()) ] [ txt header ]
            ]
        ]
        div [ _id "jjjCollapse"; _class $"card-body collapse{showClass}" ] content
    ]

/// "Yes" or "No" based on a boolean value
let yesOrNo value =
    if value then "Yes" else "No"

/// Markdown as a raw HTML text node
let md2html value =
    (MarkdownString.toHtml >> txt) value

/// Display a citizen's contact information
let contactInfo citizen isPublic =
    citizen.OtherContacts
    |> List.filter (fun it -> (isPublic && it.IsPublic) || not isPublic)
    |> List.collect (fun contact ->
        match contact.ContactType with
        | Website ->
            [   i [ _class "mdi mdi-sm mdi-web" ] []; txt " "
                a [ _href contact.Value; _target "_blank"; _rel "noopener"; _class "me-4" ] [
                    str (defaultArg contact.Name "Website")
                ]
            ]
        | Email ->
            [   i [ _class "mdi mdi-sm mdi-email-outline" ] []; txt " "
                a [ _href $"mailto:{contact.Value}"; _class "me-4" ] [ str (defaultArg contact.Name "E-mail") ]
            ]
        | Phone ->
            [   span [ _class "me-4" ] [
                    i [ _class "mdi mdi-sm mdi-phone" ] []; txt " "; str contact.Value
                    match contact.Name with Some name -> str $" ({name})" | None -> ()
                ]
            ])

/// Display a citizen's contact information
let contactInfoPrint citizen isPublic =
    citizen.OtherContacts
    |> List.filter (fun it -> (isPublic && it.IsPublic) || not isPublic)
    |> List.collect (fun contact ->
        match contact.ContactType with
        | Website ->
            [   i [ _class "mdi mdi-sm mdi-web" ] []; txt " "; str (defaultArg contact.Name "Website"); txt " &ndash; "
                str contact.Value; br []
            ]
        | Email ->
            [   i [ _class "mdi mdi-sm mdi-email-outline" ] []; txt " "; str (defaultArg contact.Name "E-mail")
                txt " &ndash; "; str contact.Value; br []
            ]
        | Phone ->
            [   span [ _class "me-4" ] [
                    i [ _class "mdi mdi-sm mdi-phone" ] []; rawText " "
                    match contact.Name with Some name -> str name; txt " &ndash; " | None -> ()
                    str contact.Value; br []
                ]
            ])

open NodaTime
open NodaTime.Text

/// Generate a full date in the citizen's local time zone
let fullDate (value : Instant) tz =
    (ZonedDateTimePattern.CreateWithCurrentCulture ("MMMM d, yyyy", DateTimeZoneProviders.Tzdb))
        .Format(value.InZone DateTimeZoneProviders.Tzdb[tz])

/// Generate a full date/time in the citizen's local time
let fullDateTime (value : Instant) tz =
    let dtPattern   = ZonedDateTimePattern.CreateWithCurrentCulture ("MMMM d, yyyy h:mm", DateTimeZoneProviders.Tzdb)
    let amPmPattern = ZonedDateTimePattern.CreateWithCurrentCulture ("tt", DateTimeZoneProviders.Tzdb)
    let tzValue     = value.InZone DateTimeZoneProviders.Tzdb[tz]
    $"{dtPattern.Format(tzValue)}{amPmPattern.Format(tzValue).ToLowerInvariant()}"


/// Layout generation functions
[<RequireQualifiedAccess>]
module Layout =

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

            /// User messages to be displayed
            Messages : string list
        }

    /// Append the application name to the page title
    let private constructTitle ctx =
        seq {
            if ctx.PageTitle <> "" then
                ctx.PageTitle; " | "
            "Jobs, Jobs, Jobs"
        }
        |> Seq.reduce (+)
        |> str
        |> List.singleton
        |> title []

    /// Generate the HTML head tag
    let private htmlHead ctx =
        head [] [
            meta [ _name "viewport"; _content "width=device-width, initial-scale=1" ]
            constructTitle ctx
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
            ] [ i [ _class $"mdi mdi-{icon}"; _ariaHidden "true" ] []; txt text ]
        nav [ _class "jjj-nav" ] [
            if ctx.IsLoggedOn then
                navLink "/citizen/dashboard" "view-dashboard-variant"             "Dashboard"
                navLink "/help-wanted"       "newspaper-variant-multiple-outline" "Help Wanted!"
                navLink "/profile/search"    "view-list-outline"                  "Job Seekers"
                navLink "/success-stories"   "thumb-up"                           "Success Stories"
                div [ _class "separator" ] []
                navLink "/citizen/account" "account-edit" "My Account"
                navLink "/listings/mine"   "sign-text"    "My Job Listings"
                navLink "/profile/edit"    "pencil"       "My Employment Profile"
                div [ _class "separator" ] []
                navLink "/citizen/log-off" "logout-variant" "Log Off"
            else
                navLink "/"               "home"              "Home"
                navLink "/profile/search" "view-list-outline" "Job Seekers"
                navLink "/citizen/log-on" "login-variant"     "Log On"
            navLink "/how-it-works" "help-circle-outline" "How It Works"
        ]

    /// Generate mobile and desktop side navigation areas
    let private sideNavs ctx = [
        div [ _id "mobileMenu"; _class "jjj-mobile-menu offcanvas offcanvas-end"; _tabindex "-1"
              _ariaLabelledBy "mobileMenuLabel" ] [
            div [ _class "offcanvas-header" ] [
                h5 [ _id "mobileMenuLabel" ] [ txt "Menu" ]
                button [
                    _class "btn-close text-reset"; _type "button"; _data "bs-dismiss" "offcanvas"; _ariaLabel "Close"
                ] []
            ]
            div [ _class "offcanvas-body" ] [ links ctx ]
        ]
        aside [ _class "jjj-full-menu d-none d-md-block p-3" ] [
            p [ _class "home-link pb-3" ] [ a [ _href "/" ] [ txt "Jobs, Jobs, Jobs" ] ]
            emptyP
            links ctx
        ]
    ]

    /// Title bars for mobile and desktop
    let private titleBars = [
        nav [ _class "d-flex d-md-none navbar navbar-dark" ] [
            span [ _class "navbar-text" ] [ a [ _href "/" ] [ txt "Jobs, Jobs, Jobs" ] ]
            button [ _class "btn"; _data "bs-toggle" "offcanvas"; _data "bs-target" "#mobileMenu"
                     _ariaControls "mobileMenu" ] [ i [ _class "mdi mdi-menu" ] [] ]
        ]
        nav [ _class "d-none d-md-flex navbar navbar-light bg-light"] [
            span [] [ txt "&nbsp;" ]
            span [ _class "navbar-text" ] [
                txt "(&hellip;and Jobs &ndash; "; audioClip "pelosi-jobs" (txt "Let&rsquo;s Vote for Jobs!"); txt ")"
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
                txt $"Jobs, Jobs, Jobs v{version} &bull; "
                a [ _href "/privacy-policy" ] [ txt "Privacy Policy" ]; txt " &bull; "
                a [ _href "/terms-of-service" ] [ txt "Terms of Service" ]
            ]
        ]

    /// Render any messages
    let private messages ctx =
        ctx.Messages
        |> List.map (fun msg ->
            let parts   = msg.Split "|||"
            let level   = if parts[0] = "error" then "danger" else parts[0]
            let message = parts[1]
            div [ _class $"alert alert-{level} alert-dismissable fade show d-flex justify-content-between p-2 mb-1 mt-1"
                  _roleAlert ] [
                p [ _class "mb-0" ] [
                    if level <> "success" then strong [] [ txt $"{parts[0].ToUpperInvariant ()}: " ]
                    txt message
                ]
                button [ _type "button"; _class "btn-close"; _data "bs-dismiss" "alert"; _ariaLabel "Close" ] []
            ])
        |> div [ _id "alerts" ]
        
    /// Create a full view
    let full ctx =
        html [ _lang "en" ] [
            htmlHead ctx
            body [] [
                div [ _class "jjj-app"; _hxBoost; _hxTarget "this" ] [
                    yield! sideNavs ctx
                    div [ _class "jjj-main" ] [
                        yield! titleBars
                        main [ _class "jjj-content container-fluid" ] [
                            messages ctx
                            ctx.Content
                        ]
                        htmlFoot
                    ]
                ]
                Script.minified
                script [ _async
                         _src         "https://cdn.jsdelivr.net/npm/bootstrap@5.2.0/dist/js/bootstrap.bundle.min.js"
                         _integrity   "sha384-A3rJD856KowSb7dwlZdYEkO39Gagi7vIsF0jrRAoQmDKKtQBHUuLZ9AsSv4jD4Xa"
                         _crossorigin "anonymous" ] []
                script [ _src "/script.js" ] []
                template [ _id "alertTemplate" ] [
                    div [ _class $"alert alert-dismissable fade show d-flex justify-content-between p-2 mb-1 mt-1"
                          _roleAlert ] [
                        p [ _class "mb-0" ] []
                        button [ _type "button"; _class "btn-close"; _data "bs-dismiss" "alert"; _ariaLabel "Close" ] []
                    ]
                ]
            ]
        ]

    /// Create a partial (boosted response) view
    let partial ctx =
        html [ _lang "en" ] [
            head [] [
                constructTitle ctx
            ]
            body [] [
                yield! sideNavs ctx
                div [ _class "jjj-main" ] [
                    yield! titleBars
                    main [ _class "jjj-content container-fluid" ] [
                        messages ctx
                        ctx.Content
                    ]
                    htmlFoot
                ]
            ]
        ]
    
    /// Render a print view (styles, but no other layout)
    let print ctx =
        html [ _lang "en" ] [
            htmlHead ctx
            body [ _class "m-1" ] [ ctx.Content ]
        ]
    
    /// Render a bare view (used for components)
    let bare ctx =
        html [ _lang "en" ] [
            head [] [ title [] [] ]
            body [] [ ctx.Content ]
        ]
