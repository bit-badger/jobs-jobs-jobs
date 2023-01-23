module JobsJobsJobs.Home.Views

open Giraffe.ViewEngine
open JobsJobsJobs.Common.Views

/// The home page
let home =
    article [] [
        emptyP
        p [] [
            txt "Welcome to Jobs, Jobs, Jobs (AKA No Agenda Careers), where citizens of Gitmo Nation can assist one "
            txt "another in finding employment. This will enable them to continue providing value-for-value to Adam "
            txt "and John, as they continue their work deconstructing the misinformation that passes for news on a "
            txt "day-to-day basis."
        ]
        p [] [
            txt "Do you not understand the terms in the paragraph above? No worries; just head over to "
            a [ _href "https://noagendashow.net"; _target "_blank"; _rel "noopener" ] [
                txt "The Best Podcast in the Universe"
            ]
            txt " "; em [] [ audioClip "thats-true" (txt "(that&rsquo;s true!)") ]
            txt " and find out what you&rsquo;re missing."
        ]
    ]

/// Online help / documentation
let howItWorks =
    let linkedPage = _class "badge text-light bg-success"
    let actionButton = _class "badge text-light bg-secondary rounded-pill"
    let mainHeading = _class "border-top border-2 mt-4 pt-1"
    pageWithTitle "How It Works" [
        h5 [ _class "pb-3 text-muted fst-italic" ] [ txt "Last Updated January 22<sup>nd</sup>, 2023" ]
        p [ _class "fst-italic" ] [
            txt "Show me how to "; a [ _href "#listing-search" ] [ txt "find a job" ]; txt " &bull; "
            a [ _href "#listing" ] [ txt "list a job opportunity" ]; txt " &bull; "
            a [ _href "#profile-search" ] [ txt "find people to hire" ]; txt " &bull; "
            a [ _href "#profile" ] [ txt "create an employment profile" ]
        ]

        h4 [ _id "listing-search"; mainHeading] [ txt "Find a Job Listing" ]
        p [] [
            txt "Active job listings are found on the "; span [ linkedPage ] [ txt "Help Wanted!" ]; txt " page. When "
            txt "you first bring up this page, you will see several criteria by which you can narrow your results, "
            txt "though none are required. When you click the "; span [ actionButton ] [ txt "Search" ]; txt " button, "
            txt "you will see open job listings filtered by whatever criteria you specified. Each job  displays its "
            txt "title, its location, whether it is a remote opportunity, and (if specified) the date by which the job "
            txt "needs to be filled."
        ]
        p [] [
            txt "Clicking the "; span [ linkedPage] [ txt "View" ]; txt " link on a listing brings up the full view "
            txt "page for a listing. This page displays all of the information from the search results, along with the "
            txt "citizen who posted it, and the full details of the job. All the citizen&rsquo;s contact information "
            txt "is displayed, and you can use any of these means to get in touch with them to inquire about the "
            txt "position."
        ]

        h4 [ _id "listing"; mainHeading ] [ txt "Job Listings" ]
        h5 [] [ txt "Create a Job Listing" ]
        p [] [
            txt "The "; span [ linkedPage] [ txt "My Job Listings" ]; txt " page shows all of the job listings you "
            txt "have created. To add a new one, click the "; span [ actionButton] [ txt "Add a New Listing" ]
            txt " button. This page allows you to specify a title for the listing; the continent and region; whether "
            txt "it is a remote opportunity; the date by which a job needs to be filled; and a full description of the "
            txt "position, using "; a [ _href "#markdown" ] [ txt "Markdown" ]; txt ". Once you save the listing, it "
            txt "will be visible to the other citizens here."
        ]

        h5 [] [ txt "Maintain and Share Your Job Listings" ]
        p [] [
            txt "The "; span [ linkedPage] [ txt "My Job Listings" ]; txt " page will show you all of your active job "
            txt "listings just below the "; span [ actionButton] [ txt "Add a Job Listing" ]; txt " button. Within "
            txt "this table, you can edit the listing, view it, or expire it (more on that below). The "
            span [ linkedPage] [ txt "View" ]; txt " link will show you the job listing just as other users will see "
            txt "it. You can share the link from your browser, and Jobs, Jobs, Jobs users will be able to log on and "
            txt "view it."
        ]

        h5 [] [ txt "Expire a Job Listing" ]
        p [] [
            txt "Once the job is filled, or the opportunity has passed, you will want to expire the listing; this is "
            txt "what the "; span [ linkedPage] [ txt "Expire" ]; txt " link allows you to do. When you click it, you "
            txt "will be presented with a single question &ndash; was the job filled due to its listing here? If not, "
            txt "leave that blank, click the "; span [ actionButton] [ txt "Expire" ]; txt " button, and the listing "
            txt "will be expired. If you click that box, though, another Markdown editor will appear, where you can "
            txt "share a story of the experience. This is not required, but if you put text there, it will be recorded "
            txt "as a Success Story, and other users will be able to read about your success."
        ]
        p [] [
            txt "Once you have at least one expired job listing, the "; span [ linkedPage] [ txt "My Job Listing" ]
            txt " page will have a new section below your active listings, where you can see your expired ones. You "
            txt "can still view the expired listing, and links that you may have shared will still pull up the "
            txt "listing; there will be an &ldquo;expired&rdquo; label beside the title, so that whoever is viewing it "
            txt "knows that they are reading about a job that is no longer available."
        ]

        h4 [ _id "profile-search"; mainHeading ] [ txt "Searching Profiles" ]
        p [] [
            txt "The "; span [ linkedPage] [ txt "Employment Profiles"]; txt " link at the side allows you to search "
            txt "for profiles by continent, the citizen&rsquo;s desire for remote work, a skill, or any text in their "
            txt "professional biography and experience. If you find someone with whom you&rsquo;d like to discuss "
            txt "potential opportunities, their contact information is displayed below their name at the top of the "
            txt "profile."
        ]

        h4 [ _id "profile"; mainHeading ] [ txt "Your Employment Profile" ]
        // TODO: this is substantially different
        // p [] [
        //     The employment profile is your r&eacute;sum&eacute;, visible to other citizens here. It also allows you to specify
        //     your real name, if you so desire; if that is filled in, that is how you will be identified in search results,
        //     profile views, etc. If not, you will be identified as you are on your Mastodon instance; this system updates your
        //     current display name each time you log on.
        // ]

        // h5 Completing Your Profile
        // p.
        //     The #[span.link My Employment Profile] page lets you establish or modify your employment profile; the
        //     #[span.link Dashboard] page also has buttons that let you create, edit, and view your profile.
        // ul
        //     li.
        //     The #[span.link Professional Biography] is the &ldquo;Objective&rdquo; part of a traditional r&eacute;sum&eacute;.
        //     This section supports #[a(href="#markdown") Markdown], so you can include actual headings, formatting, etc.
        //     li.
        //     Skills are optional, but they are the place to record skills you have. Along with each skill, there is a
        //     #[span.link Notes] field, which can be used to indicate the time you&rsquo;ve practiced a particular skill, the
        //     mastery you have of that skill, etc. It is free-form text, so it is all up to you how you utilize the field.
        //     li.
        //     The #[span.link Experience] field is intended to capture a chronological or topical employment history. This
        //     Markdown space can be used to capture chronological history, certifications, or any other information &ndash;
        //     however you would like it presented to fellow citizens.
        //     #[em.text-muted (If you would like a chronological job builder, reach out and let us know.)]
        //     li.
        //     If you check the #[span.link Allow my profile to be searched publicly] checkbox #[strong and] you are seeking
        //     employment, your continent, region, and skills fields will be searchable and displayed to public users of the
        //     site. They will not be tied to your Mastodon handle or real name; they are there to let people peek behind the
        //     curtain a bit, and hopefully inspire them to join us.

        // h5 Viewing and Sharing Your Profile
        // p.
        //     Once your profile has been established, the #[span.link My Employment Profile] page will have a button at the bottom
        //     that will let you view your profile the way all other validated users will be able to see it. (There will also be a
        //     link to this page from the #[span.link Dashboard].) The URL of this page can be shared on any No Agenda-affiliated
        //     Mastodon instance, if you would like to share it there. Just as with job listings, existing users will go straight
        //     there, while others will get there once they authorize this application.
        // p.
        //     The name on employment profiles is a link to that user&rsquo;s profile on their Mastodon instance; from there,
        //     others can communicate further with you using the tools Mastodon provides.

        // h5 &ldquo;I Found a Job!&rdquo;
        // p.
        //     If your profile indicates that you are seeking employment, and you secure employment, that is something you will
        //     want to update (and &ndash; congratulations!). From both the #[span.link Dashboard] and
        //     #[span.link My Employment Profile] pages, you will see a link that encourages you to tell us about it. Click either
        //     of those links, and you will be brought to a page that allows you to indicate whether your employment actually came
        //     from someone finding your profile on Jobs, Jobs, Jobs, and gives you a place to write about the experience. These
        //     stories are only viewable by validated users, so feel free to use as much (or as little) identifying information as
        //     you&rsquo;d like. You can also submit this page with all the fields blank; in that case, your &ldquo;Seeking
        //     Employment&rdquo; flag is cleared, and the blank story is recorded.
        // p.
        //     As a validated user, you can also view others success stories. Clicking #[span.link Success Stories] in the sidebar
        //     will display a list of all the stories that have been recorded. If there is a story to be read, there will be a link
        //     to read it; if you submitted the story, there will also be an #[span.link Edit] link.

        // h5 Publicly Available Information
        // p.
        //     The #[span.link Job Seekers] page for profile information will allow users to search for and display the continent,
        //     region, skills, and notes of users who are seeking employment #[strong and] have opted in to their information being
        //     publicly searchable. If you are a public user, this information is always the latest we have; check out the link at
        //     the top of the search results for how you can learn more about these fine human resources!

        h4 [ _id "markdown"; mainHeading ] [ txt "A Bit about Markdown" ]
        p [] [
            txt "Markdown is a plain-text way to specify formatting quite similar to that provided by word processors. "
            txt "The "
            a [ _href "https://daringfireball.net/projects/markdown/"; _target "_blank"; _rel "noopener" ] [
                txt "original page"
            ]; txt " for the project is a good overview of its capabilities, and the pages at "
            a [ _href "https://www.markdownguide.org/"; _target "_blank"; _rel "noopener" ] [ txt "Markdown Guide" ]
            txt " give in-depth lessons to make the most of this language. The version of Markdown employed here "
            txt "supports many popular extensions, include smart quotes (turning &quot;a quote&quot; into &ldquo;a "
            txt "quote&rdquo;), tables, super/subscripts, and more."
        ]

        h4 [ mainHeading ] [ txt "Help / Suggestions" ]
        p [] [
            txt "This is open-source software "
            a [ _href "https://github.com/bit-badger/jobs-jobs-jobs"; _target "_blank"; _rel "noopener" ] [
                txt "developed on Github"
            ]; txt "; feel free to "
            a [ _href "https://github.com/bit-badger/jobs-jobs-jobs/issues"; _target "_blank"; _rel "noopener" ] [
                txt "create an issue there"
            ]; txt ", or look up @danieljsummers on No Agenda Social."
        ]
    ]

/// The privacy policy
let privacyPolicy =
    let appName = txt "Jobs, Jobs, Jobs"
    article [] [
        h3 [] [ txt "Privacy Policy" ]
        p [ _class "fst-italic" ] [ txt "(as of December 27<sup>th</sup>, 2022)" ]

        p [] [
            appName; txt " (&ldquo;we,&rdquo; &ldquo;our,&rdquo; or &ldquo;us&rdquo;) is committed to protecting your "
            txt "privacy. This Privacy Policy explains how your personal information is collected, used, and disclosed "
            txt "disclosed by "; appName; txt "."
        ]
        p [] [
            txt "This Privacy Policy applies to our website, and its associated subdomains (collectively, our "
            txt "&ldquo;Service&rdquo;) alongside our application, "; appName; rawText ". By accessing or using our "
            txt "Service, you signify that you have read, understood, and agree to our collection, storage, use, and "
            txt "disclosure of your personal information as described in this Privacy Policy and our Terms of Service."
        ]

        h4 [] [ txt "Definitions and key terms" ]
        p [] [
            txt "To help explain things as clearly as possible in this Privacy Policy, every time any of these terms "
            txt "are referenced, are strictly defined as:"
        ]
        ul [] [
            li [] [
                txt "Cookie: small amount of data generated by a website and saved by your web browser. It is used to "
                txt "identify your browser, provide analytics, remember information about you such as your language "
                txt "preference or login information."
            ]
            li [] [
                txt "Company: when this policy mentions &ldquo;Company,&rdquo; &ldquo;we,&rdquo; &ldquo;us,&rdquo; or "
                txt "&ldquo;our,&rdquo; it refers to "; appName; txt ", that is responsible for your information under "
                txt "this Privacy Policy."
            ]
            li [] [
                txt "Country: where "; appName; txt " or the owners/founders of "; appName
                txt " are based, in this case is US."
            ]
            li [] [
                txt "Customer: refers to the company, organization or person that signs up to use the "; appName
                txt " Service to manage the relationships with your consumers or service users."
            ]
            li [] [
                txt "Device: any internet connected device such as a phone, tablet, computer or any other device that "
                txt "can be used to visit "; appName; txt " and use the services."
            ]
            li [] [
                txt "IP address: Every device connected to the Internet is assigned a number known as an Internet "
                txt "protocol (IP) address. These numbers are usually assigned in geographic blocks. An IP address can "
                txt "often be used to identify the location from which a device is connecting to the Internet."
            ]
            li [] [
                txt "Personnel: refers to those individuals who are employed by "; appName; txt " or are under "
                txt "contract to perform a service on behalf of one of the parties."
            ]
            li [] [
                txt "Personal Data: any information that directly, indirectly, or in connection with other information "
                txt "— including a personal identification number — allows for the identification or identifiability "
                txt "of a natural person."
            ]
            li [] [
                txt "Service: refers to the service provided by "; appName; txt " as described in the relative terms "
                txt "(if available) and on this platform."
            ]
            li [] [
                txt "Third-party service: refers to advertisers, contest sponsors, promotional and marketing partners, "
                txt "and others who provide our content or whose products or services we think may interest you."
            ]
            li [] [
                txt "Website: "; appName; txt "&rsquo;s site, which can be accessed via this URL: "
                a [ _href "/" ] [ txt "https://noagendacareers.com/" ]
            ]
            li [] [
                txt "You: a person or entity that is registered with "; appName; txt " to use the Services."
            ]
        ]

        h4 [] [ txt "What Information Do We Collect?" ]
        p [] [
            txt "We collect information from you when you visit our website, register on our site, or fill out a form."
        ]
        ul [] [
            li [] [ txt "Name / Username" ]
            li [] [ txt "Coarse Geographic Location" ]
            li [] [ txt "Employment History" ]
            li [] [ txt "Job Listing Information" ]
        ]

        h4 [] [ txt "How Do We Use The Information We Collect?" ]
        p [] [ txt "Any of the information we collect from you may be used in one of the following ways:" ]
        ul [] [
            li [] [
                txt "To personalize your experience (your information helps us to better respond to your individual "
                txt "needs)"
            ]
            li [] [
                txt "To improve our website (we continually strive to improve our website offerings based on the "
                txt "information and feedback we receive from you)"
            ]
            li [] [
                txt "To improve customer service (your information helps us to more effectively respond to your "
                txt "customer service requests and support needs)"
            ]
        ]

        h4 [] [ txt "When does "; appName; txt " use end user information from third parties?" ]
        p [] [
            appName; txt " will collect End User Data necessary to provide the "; appName
            txt " services to our customers."
        ]
        p [] [
            txt "End users may voluntarily provide us with information they have made available on social media "
            txt "websites. If you provide us with any such information, we may collect publicly available information "
            txt "from the social media websites you have indicated. You can control how much of your information "
            txt "social media websites make public by visiting these websites and changing your privacy settings."
        ]

        h4 [] [ txt "When does "; appName; txt " use customer information from third parties?" ]
        p [] [ txt "We do not utilize third party information apart from the end-user data described above." ]

        h4 [] [ txt "Do we share the information we collect with third parties?" ]
        p [] [
            txt "We may disclose personal and non-personal information about you to government or law enforcement "
            txt "officials or private parties as we, in our sole discretion, believe necessary or appropriate in order "
            txt "to respond to claims, legal process (including subpoenas), to protect our rights and interests or "
            txt "those of a third party, the safety of the public or any person, to prevent or stop any illegal, "
            txt "unethical, or legally actionable activity, or to otherwise comply with applicable court orders, laws, "
            txt "rules and regulations."
        ]

        h4 [] [ txt "Where and when is information collected from customers and end users?" ]
        p [] [
            appName; txt " will collect personal information that you submit to us. We may also receive personal "
            txt "information about you from third parties as described above."
        ]

        h4 [] [ txt "How Do We Use Your E-mail Address?" ]
        p [] [
            appName; txt " uses your e-mail address to identify you, along with your password, as an authorized user "
            txt "of this site. E-mail addresses are verified via a time-sensitive link, and may also be used to send "
            txt "password reset authorization codes. We do not display this e-mail address to users. If you choose to "
            txt "add an e-mail address as a contact type, that e-mail address will be visible to other authorized "
            txt "users."
        ]

        h4 [] [ txt "How Long Do We Keep Your Information?" ]
        p [] [
            txt "We keep your information only so long as we need it to provide "; appName; txt " to you and fulfill "
            txt "the purposes described in this policy. When we no longer need to use your information and there is no "
            txt "need for us to keep it to comply with our legal or regulatory obligations, we’ll either remove it "
            txt "from our systems or depersonalize it so that we can&rsquo;t identify you."
        ]

        h4 [] [ txt "How Do We Protect Your Information?" ]
        p [] [
            txt "We implement a variety of security measures to maintain the safety of your personal information when "
            txt "you enter, submit, or access your personal information. We mandate the use of a secure server. We "
            txt "cannot, however, ensure or warrant the absolute security of any information you transmit to "; appName
            txt " or guarantee that your information on the Service may not be accessed, disclosed, altered, or "
            txt "destroyed by a breach of any of our physical, technical, or managerial safeguards."
        ]

        h4 [] [ txt "Could my information be transferred to other countries?" ]
        p [] [
            appName; txt " is hosted in the US. Information collected via our website may be viewed and hosted "
            txt "anywhere in the world, including countries that may not have laws of general applicability regulating "
            txt "the use and transfer of such data. To the fullest extent allowed by applicable law, by using any of "
            txt "the above, you voluntarily consent to the trans-border transfer and hosting of such information."
        ]

        h4 [] [ txt "Is the information collected through the "; appName; txt " Service secure?" ]
        p [] [
            txt "We take precautions to protect the security of your information. We have physical, electronic, and "
            txt "managerial procedures to help safeguard, prevent unauthorized access, maintain data security, and "
            txt "correctly use your information. However, neither people nor security systems are foolproof, including "
            txt "encryption systems. In addition, people can commit intentional crimes, make mistakes, or fail to "
            txt "follow policies. Therefore, while we use reasonable efforts to protect your personal information, we "
            txt "cannot guarantee its absolute security. If applicable law imposes any non-disclaimable duty to "
            txt "protect your personal information, you agree that intentional misconduct will be the standards used "
            txt "to measure our compliance with that duty."
        ]

        h4 [] [ txt "Can I update or correct my information?" ]
        p [] [
            txt "The rights you have to request updates or corrections to the information "; appName
            txt " collects depend on your relationship with "; appName; txt "."
        ]
        p [] [
            txt "Customers have the right to request the restriction of certain uses and disclosures of personally "
            txt "identifiable information as follows. You can contact us in order to (1) update or correct your "
            txt "personally identifiable information, or (3) delete the personally identifiable information maintained "
            txt "about you on our systems (subject to the following paragraph), by cancelling your account. Such "
            txt "updates, corrections, changes and deletions will have no effect on other information that we maintain "
            txt "in accordance with this Privacy Policy prior to such update, correction, change, or deletion. You are "
            txt "responsible for maintaining the secrecy of your unique password and account information at all times."
        ]
        p [] [
            appName; txt " also provides ways for users to modify or remove the information we have collected from "
            txt "them from the application; these actions will have the same effect as contacting us to modify or "
            txt "remove data."
        ]
        p [] [
            txt "You should be aware that it is not technologically possible to remove each and every record of the "
            txt "information you have provided to us from our system. The need to back up our systems to protect "
            txt "information from inadvertent loss means that a copy of your information may exist in a non-erasable "
            txt "form that will be difficult or impossible for us to locate. Promptly after receiving your request, "
            txt "all personal information stored in databases we actively use, and other readily searchable media will "
            txt "be updated, corrected, changed, or deleted, as appropriate, as soon as and to the extent reasonably "
            txt "and technically practicable."
        ]
        p [] [
            txt "If you are an end user and wish to update, delete, or receive any information we have about you, you "
            txt "may do so by contacting the organization of which you are a customer."
        ]

        h4 [] [ txt "Governing Law" ]
        p [] [
            txt "This Privacy Policy is governed by the laws of US without regard to its conflict of laws provision. "
            txt "You consent to the exclusive jurisdiction of the courts in connection with any action or dispute "
            txt "arising between the parties under or in connection with this Privacy Policy except for those "
            txt "individuals who may have rights to make claims under Privacy Shield, or the Swiss-US framework."
        ]
        p [] [
            txt "The laws of US, excluding its conflicts of law rules, shall govern this Agreement and your use of the "
            txt "website. Your use of the website may also be subject to other local, state, national, or "
            txt "international laws."
        ]
        p [] [
            txt "By using "; appName; txt " or contacting us directly, you signify your acceptance of this Privacy "
            txt "Policy. If you do not agree to this Privacy Policy, you should not engage with our website, or use "
            txt "our services. Continued use of the website, direct engagement with us, or following the posting of "
            txt "changes to this Privacy Policy that do not significantly affect the use or disclosure of your "
            txt "personal information will mean that you accept those changes."
        ]

        h4 [] [ txt "Your Consent" ]
        p [] [
            txt "We&rsquo;ve updated our Privacy Policy to provide you with complete transparency into what is being "
            txt "set when you visit our site and how it&rsquo;s being used. By using our website, registering an "
            txt "account, or making a purchase, you hereby consent to our Privacy Policy and agree to its terms."
        ]

        h4 [] [ txt "Links to Other Websites" ]
        p [] [
            txt "This Privacy Policy applies only to the Services. The Services may contain links to other websites "
            txt "not operated or controlled by "; appName; txt ". We are not responsible for the content, accuracy, or "
            txt "opinions expressed in such websites, and such websites are not investigated, monitored, or checked "
            txt "for accuracy or completeness by us. Please remember that when you use a link to from the Services to "
            txt "another website, our Privacy Policy is no longer in effect. Your browsing and interaction on any "
            txt "other website, including those that have a link on our platform, is subject to that website’s own "
            txt "rules and policies. Such third parties may use their own cookies or other methods to collect "
            txt "information about you."
        ]

        h4 [] [ txt "Cookies" ]
        p [] [
            appName; txt " uses a session Cookie to identify an active, logged-on session. This Cookie is removed when "
            txt "when You explicitly log off; is not accessible via script; and must be transferred over a secured, "
            txt "encrypted connection."
        ]
        p [] [ appName; txt " uses no persistent or Third-Party Cookies." ]

        h4 [] [ txt "Kids&rsquo; Privacy" ]
        p [] [
            txt "We do not address anyone under the age of 13. We do not knowingly collect personally identifiable "
            txt "information from anyone under the age of 13. If You are a parent or guardian and You are aware that "
            txt "Your child has provided Us with Personal Data, please contact Us. If We become aware that We have "
            txt "collected Personal Data from anyone under the age of 13 without verification of parental consent, We "
            txt "take steps to remove that information from Our servers."
        ]

        h4 [] [ txt "Changes To Our Privacy Policy" ]
        p [] [
            txt "We may change our Service and policies, and we may need to make changes to this Privacy Policy so "
            txt "that they accurately reflect our Service and policies. Unless otherwise required by law, we will "
            txt "notify you (for example, through our Service) before we make changes to this Privacy Policy and give "
            txt "you an opportunity to review them before they go into effect. Then, if you continue to use the "
            txt "Service, you will be bound by the updated Privacy Policy. If you do not want to agree to this or any "
            txt "updated Privacy Policy, you can delete your account."
        ]

        h4 [] [ txt "Third-Party Services" ]
        p [] [
            txt "We may display, include or make available third-party content (including data, information, "
            txt "applications and other products services) or provide links to third-party websites or services "
            txt "(&ldquo;Third-Party Services&rdquo;)."
        ]
        p [] [
            txt "You acknowledge and agree that "; appName; txt " shall not be responsible for any Third-Party "
            txt "Services, including their accuracy, completeness, timeliness, validity, copyright compliance, "
            txt "legality, decency, quality or any other aspect thereof. "; appName; txt " does not assume and shall "
            txt "not have any liability or responsibility to you or any other person or entity for any Third-Party "
            txt "Services."
        ]
        p [] [
            txt "Third-Party Services and links thereto are provided solely as a convenience to you and you access and "
            txt "use them entirely at your own risk and subject to such third parties&rsquo; terms and conditions."
        ]

        h4 [] [ txt "Tracking Technologies" ]
        p [] [ appName; txt " does not use any tracking technologies." ]

        h4 [] [ txt "Information about General Data Protection Regulation (GDPR)" ]
        p [] [
            txt "We may be collecting and using information from you if you are from the European Economic Area (EEA), "
            txt "and in this section of our Privacy Policy we are going to explain exactly how and why is this data "
            txt "collected, and how we maintain this data under protection from being replicated or used in the wrong "
            txt "way."
        ]

        h5 [] [ txt "What is GDPR?" ]
        p [] [
            txt "GDPR is an EU-wide privacy and data protection law that regulates how EU residents&rsquo; data is "
            txt "protected by companies and enhances the control the EU residents have, over their personal data."
        ]
        p [] [
            txt "The GDPR is relevant to any globally operating company and not just the EU-based businesses and EU "
            txt "residents. Our customers’ data is important irrespective of where they are located, which is why we "
            txt "have implemented GDPR controls as our baseline standard for all our operations worldwide."
        ]

        h5 [] [ txt "What is personal data?" ]
        p [] [
            txt "Any data that relates to an identifiable or identified individual. GDPR covers a broad spectrum of "
            txt "information that could be used on its own, or in combination with other pieces of information, to "
            txt "identify a person. Personal data extends beyond a person’s name or email address. Some examples "
            txt "include financial information, political opinions, genetic data, biometric data, IP addresses, "
            txt "physical address, sexual orientation, and ethnicity."
        ]
        p [] [ txt "The Data Protection Principles include requirements such as:" ]
        ul [] [
            li [] [
                txt "Personal data collected must be processed in a fair, legal, and transparent way and should only "
                txt "be used in a way that a person would reasonably expect."
            ]
            li [] [
                txt "Personal data should only be collected to fulfil a specific purpose and it should only be used "
                txt "for that purpose. Organizations must specify why they need the personal data when they collect it."
            ]
            li [] [ txt "Personal data should be held no longer than necessary to fulfil its purpose." ]
            li [] [
                txt "People covered by the GDPR have the right to access their own personal data. They can also "
                txt "request a copy of their data, and that their data be updated, deleted, restricted, or moved to "
                txt "another organization."
            ]
        ]

        h5 [] [ txt "Why is GDPR important?" ]
        p [] [
            txt "GDPR adds some new requirements regarding how companies should protect individuals&rsquo; personal "
            txt "data that they collect and process. It also raises the stakes for compliance by increasing "
            txt "enforcement and imposing greater fines for breach. Beyond these facts, it&rsquo;s simply the right "
            txt "thing to do. At "; appName; txt " we strongly believe that your data privacy is very important and we "
            txt "already have solid security and privacy practices in place that go beyond the requirements of this "
            txt "regulation."
        ]

        h5 [] [ txt "Individual Data Subject&rsquo;s Rights - Data Access, Portability, and Deletion" ]
        p [] [
            txt "We are committed to helping our customers meet the data subject rights requirements of GDPR. "
            appName; txt " processes or stores all personal data in fully vetted, DPA compliant vendors. We do store "
            txt "all conversation and personal data for up to 6 years unless your account is deleted. In which case, "
            txt "we dispose of all data in accordance with our Terms of Service and Privacy Policy, but we will not "
            txt "hold it longer than 60 days."
        ]
        p [] [
            txt "We are aware that if you are working with EU customers, you need to be able to provide them with the "
            txt "ability to access, update, retrieve and remove personal data. We got you! We&rsquo;ve been set up as "
            txt "self service from the start and have always given you access to your data. Our customer support team "
            txt "is here for you to answer any questions you might have about working with the API."
        ]

        h4 [] [ txt "California Residents" ]
        p [] [
            txt "The California Consumer Privacy Act (CCPA) requires us to disclose categories of Personal Information "
            txt "we collect and how we use it, the categories of sources from whom we collect Personal Information, "
            txt "and the third parties with whom we share it, which we have explained above."
        ]
        p [] [
            txt "We are also required to communicate information about rights California residents have under "
            txt "California law. You may exercise the following rights:"
        ]
        ul [] [
            li [] [
                txt "Right to Know and Access. You may submit a verifiable request for information regarding the: (1) "
                txt "categories of Personal Information we collect, use, or share; (2) purposes for which categories "
                txt "of Personal Information are collected or used by us; (3) categories of sources from which we "
                txt "collect Personal Information; and (4) specific pieces of Personal Information we have collected "
                txt "about you."
            ]
            li [] [
                txt "Right to Equal Service. We will not discriminate against you if you exercise your privacy rights."
            ]
            li [] [
                txt "Right to Delete. You may submit a verifiable request to close your account and we will delete "
                txt "Personal Information about you that we have collected."
            ]
            li [] [
                txt "Request that a business that sells a consumer&rsquo;s personal data, not sell the "
                txt "consumer&rsquo;s personal data."
            ]
        ]
        p [] [
            txt "If you make a request, we have one month to respond to you. If you would like to exercise any of "
            txt "these rights, please contact us."
        ]
        p [] [ txt "We do not sell the Personal Information of our users." ]
        p [] [ txt "For more information about these rights, please contact us." ]

        h5 [] [ txt "California Online Privacy Protection Act (CalOPPA)" ]
        p [] [
            txt "CalOPPA requires us to disclose categories of Personal Information we collect and how we use it, the "
            txt "categories of sources from whom we collect Personal Information, and the third parties with whom we "
            txt "share it, which we have explained above."
        ]
        p [] [ txt "CalOPPA users have the following rights:" ]
        ul [] [
            li [] [
                txt "Right to Know and Access. You may submit a verifiable request for information regarding the: (1) "
                txt "categories of Personal Information we collect, use, or share; (2) purposes for which categories "
                txt "of Personal Information are collected or used by us; (3) categories of sources from which we "
                txt "collect Personal Information; and (4) specific pieces of Personal Information we have collected "
                txt "about you."
            ]
            li [] [
                txt "Right to Equal Service. We will not discriminate against you if you exercise your privacy rights."
            ]
            li [] [
                txt "Right to Delete. You may submit a verifiable request to close your account and we will delete "
                txt "Personal Information about you that we have collected."
            ]
            li [] [
                txt "Right to request that a business that sells a consumer&rsquo;s personal data, not sell the "
                txt "consumer&rsquo;s personal data."
            ]
        ]
        p [] [
            txt "If you make a request, we have one month to respond to you. If you would like to exercise any of "
            txt "these rights, please contact us."
        ]
        p [] [ txt "We do not sell the Personal Information of our users." ]
        p [] [ txt "For more information about these rights, please contact us." ]

        h4 [] [ txt "Contact Us" ]
        p [] [ txt "Don&rsquo;t hesitate to contact us if you have any questions." ]
        ul [] [
            li [] [
                txt "Via this Link: "; a [ _href "/how-it-works" ] [ txt "https://noagendacareers.com/how-it-works" ]
            ]
        ]

        hr []

        p [ _class "fst-italic" ] [ txt "Changes for "; appName; txt " v3 (December 27<sup>th</sup>, 2022)" ]
        ul [] [
            li [ _class "fst-italic" ] [ txt "Removed references to Mastodon" ]
            li [ _class "fst-italic" ] [ txt "Added references to job listings" ]
            li [ _class "fst-italic" ] [ txt "Changed information regarding e-mail addresses" ]
            li [ _class "fst-italic" ] [ txt "Updated cookie / tracking sections for new architecture" ]
        ]
        p [ _class "fst-italic" ] [
            txt "Change on September 6<sup>th</sup>, 2021 &ndash; replaced &ldquo;No Agenda Social&rdquo; with generic "
            txt "terms for any authorized Mastodon instance."
        ]
    ]

/// The page for terms of service
let termsOfService =
    article [] [
        h3 [] [ txt "Terms of Service" ]
        p [ _class "fst-italic" ] [ txt "(as of August 30<sup>th</sup>, 2022)" ]
        h4 [] [ txt "Acceptance of Terms" ]
        p [] [
            txt "By accessing this web site, you are agreeing to be bound by these Terms and Conditions, and that you "
            txt "are responsible to ensure that your use of this site complies with all applicable laws. Your "
            txt "continued use of this site implies your acceptance of these terms."
        ]
        h4 [] [ txt "Description of Service and Registration" ]
        p [] [
            txt "Jobs, Jobs, Jobs is a service that allows individuals to enter and amend employment profiles and job "
            txt "listings, restricting access to the details of these to other users of this site, unless the "
            txt "individual specifies that this information should be visible publicly. See our "
            a [ _href "/privacy-policy" ] [ txt "privacy policy" ]
            txt " for details on the personal (user) information we maintain."
        ]
        h4 [] [ txt "Liability" ]
        p [] [
            txt "This service is provided &ldquo;as is&rdquo;, and no warranty (express or implied) exists. The "
            txt "service and its developers may not be held liable for any damages that may arise through the use of "
            txt "this service."
        ]
        h4 [] [ txt "Updates to Terms" ]
        p [] [
            txt "These terms and conditions may be updated at any time. When these terms are updated, users will be "
            txt "notified via a notice on the dashboard page. Additionally, the date at the top of this page will be "
            txt "updated, and any substantive updates will also be accompanied by a summary of those changes."
        ]
        hr []
        p [] [
            txt "You may also wish to review our "; a [ _href "/privacy-policy" ] [ txt "privacy policy" ]
            txt " to learn how we handle your data."
        ]
        hr []
        p [ _class "fst-italic" ] [
            txt "Change on August 30<sup>th</sup>, 2022 &ndash; added references to job listings, removed references "
            txt "to Mastodon instances."
        ]
        p [ _class "fst-italic" ] [
            txt "Change on September 6<sup>th</sup>, 2021 &ndash; replaced &ldquo;No Agenda Social&rdquo; with a list "
            txt "of all No Agenda-affiliated Mastodon instances."
        ]
    ]
