﻿using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Profiles
{
    public partial class Search : ComponentBase
    {
        /// <summary>
        /// Whether a search has been performed
        /// </summary>
        private bool Searched { get; set; } = false;

        /// <summary>
        /// Indicates whether a request for matching profiles is in progress
        /// </summary>
        private bool Searching { get; set; } = false;

        /// <summary>
        /// The search criteria
        /// </summary>
        private ProfileSearch Criteria { get; set; } = new ProfileSearch();

        /// <summary>
        /// Error messages encountered while searching for profiles
        /// </summary>
        private IList<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// All continents
        /// </summary>
        private IEnumerable<Continent> Continents { get; set; } = Enumerable.Empty<Continent>();

        /// <summary>
        /// The search results
        /// </summary>
        private IEnumerable<ProfileSearchResult> SearchResults { get; set; } = Enumerable.Empty<ProfileSearchResult>();

        protected override async Task OnInitializedAsync()
        {
            Continents = await state.GetContinents(http);

            // Determine if we have searched before
            var query = QueryHelpers.ParseQuery(nav.ToAbsoluteUri(nav.Uri).Query);

            if (query.TryGetValue("Searched", out var searched))
            {
                Searched = Convert.ToBoolean(searched);
                void setPart(string part, Action<string> func)
                {
                    if (query.TryGetValue(part, out var partValue)) func(partValue);
                }
                setPart("ContinentId", x => Criteria.ContinentId = x);
                setPart("Skill", x => Criteria.Skill = x);
                setPart("BioExperience", x => Criteria.BioExperience = x);
                setPart("RemoteWork", x => Criteria.RemoteWork = x);

                await RetrieveProfiles();
            }
        }

        /// <summary>
        /// Do a search
        /// </summary>
        /// <remarks>This navigates with the parameters in the URL; this should trigger a search</remarks>
        private async Task DoSearch()
        {
            var query = SearchQuery();
            query.Add("Searched", "True");
            nav.NavigateTo(QueryHelpers.AddQueryString("/profile/search", query));
            await RetrieveProfiles();
        }

        /// <summary>
        /// Retreive profiles matching the current search criteria
        /// </summary>
        private async Task RetrieveProfiles()
        {
            Searching = true;

            var searchResult = await ServerApi.RetrieveMany<ProfileSearchResult>(http,
                QueryHelpers.AddQueryString("profile/search", SearchQuery()));

            if (searchResult.IsOk)
            {
                SearchResults = searchResult.Ok;
            }
            else
            {
                ErrorMessages.Add(searchResult.Error);
            }

            Searched = true;
            Searching = false;
        }

        private static string? IsSeeking(ProfileSearchResult profile) =>
            profile.SeekingEmployment ? "font-weight-bold" : null;

        /// <summary>
        /// Return "Yes" for true and "No" for false
        /// </summary>
        /// <param name="condition">The condition in question</param>
        /// <returns>"Yes" for true, "No" for false</returns>
        private static string YesOrNo(bool condition) => condition ? "Yes" : "No";

        /// <summary>
        /// Create a search query string from the currently-entered criteria
        /// </summary>
        /// <returns>The query string for the currently-entered criteria</returns>
        private IDictionary<string, string?> SearchQuery()
        {
            var dict = new Dictionary<string, string?>();
            if (Criteria.IsEmptySearch) return dict;

            void part(string name, Func<ProfileSearch, string?> func)
            {
                if (!string.IsNullOrEmpty(func(Criteria))) dict.Add(name, func(Criteria));
            }

            part("ContinentId", it => it.ContinentId);
            part("Skill", it => it.Skill);
            part("BioExperience", it => it.BioExperience);
            part("RemoteWork", it => it.RemoteWork);

            return dict;
        }
    }
}
