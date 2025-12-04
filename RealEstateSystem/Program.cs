using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Data;
using RealEstateSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Password hasher for our custom User entity
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Session (for login state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


// =============================
// SEED DEFAULT ADMIN USER
// =============================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    // Apply migrations (if not already)
    context.Database.Migrate();

    // 🔐 FIXED ADMIN CREDENTIALS
    var adminEmail = "admin@gmail.com";   // EXACT email for login (no "a" in gmil)
    var adminPassword = "Admin@123";
    //......................................................................................................................
    // Check if this admin already exists
    //var existingAdmin = context.Users.FirstOrDefault(u => u.Email == adminEmail);
    // Check if an admin already exists (any email)
    //......................................................................................................................


    var existingAdmin = context.Users.FirstOrDefault(u => u.Role == UserRole.Admin);


    if (existingAdmin == null)
    {
        var adminUser = new User
        {
            FirstName = "System",
            LastName = "Admin",
            Email = adminEmail,
            PhoneNumber = "0123456789",
            Role = UserRole.Admin,
            IsActive = true,
            IsVerified = true,
            CreatedDate = DateTime.UtcNow
        };

        // Hash the fixed password
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

        context.Users.Add(adminUser);
        context.SaveChanges();
    }
}


// =============================
// HTTP PIPELINE
// =============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();      // session BEFORE auth/authorization

// later you can add app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}")
    .WithStaticAssets();

app.Run();


