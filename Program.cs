//using chat.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlaySafe.Data;
using PlaySafe.Hubs;
using PlaySafe.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<dbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbContext") ?? throw new InvalidOperationException("Connection string 'dbContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
//configure app services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddMvc(options =>
{
    options.EnableEndpointRouting = false;
    var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddXmlDataContractSerializerFormatters();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<dbContext>();
builder.Services.Configure<IdentityOptions>(options => 
{
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireNonAlphanumeric = false;

});
builder.Services.AddSession();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseMvc();
app.UseAuthorization();
app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");
});



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=login}/{id?}");
//AppDbInitializer.Seed(app);

app.Run();
