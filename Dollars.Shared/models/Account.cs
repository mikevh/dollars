namespace Dollars.Shared.Models;

public class Account
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;    
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
