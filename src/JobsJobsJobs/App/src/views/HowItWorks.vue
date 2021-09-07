<template lang="pug">
article
  page-title(title="How It Works")
  h3 How It Works
  h5.pb-3.text-muted: em Last Updated August 29#[sup th], 2021
  p: em.
    Show me how to #[a(href="#listing-search") find a job]
    #[!= " &bull; "]#[a(href="#listing") list a job opportunity]
    #[!= " &bull; "]#[a(href="#profile-search") find people to hire]
    #[!= " &bull; "]#[a(href="#profile") create an employment profile]

  hr

  h4#listing-search Find a Job Listing
  p.
    Active job listings are found on the #[span.link Help Wanted!] page. When you first bring up this page, you will see
    several criteria by which you can narrow your results, though none are required. When you click the
    #[span.button Search] button, you will see open job listings filtered by whatever criteria you specified. Each job
    displays its title, its location, whether it is a remote opportunity, and (if specified) the date by which the job
    needs to be filled.
  p.
    Clicking the #[span.link View] link on a listing brings up the full view page for a listing. This page displays all
    of the information from the search results, along with the citizen who posted it, and the full details of the job.
    The citizen&rsquo;s name is a link to their profile page at their Mastodon instance; you can use that to get their
    handle, and use Mastodon&rsquo;s communication facilites to inquire about the position.
  p: em.text-muted.
    (If you know of a way to construct a link to Mastodon that would start a direct message, please reach out;
    I&rsquo;ve searched and searched, and asked NAS, but have not yet determined how to do that.)

  hr

  h4#listing Job Listings
  h5 Create a Job Listing
  p.
    The #[span.link My Job Listings] page shows all of the job listings you have created. To add a new one, click the
    #[span.button Add a New Listing] button. This page allows you to specify a title for the listing; the continent and
    region; whether it is a remote opportunity; the date by which a job needs to be filled; and a full description of
    the position, using #[a(href="#markdown") Markdown]. Once you save the listing, it will be visible to the other
    citizens here.

  h5 Maintain and Share Your Job Listings
  p.
    The #[span.link My Job Listings] page will show you all of your active job listings just below the
    #[span.button Add a Job Listing] button. Within this table, you can edit the listing, view it, or expire it (more on
    that below). The #[span.link View] link will show you the job listing just as other users will see it. You can share
    the link from your browser on any No Agenda-affiliated Mastodon instance, and those who click on it will be able to
    view it. (Existing users of Jobs, Jobs, Jobs will go right to it; others will need to authorize this site&rsquo;s
    access, but then they will get there as well.)

  h5 Expire a Job Listing
  p.
    Once the job is filled, or the opportunity has passed, you will want to expire the listing; this is what the
    #[span.link Expire] link allows you to do. When you click it, you will be presented with a single question &ndash;
    was the job filled due to its listing here? If not, leave that blank, click the #[span.button Expire] button, and
    the listing will be expired. If you click that box, though, another Markdown editor will appear, where you can share
    a story of the experience. This is not required, but if you put text there, it will be recorded as a Success Story,
    and other users will be able to read about your success.
  p.
    Once you have at least one expired job listing, the #[span.link My Job Listing] page will have a new section below
    your active listings, where you can see your expired ones. You can still view the expired listing, and links that
    you may have shared will still pull up the listing; there will be an &ldquo;expired&rdquo; label beside the title,
    so that whoever is viewing it knows that they are reading about a job that is no longer available.

  hr

  h4#profile-search Searching Profiles
  p.
    The #[span.link Employment Profiles] link at the side allows you to search for profiles by continent, the
    citizen&rsquo;s desire for remote work, a skill, or any text in their professional biography and experience. If you
    find someone with whom you&rsquo;d like to discuss potential opportunities, the name at the top of the profile links
    to their Mastodon profile, where you can use its features to get in touch.

  hr

  h4#profile Your Employment Profile
  p.
    The employment profile is your r&eacute;sum&eacute;, visible to other citizens here. It also allows you to specify
    your real name, if you so desire; if that is filled in, that is how you will be identified in search results,
    profile views, etc. If not, you will be identified as you are on your Mastodon instance; this system updates your
    current display name each time you log on.

  h5 Completing Your Profile
  p.
    The #[span.link My Employment Profile] page lets you establish or modify your employment profile; the
    #[span.link Dashboard] page also has buttons that let you create, edit, and view your profile.
  ul
    li.
      The #[span.link Professional Biography] is the &ldquo;Objective&rdquo; part of a traditional r&eacute;sum&eacute;.
      This section supports #[a(href="#markdown") Markdown], so you can include actual headings, formatting, etc.
    li.
      Skills are optional, but they are the place to record skills you have. Along with each skill, there is a
      #[span.link Notes] field, which can be used to indicate the time you&rsquo;ve practiced a particular skill, the
      mastery you have of that skill, etc. It is free-form text, so it is all up to you how you utilize the field.
    li.
      The #[span.link Experience] field is intended to capture a chronological or topical employment history. This
      Markdown space can be used to capture chronological history, certifications, or any other information &ndash;
      however you would like it presented to fellow citizens.
      #[em.text-muted (If you would like a chronological job builder, reach out and let us know.)]
    li.
      If you check the #[span.link Allow my profile to be searched publicly] checkbox #[strong and] you are seeking
      employment, your continent, region, and skills fields will be searchable and displayed to public users of the
      site. They will not be tied to your Mastodon handle or real name; they are there to let people peek behind the
      curtain a bit, and hopefully inspire them to join us.

  h5 Viewing and Sharing Your Profile
  p.
    Once your profile has been established, the #[span.link My Employment Profile] page will have a button at the bottom
    that will let you view your profile the way all other validated users will be able to see it. (There will also be a
    link to this page from the #[span.link Dashboard].) The URL of this page can be shared on any No Agenda-affiliated
    Mastodon instance, if you would like to share it there. Just as with job listings, existing users will go straight
    there, while others will get there once they authorize this application.
  p.
    The name on employment profiles is a link to that user&rsquo;s profile on their Mastodon instance; from there,
    others can communicate further with you using the tools Mastodon provides.

  h5 &ldquo;I Found a Job!&rdquo;
  p.
    If your profile indicates that you are seeking employment, and you secure employment, that is something you will
    want to update (and &ndash; congratulations!). From both the #[span.link Dashboard] and
    #[span.link My Employment Profile] pages, you will see a link that encourages you to tell us about it. Click either
    of those links, and you will be brought to a page that allows you to indicate whether your employment actually came
    from someone finding your profile on Jobs, Jobs, Jobs, and gives you a place to write about the experience. These
    stories are only viewable by validated users, so feel free to use as much (or as little) identifying information as
    you&rsquo;d like. You can also submit this page with all the fields blank; in that case, your &ldquo;Seeking
    Employment&rdquo; flag is cleared, and the blank story is recorded.
  p.
    As a validated user, you can also view others success stories. Clicking #[span.link Success Stories] in the sidebar
    will display a list of all the stories that have been recorded. If there is a story to be read, there will be a link
    to read it; if you submitted the story, there will also be an #[span.link Edit] link.

  h5 Publicly Available Information
  p.
    The #[span.link Job Seekers] page for profile information will allow users to search for and display the continent,
    region, skills, and notes of users who are seeking employment #[strong and] have opted in to their information being
    publicly searchable. If you are a public user, this information is always the latest we have; check out the link at
    the top of the search results for how you can learn more about these fine human resources!

  hr

  h4#markdown A Bit about Markdown
  p.
    Markdown is a plain-text way to specify formatting quite similar to that provided by word processors. The
    #[a(href="https://daringfireball.net/projects/markdown/" target="_blank") original page] for the project is a good
    good overview of its capabilities, and the pages at
    #[a(href="https://www.markdownguide.org/" target="_blank") Markdown Guide] give in-depth lessons to make the most of
    this language. The version of Markdown employed here supports many popular extensions, include smart quotes (turning
    "a quote" into &ldquo;a quote&rdquo;), tables, super/subscripts, and more.

  hr

  h4 Help / Suggestions
  p.
    This is open-source software
    #[a(href="https://github.com/bit-badger/jobs-jobs-jobs" _target="_blank") developed on Github]; feel free to
    #[a(href="https://github.com/bit-badger/jobs-jobs-jobs/issues" target="_blank") create an issue there], or look up
    @danieljsummers on No Agenda Social.
</template>

<style lang="sass" scoped>
span.link
  background-color: rgba(144, 238, 144, .25)
span.button
  border: solid 1px lightgreen
  border-radius: .25rem
span.link,
span.button
  padding: 0 .25rem
</style>
