using Dollars.Api.Data;
using Microsoft.AspNetCore.Identity;

public static class RegisterUser
{
    public record Request(
        string Email, 
        string Password, 
        bool EnableNotifications = false);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/register", async (
            ILogger<Request> logger,
            Request req, 
            AppDbContext db,
            UserManager<AppUser> um) =>
        {
            var user = new AppUser
            {
                UserName = req.Email,
                EnableNotifications = req.EnableNotifications
            };
            using var txn = await db.Database.BeginTransactionAsync();

            var result = await um.CreateAsync(user, req.Password);

            if(result != IdentityResult.Success)
            {
                logger.LogError("failed to created user: {errors}", string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
                return Results.BadRequest(result.Errors);
            }

            result = await um.AddToRoleAsync(user, AppRoles.User);
            if(result != IdentityResult.Success)
            {
                logger.LogError("failed to add user to role \"user\": {errors}", string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
                return Results.BadRequest(result.Errors);
            }

            await txn.CommitAsync();

            return Results.Ok(new { user.Id });
        });
    }
}