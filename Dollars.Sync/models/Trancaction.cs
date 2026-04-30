public class Transaction
{
    public int Id { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public string SourceId { get; set; } = string.Empty;
    public string Payee { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; } // if negative, it's an expense. if positive, it's income.
    public string Description { get; set; } = string.Empty;
    public string Memo { get; set; } = string.Empty;
}
