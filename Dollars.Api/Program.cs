using Dollars.Shared.Repos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<AccountsRepo>();

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
