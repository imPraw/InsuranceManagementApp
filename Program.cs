using InsuranceManagement.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MVC (for views) AND API controllers together
builder.Services.AddControllersWithViews();

// Required for ClaimsApiController to access the session (to check who's logged in)
builder.Services.AddHttpContextAccessor();

// Register Entity Framework Core with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=insurance.db"));

// Session support for our custom authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session must come BEFORE authorization and routing
app.UseSession();

app.UseAuthorization();

// Seed default data (admin user, roles, menus)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // Make sure the database and all tables exist
    context.Database.EnsureCreated();
    DbSeeder.SeedData(context);
}

// This handles both MVC routes (/Insurance/Index) and API routes (/api/claims)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// This enables the [ApiController] attribute routes like /api/claims
app.MapControllers();

app.Run();
