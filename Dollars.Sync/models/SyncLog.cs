public class SyncLog
{
    public int Id { get; set; }
    public DateTime SyncDate { get; set; }
    public string Provider { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public string JsonData { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}