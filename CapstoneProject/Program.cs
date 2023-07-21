using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using CapstoneProject.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using SmartBreadcrumbs.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using BusinessObjects.Models;
using Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.CodeAnalysis.Options;

var builder = WebApplication.CreateBuilder(args);
IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
IConfigurationSection facebookAuthNSection = builder.Configuration.GetSection("Authentication:Facebook");

builder.Services.AddBreadcrumbs(Assembly.GetExecutingAssembly(), options => {
    options.TagName = "nav";
    options.TagClasses = "";
    options.OlClasses = "breadcrumb";
    options.LiClasses = "breadcrumb-item";
    options.ActiveLiClasses = "breadcrumb-item active";
});

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions => {
        googleOptions.ClientId = googleAuthNSection["ClientId"];
        googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
        googleOptions.CallbackPath = "/signin-google";
        googleOptions.Scope.Add("https://www.googleapis.com/auth/userinfo.email");
        googleOptions.SaveTokens = true;
        googleOptions.Events.OnCreatingTicket = ctx => {
            List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();
            tokens.Add(new AuthenticationToken() {
                Name = "TicketCreated",
                Value = DateTime.UtcNow.ToString()
            });
            ctx.Properties.StoreTokens(tokens);
            return Task.CompletedTask;
        };
    })
    .AddFacebook(facebookOptions => {
        facebookOptions.AppId = facebookAuthNSection["AppId"];
        facebookOptions.AppSecret = facebookAuthNSection["AppSecret"];
        facebookOptions.Scope.Remove("email");
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<RecipeOrganizerContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddIdentity<Account, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<RecipeOrganizerContext>();

builder.Services.Configure<IdentityOptions>(options => {
    // Default Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddScoped<IIngredientCategoryRepository, IngredientCategoryRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IFavouriteRepository, FavouriteRepository>();
builder.Services.AddScoped<IMealPlanRepository, MealPlanRepository>();
builder.Services.AddScoped<IRecipeFeedbackRepository, RecipeFeedbackRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCookiePolicy(new CookiePolicyOptions() {
    MinimumSameSitePolicy = SameSiteMode.Lax
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
