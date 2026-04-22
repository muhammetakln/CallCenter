using Business;
using Core.Abstracts.IServices;
using Core.Concretes.Enums;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomerServices(builder.Configuration);

builder.Services.AddRazorPages();


builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/identity/login";
    options.LogoutPath = "/identity/logout";
    options.AccessDeniedPath = "/identity/accessdenied";

});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
//Bait iþlemler için tek bir sorumluluk bulunan bir mikroservis oluþumu.
app.MapPost("/api/leads/pick/{id}", async (ILeadService service, ClaimsPrincipal user, string id) => await service.PickLeadAsync(id, user));
app.MapPost("/api/leads/addactivity/{type}/{id}", async (ILeadService service, ClaimsPrincipal user,string id , ActivityType type) => await service.AddActivityAsync(type,id, user));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
