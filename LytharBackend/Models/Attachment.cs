namespace LytharBackend.Models;

public class Attachment
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string CdnNamespace { get; set; }
    public required string CdnUrl { get; set; }
}