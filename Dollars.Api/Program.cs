using System.Security.Claims;
using Dollars.Api.Data;
using Dollars.Shared.Repos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<AccountsRepo>();
builder.Services.AddDbContext<AppDbContext>(o => {
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddAuthentication(o =>
{
   o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
   o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; 
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:Issuer"];
    o.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:Audience"];
    o.TokenValidationParameters.IssuerSigningKey =
        new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]
        ?? throw new ArgumentException("Jwt:SecretKey not found")));
    o.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(3);
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

    // todo: seed admin user
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
