using Blazored.Toast;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobsJobsJobs.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton(new AppState());
            builder.Services.AddSingleton(new JsonSerializerOptions().ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
            builder.Services.AddBlazoredToast();
            await builder.Build().RunAsync();
        }
    }
}
