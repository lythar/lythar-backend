using System.Net;

namespace LytharBackend.Exceptons;

public class ChannelNotFoundException : BaseHttpException
{
    public ChannelNotFoundException(string channelId) : base("ChannelNotFound", $"Kanał '{channelId}' nie istnieje.", HttpStatusCode.NotFound) { }
}
