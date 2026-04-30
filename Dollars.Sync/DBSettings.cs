using Microsoft.Extensions.Configuration;

public class DBSettings
{
    public string ConnectionString { get; } = string.Empty;

    public DBSettings(IConfiguration config)
    {
        ConnectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;    
    }
}