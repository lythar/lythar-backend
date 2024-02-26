namespace LytharBackend.Models;

public class Channel
{
    public long ChannelId { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public required DateTime CreatedAt { get; set; }
    public User? Creator { get; set; } = null!;
    public bool IsPublic { get; set; } = true;
    public bool IsDirectMessages { get; set; } = false;
    // only has members when it's not public
    public List<User> Members { get; set; } = new();
    public string? IconId { get; set; }
    public string? IconUrl { get; set; }
}
