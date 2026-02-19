using JobCommandCenter.Data;
using JobCommandCenter.ServiceDefaults;
using JobCommandCenter.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add PostgreSQL database
builder.AddNpgsqlDbContext<JobCommandCenterDbContext>("jobcommandcenter-db");

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add MudBlazor
builder.Services.AddMudServices();

// Add application services
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IScoringConfigService, ScoringConfigService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<JobCommandCenter.Web.App>()
    .AddInteractiveServerRenderMode();

// Map Aspire health checks
app.MapDefaultEndpoints();

app.Run();
