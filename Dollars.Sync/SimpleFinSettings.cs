public class SimpleFinSettings
{
    public bool Enabled { get; set; } = false;
    public string AccessUrl { get; set; } = string.Empty;
    public int SyncBackDays { get; set; } = 45;
    public int WaitHours = 4;
}