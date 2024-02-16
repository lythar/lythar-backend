using System.Net;

namespace LytharBackend.Exceptons;

public class ChannelNotFoundException : BaseHttpException
{
    public ChannelNotFoundException(string channelId) : base("ChannelNotFound", $"Channel '{channelId}' doesn't exist.", HttpStatusCode.NotFound) { }
}
