using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC services
builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)

    // Admin / SuperAdmin authentication cookie
    .AddCookie(
        CookieAuthenticationDefaults.AuthenticationScheme,
        options =>
        {
            options.LoginPath =
                "/AdminAccount/Login";

            options.AccessDeniedPath =
                "/AdminAccount/AccessDenied";

            options.Cookie.Name =
                "MoneyMiners.AdminAuth";

            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(30);

            options.SlidingExpiration = true;
        })

    // Investor authentication cookie
    .AddCookie(
        "InvestorCookie",
        options =>
        {
            options.LoginPath =
                "/InvestorAccount/Login";

            options.AccessDeniedPath =
                "/InvestorAccount/AccessDenied";

            options.Cookie.Name =
                "MoneyMiners.InvestorAuth";

            options.Cookie.HttpOnly = true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(60);

            options.SlidingExpiration = true;
        });

builder.Services.AddAuthorization();

// Repository services
builder.Services.AddScoped<
    IContactMessageRepository,
    ContactMessageRepository>();

builder.Services.AddScoped<
    IAdminUserRepository,
    AdminUserRepository>();

builder.Services.AddScoped<
    IInvestorRepository,
    InvestorRepository>();

builder.Services.AddScoped<
    IInvestorAccountRepository,
    InvestorAccountRepository>();

builder.Services.AddScoped<
    IInvestorOtpRepository,
    InvestorOtpRepository>();

builder.Services.AddScoped<
    IInvestmentRepository,
    InvestmentRepository>();

// Security services
builder.Services.AddSingleton<
    ISensitiveDataProtector,
    SensitiveDataProtector>();

builder.Services.AddScoped<
    IInvestorSmsSender,
    DevelopmentInvestorSmsSender>();

builder.Services.AddScoped<
    IInvestorOtpService,
    InvestorOtpService>();

builder.Services.AddScoped<
    IPasswordHasher<AdminUser>,
    PasswordHasher<AdminUser>>();

builder.Services.AddScoped<
    IPasswordHasher<InvestorAccount>,
    PasswordHasher<InvestorAccount>>();

var app = builder.Build();

// HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();