<template lang="pug">
article
  page-title(title='How It Works')
  h3 How It Works
  h4 Completing Your Profile
  ul
    li.
      The &ldquo;View Your Employment Profile&rdquo; link (which you&rdquo;ll see on this page, once your profile is
      established) shows your profile the way all other validated users will be able to see it. While this site does not
      perform communication with others over No Agenda Social, the name on employment profiles is a link to that
      user&rsquo;s profile; from there, others can communicate further with you using the tools Mastodon provides.
    li.
      The &ldquo;Professional Biography&rdquo; and &ldquo;Experience&rdquo; sections support Markdown, a plain-text way
      to specify formatting quite similar to that provided by word processors. The
      #[a(href='https://daringfireball.net/projects/markdown/' target='_blank') original page] for the project is a
      good overview of its capabilities, and the pages at
      #[a(href='https://www.markdownguide.org/' target='_blank') Markdown Guide] give in-depth lessons to make the most
      of this language. The version of Markdown employed here supports many popular extensions, include smart quotes
      (turning "a quote" into &ldquo;a quote&rdquo;), tables, super/subscripts, and more.
    li.
      Skills are optional, but they are the place to record skills you have. Along with the skill, there is a
      &ldquo;Notes&rdquo; section, which can be used to indicate the time you&rsquo;ve practiced a particular skill, the
      mastery you have of that skill, etc. It is free-form text, so it is all up to you how you utilize the field.
    li.
      The &ldquo;Experience&rdquo; field is intended to capture a chronological or topical employment history; with this
      &ldquo;quick-n-dirty&rdquo; implementation, this Markdown box can be used to capture that information however you
      would like it presented to fellow citizens.
    li.
      If you check the &ldquo;Allow my profile to be searched publicly&rdquo; checkbox, #[strong and] you are seeking
      employment, your continent, region, and skills fields will be searchable and displayed to public users of the
      site. They will not be tied to your No Agenda Social handle or real name; they are there to let people peek
      behind the curtain a bit, and hopefully inspire them to join us.

  h4 Searching Profiles
  p.
    The &ldquo;View Profiles&rdquo; link at the side allows you to search for profiles by continent, the citizen&rsquo;s
    desire for remote work, a skill, or any text in their professional biography and experience. If you find someone
    with whom you&rsquo;d like to discuss potential opportunities, the name at the top of the profile links to their No
    Agenda Social account, where you can use its features to get in touch.

  h4 Finding Employment
  p.
    If your profile indicates that you are seeking employment, and you secure employment, that is something you will
    want to update (and &ndash; congratulations!). From both the Dashboard and the Edit Profile pages, you will see a
    link that encourages you to tell us about it. Click either of those links, and you will be brought to a page that
    allows you to indicate whether your employment actually came from someone finding your profile on Jobs, Jobs, Jobs,
    and gives you a place to write about the experience. These stories are only viewable by validated users, so feel
    free to use as much (or as little) identifying information as you&rsquo;d like. You can also submit this page with
    all the fields blank; in that case, your &ldquo;Seeking Employment&rdquo; flag is cleared, and the
    &ldquo;story&rdquo; is recorded.
  p.
    As a validated user, you can also view others success stories. Clicking &ldquo;Success Stories&rdquo; in the sidebar
    will display a list of all the stories that have been recorded. If there is a story to be read, there will be a link
    to read it; if you submitted the story, there will also be an &ldquo;Edit&rdquo; link.

  h4 Publicly Available Information
  p.
    The &ldquo;Job Seekers&rdquo; page for profile information will allow users to search for and display the continent,
    region, skills, and notes of users who are seeking employment #[strong and] have opted in to their information being
    publicly searchable. If you are a public user, this information is always the latest we have; check out the link at
    the top of the search results for how you can learn more about these fine human resources!

  h4 Help / Suggestions
  p.
    This is open-source software
    #[a(href='https://github.com/bit-badger/jobs-jobs-jobs' _target='_blank') developed on Github]; feel free to
    #[a(href='https://github.com/bit-badger/jobs-jobs-jobs/issues' target='_blank') create an issue there], or look up
    @danieljsummers on No Agenda Social.
</template>
