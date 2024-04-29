using BlazorGrpcDemo.Client.Pages;
using BlazorGrpcDemo.Client.Services;
using BlazorGrpcDemo.Components;
using BlazorGrpcDemo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7086/") });

builder.Services.AddSingleton<PersonsManager>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddGrpc();

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

app.UseGrpcWeb();
app.MapGrpcService<PeopleService>().EnableGrpcWeb();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorGrpcDemo.Client._Imports).Assembly);

app.MapControllers();

app.Run();
