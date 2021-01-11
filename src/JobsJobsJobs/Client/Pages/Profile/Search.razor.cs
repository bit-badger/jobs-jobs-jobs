﻿using JobsJobsJobs.Shared;
using JobsJobsJobs.Shared.Api;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client.Pages.Profile
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
            ServerApi.SetJwt(http, state);
            var continentResult = await ServerApi.RetrieveMany<Continent>(http, "continent/all");

            if (continentResult.IsOk)
            {
                Continents = continentResult.Ok;
            }
            else
            {
                ErrorMessages.Add(continentResult.Error);
            }

            // TODO: remove this call once the filter is ready
            await RetrieveProfiles();
        }

        /// <summary>
        /// Retreive profiles matching the current search criteria
        /// </summary>
        private async Task RetrieveProfiles()
        {
            Searching = true;

            // TODO: send a filter with this request
            var searchResult = await ServerApi.RetrieveMany<ProfileSearchResult>(http, "profile/search");

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
    }
}
