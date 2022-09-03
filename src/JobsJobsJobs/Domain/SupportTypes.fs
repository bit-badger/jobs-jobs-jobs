namespace JobsJobsJobs.Domain

open System
open Giraffe

/// The ID of a user (a citizen of Gitmo Nation)
type CitizenId = CitizenId of Guid

/// Support functions for citizen IDs
module CitizenId =
    
    /// Create a new citizen ID
    let create () = (Guid.NewGuid >> CitizenId) ()
    
    /// A string representation of a citizen ID
    let toString = function CitizenId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a citizen ID
    let ofString = ShortGuid.toGuid >> CitizenId
    
    /// Get the GUID value of a citizen ID
    let value = function CitizenId guid -> guid


/// The ID of a continent
type ContinentId = ContinentId of Guid

/// Support functions for continent IDs
module ContinentId =
    
    /// Create a new continent ID
    let create () = (Guid.NewGuid >> ContinentId) ()
    
    /// A string representation of a continent ID
    let toString = function ContinentId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a continent ID
    let ofString = ShortGuid.toGuid >> ContinentId
    
    /// Get the GUID value of a continent ID
    let value = function ContinentId guid -> guid


/// The ID of a job listing
type ListingId = ListingId of Guid

/// Support functions for listing IDs
module ListingId =
    
    /// Create a new job listing ID
    let create () = (Guid.NewGuid >> ListingId) ()
    
    /// A string representation of a listing ID
    let toString = function ListingId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a listing ID
    let ofString = ShortGuid.toGuid >> ListingId
    
    /// Get the GUID value of a listing ID
    let value = function ListingId guid -> guid


/// A string of Markdown text
type MarkdownString = Text of string

/// Support functions for Markdown strings
module MarkdownString =
    
    open Markdig
    
    /// The Markdown conversion pipeline (enables all advanced features)
    let private pipeline = MarkdownPipelineBuilder().UseAdvancedExtensions().Build ()
    
    /// Convert this Markdown string to HTML
    let toHtml = function Text text -> Markdown.ToHtml (text, pipeline)
    
    /// Convert a Markdown string to its string representation
    let toString = function Text text -> text


/// Types of contacts supported by Jobs, Jobs, Jobs
type ContactType =
    /// E-mail addresses
    | Email
    /// Phone numbers (home, work, cell, etc.)
    | Phone
    /// Websites (personal, social, etc.)
    | Website


/// Another way to contact a citizen from this site 
type OtherContact =
    {   /// The type of contact
        ContactType : ContactType
        
        /// The name of the contact (Email, No Agenda Social, LinkedIn, etc.) 
        Name : string option
        
        /// The value for the contact (e-mail address, user name, URL, etc.)
        Value : string
    }


/// The ID of a skill
type SkillId = SkillId of Guid

/// Support functions for skill IDs
module SkillId =
    
    /// Create a new skill ID
    let create () = (Guid.NewGuid >> SkillId) ()
    
    /// A string representation of a skill ID
    let toString = function SkillId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a skill ID
    let ofString = ShortGuid.toGuid >> SkillId
    
    /// Get the GUID value of a skill ID
    let value = function SkillId guid -> guid


/// The ID of a success report
type SuccessId = SuccessId of Guid

/// Support functions for success report IDs
module SuccessId =
    
    /// Create a new success report ID
    let create () = (Guid.NewGuid >> SuccessId) ()
    
    /// A string representation of a success report ID
    let toString = function SuccessId it -> ShortGuid.fromGuid it
    
    /// Parse a string into a success report ID
    let ofString = ShortGuid.toGuid >> SuccessId
    
    /// Get the GUID value of a success ID
    let value = function SuccessId guid -> guid
