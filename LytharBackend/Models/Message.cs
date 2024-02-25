namespace LytharBackend.Models;

public class Message
{
    public long MessageId { get; set; }
    public required string Content { get; set; }
    public required DateTime SentAt { get; set; }
    public DateTime? EditedAt { get; set; } = null;
    public Channel Channel { get; set; } = null!;
    public required long ChannelId { get; set; }
    public User Author { get; set; } = null!;
    public required int AuthorId { get; set; }
    public List<Attachment> Attachments { get; set; } = new();
}
