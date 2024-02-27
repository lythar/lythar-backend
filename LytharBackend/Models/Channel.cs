using LytharBackend.Exceptons;
using Microsoft.EntityFrameworkCore;

namespace LytharBackend.Models;

public static class ChannelExtensions
{
    public static async Task<Channel> GetChannelById(this DatabaseContext context, long id)
    {
        var channel = await context.Channels.Where(x => x.ChannelId == id).FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(id.ToString());
        }

        return channel;
    }

    public static IQueryable<Channel> WhereHasAccess(this IQueryable<Channel> query, User user)
    {
        return query.Where(x => user.IsAdmin || x.IsPublic || x.Members.Contains(user));
    }

    public static IQueryable<Channel> WhereHasAdminAccess(this IQueryable<Channel> query, User user)
    {
        return query.Where(x => user.IsAdmin || (x.Creator != null && x.Creator == user));
    }

    public static async Task<Channel> FirstOrThrowAsync(this IQueryable<Channel> query, long id)
    {
        var result = await query.FirstOrDefaultAsync();

        if (result == null)
        {
            throw new ChannelNotFoundException(id.ToString());
        }

        return result;
    }
}

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
