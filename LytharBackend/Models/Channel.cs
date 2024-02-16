namespace LytharBackend.Models;

public class Channel
{
    public long ChannelId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required DateTime CreatedAt { get; set; }
}
