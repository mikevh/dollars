using System.Security.Claims;
using Dollars.Api.Data;
using Dollars.Shared.Repos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt settings not found");
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<AccountsRepo>();
builder.Services.AddDbContext<AppDbContext>(o => {
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                jwtSettings.SecretKey ?? throw new InvalidOperationException("Jwt:SecretKey not found"))),
            ClockSkew = TimeSpan.FromSeconds(3),
            ValidateIssuerSigningKey = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // todo: move to a helper class
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    using var txn = await db.Database.BeginTransactionAsync();
    
    // ensure roles exist

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    if(!await roleManager.RoleExistsAsync(AppRoles.Admin))
    {
        await roleManager.CreateAsync(new AppRole() { Name = AppRoles.Admin });
    }
    if(!await roleManager.RoleExistsAsync(AppRoles.User))
    {
        await roleManager.CreateAsync(new AppRole() { Name = AppRoles.User });
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var admin = await userManager.FindByNameAsync(builder.Configuration["Admin:User"] ?? throw new ArgumentException("Admin:User not found"));
    if(admin == null)
    {
        admin = new AppUser
        {
            UserName = builder.Configuration["Admin:User"]
        };
        var result = await userManager.CreateAsync(admin, builder.Configuration["Admin:Password"] ?? throw new ArgumentException("User:Password not found"));
        await userManager.AddToRolesAsync(admin, [AppRoles.Admin, AppRoles.User]);
    }

    await txn.CommitAsync();
}

// map features
RegisterUser.MapEndpoint(app);
LoginUser.MapEndpoint(app);

app.MapGet("/api/transactions", 
    async (AccountsRepo r) => 
        await r.Transactions()
    ).WithName("Transactions");

app.MapGet("/me", async (ClaimsPrincipal cp) =>
{
    return Results.Ok(cp.Claims.ToDictionary(c => c.Type, c => c.Value));
}).RequireAuthorization();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();
