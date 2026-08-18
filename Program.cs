using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using MoneyMiners.Models;
using MoneyMiners.Repositories;
using MoneyMiners.Services;

var builder = WebApplication.CreateBuilder(args);


// ================================
// MVC services
// ================================

builder.Services.AddControllersWithViews();


// ================================
// Authentication
// ================================

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

            options.Cookie.HttpOnly =
                true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(30);

            options.SlidingExpiration =
                true;
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

            options.Cookie.HttpOnly =
                true;

            options.Cookie.SecurePolicy =
                CookieSecurePolicy.Always;

            options.Cookie.SameSite =
                SameSiteMode.Lax;

            options.ExpireTimeSpan =
                TimeSpan.FromMinutes(60);

            options.SlidingExpiration =
                true;
        });


builder.Services.AddAuthorization();


// ================================
// Repository services
// ================================

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


// Investor Email OTP repository
builder.Services.AddScoped<
    IInvestorEmailOtpRepository,
    InvestorEmailOtpRepository>();


builder.Services.AddScoped<
    IInvestmentRepository,
    InvestmentRepository>();


// Existing Admin Mobile Password Reset OTP repository
builder.Services.AddScoped<
    IAdminPasswordResetOtpRepository,
    AdminPasswordResetOtpRepository>();


// New Admin Email Password Reset OTP repository
builder.Services.AddScoped<
    IAdminPasswordResetEmailOtpRepository,
    AdminPasswordResetEmailOtpRepository>();


builder.Services.AddScoped<
    IAdminMobileVerificationOtpRepository,
    AdminMobileVerificationOtpRepository>();


// ================================
// Security / OTP / Email services
// ================================

builder.Services.AddSingleton<
    ISensitiveDataProtector,
    SensitiveDataProtector>();


// ================================
// Email configuration
// ================================

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        EmailSettings.SectionName));


// SMTP email sender
builder.Services.AddScoped<
    IEmailSender,
    SmtpEmailSender>();


// ================================
// SMS services
// ================================

// One SMS sender will be used for
// Investor OTP
// + Admin Password Reset OTP
// + Admin Mobile Verification OTP.
builder.Services.AddScoped<
    ISmsSender,
    DevelopmentInvestorSmsSender>();


// ================================
// Investor OTP services
// ================================

// Existing mobile OTP service
builder.Services.AddScoped<
    IInvestorOtpService,
    InvestorOtpService>();


// Temporary email OTP service
builder.Services.AddScoped<
    IInvestorEmailOtpService,
    InvestorEmailOtpService>();


// ================================
// Admin OTP services
// ================================

// Existing Admin Mobile Forgot Password OTP service
builder.Services.AddScoped<
    IAdminPasswordResetOtpService,
    AdminPasswordResetOtpService>();


// New Admin Email Forgot Password OTP service
builder.Services.AddScoped<
    IAdminPasswordResetEmailOtpService,
    AdminPasswordResetEmailOtpService>();


// Admin Mobile Verification OTP service
builder.Services.AddScoped<
    IAdminMobileVerificationOtpService,
    AdminMobileVerificationOtpService>();


// ================================
// Password hashing
// ================================

// Admin password hashing
builder.Services.AddScoped<
    IPasswordHasher<AdminUser>,
    PasswordHasher<AdminUser>>();


// Investor password hashing
builder.Services.AddScoped<
    IPasswordHasher<InvestorAccount>,
    PasswordHasher<InvestorAccount>>();


// ================================
// Build application
// ================================

var app = builder.Build();


// ================================
// HTTP request pipeline
// ================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error");

    app.UseHsts();
}


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();


app.MapControllerRoute(
        name: "default",
        pattern:
            "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();