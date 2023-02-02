/// Handlers for the home page, legal stuff, and help
module JobsJobsJobs.Home.Handlers

open Giraffe
open JobsJobsJobs.Common.Handlers

// GET: /
let home : HttpHandler =
    renderHandler "Welcome" Views.home

// GET: /privacy-policy
let privacyPolicy : HttpHandler =
    renderHandler "Privacy Policy" Views.privacyPolicy

// GET: /terms-of-service
let termsOfService : HttpHandler =
    renderHandler "Terms of Service" Views.termsOfService

// GET: /how-it-works
let howItWorks : HttpHandler =
    renderHandler "How It Works" Views.Help.index

// GET: /how-it-works/accounts
let accountHelp : HttpHandler =
    renderHandler "Accounts | How It Works" Views.Help.accounts

// GET: /how-it-works/listings
let listingHelp : HttpHandler =
    renderHandler "Job Listings | How It Works" Views.Help.listings

// GET: /how-it-works/profiles
let profileHelp : HttpHandler =
    renderHandler "Employment Profiles | How It Works" Views.Help.profiles


open Giraffe.EndpointRouting

/// All endpoints for this feature
let endpoints =
    [   GET_HEAD [
            route "/"                 home
            route "/privacy-policy"   privacyPolicy
            route "/terms-of-service" termsOfService
        ]
        subRoute "/how-it-works" [
            GET_HEAD [
                route ""          howItWorks
                route "/accounts" accountHelp
                route "/listings" listingHelp
                route "/profiles" profileHelp
            ]
        ]
    ]
