using Client;
using Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<LoginService>();
//builder.Services.AddScoped<ProductService>();
//builder.Services.AddScoped<ChatService>();

builder.Services.AddScoped(sp =>
{
    var http = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:5001/")
    };
    // Make sure the browser includes cookies
    http.DefaultRequestHeaders.Add("Accept", "application/json");

    return http;
});

await builder.Build().RunAsync();
