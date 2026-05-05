using Dollars.Api.Data;
using Dollars.Shared.Repos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<AccountsRepo>();
builder.Services.AddDbContext<AppDbContext>(o => {
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<AppDbContext>()

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseHttpsRedirection();

app.MapGet("/api/transactions", 
    async (AccountsRepo r) => 
        await r.Transactions()
    ).WithName("Transactions");

app.Run();
