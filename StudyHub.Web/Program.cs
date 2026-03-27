using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudyHub.Web.Data;
using StudyHub.Web.Data.Entities;
using StudyHub.Web.Services;
using StudyHub.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<StudyHubDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 2. Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Relaxed password requirements for development/student ease of use
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<StudyHubDbContext>()
.AddDefaultTokenProviders();

// 3. Configure Cookie Authentication Paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// 4. Register Custom Application Services (Service Layer)
builder.Services.AddScoped<IGroupService, GroupService>();

// 5. Add MVC Services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ==========================================
// 6. ROLE SEEDING (PlatformAdmin & Default Admin)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure roles exist
    string adminRole = "PlatformAdmin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Ensure default admin user exists
    string adminEmail = "admin@studyhub.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Admin"
        };
        await userManager.CreateAsync(adminUser, "admin123");
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}

// 7. Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();