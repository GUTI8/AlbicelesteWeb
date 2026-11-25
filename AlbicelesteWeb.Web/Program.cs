using AlbicelesteWeb.Web.Components;
using AlbicelesteWeb.Data;
using Microsoft.EntityFrameworkCore;
using AlbicelesteWeb.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Albiceleste")
                       ?? throw new InvalidOperationException("Connection string 'Albiceleste' not found.");


builder.Services.AddDbContextFactory<AlbicelesteDbContext>(o =>
    o.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<ProfileService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
