global using BlazorStreamingDemo.Client;
global using BlazorStreamingDemo.Client.Services;
global using BlazorStreamingDemo.Server.Data;
using BlazorStreamingDemo.Components;
using Grpc.Net.Client.Web;
using Grpc.Net.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Configure your HttpClient with a local base address for debugging
builder.Services.AddHttpClient("MyApiClient", client =>
{
    // Replace with your API's local address and port
    client.BaseAddress = new Uri("https://localhost:7071/");
    // Additional configurations
});

builder.Services.AddSingleton<ApiService>();
builder.Services.AddGrpc();

builder.Services.AddSingleton<PersonsManager>();
builder.Services.AddControllers();

builder.Services.AddSingleton(services =>
{
    var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb,
        new HttpClientHandler()));
    var baseUri = new Uri("https://localhost:7071/");
    var channel = GrpcChannel.ForAddress(baseUri,
        new GrpcChannelOptions { HttpClient = httpClient });
    return new People.PeopleClient(channel);
});

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.UseGrpcWeb();
app.MapGrpcService<PeopleService>().EnableGrpcWeb();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorStreamingDemo.Client._Imports).Assembly);

app.MapHub<StreamHub>("/StreamHub");

app.Run();
