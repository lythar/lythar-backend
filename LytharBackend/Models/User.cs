namespace LytharBackend.Models;

public class User
{
    public int Id { get; set; }
    public required string Login { get; set; }
    public required string Name { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }

    /// <summary>
    /// Any URL that fits in HTML img src attribute, so can be a base64 blob or an actual URL
    /// </summary>
    public string? AvatarUrl { get; set; }
}
