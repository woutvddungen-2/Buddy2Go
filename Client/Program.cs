using Client;
using Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JourneyService>();
builder.Services.AddScoped<BuddyService>();

var config = builder.Configuration;
string apiBase = config["ApiBaseUrl"] ?? throw new Exception("No BASE URL loaded");

builder.Services.AddScoped(sp =>
{
    var http = new HttpClient
    {
        BaseAddress = new Uri(apiBase)
    };
    // Make sure the browser includes cookies
    http.DefaultRequestHeaders.Add("Accept", "application/json");

    return http;
});

await builder.Build().RunAsync();
