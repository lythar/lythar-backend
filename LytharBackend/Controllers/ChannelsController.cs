﻿using LytharBackend.Exceptons;
using LytharBackend.Models;
using LytharBackend.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;

namespace LytharBackend.Controllers;

[Route("channels/api")]
public class ChannelsController : Controller
{
    private ISessionService SessionService;
    private DatabaseContext DatabaseContext;

    public ChannelsController(ISessionService sessionService, DatabaseContext databaseContext)
    {
        SessionService = sessionService;
        DatabaseContext = databaseContext;
    }

    public class CreateChannelForm
    {
        [MaxLength(32)]
        public required string Name { get; set; }
        [MaxLength(2048)]
        public required string Description { get; set; }
    }

    public class CreateChannelResponse
    {
        public required long ChannelId { get; set; }
    }

    [HttpPost]
    [Route("create")]
    [SwaggerResponse(200, typeof(CreateChannelResponse))]
    public async Task<CreateChannelResponse> CreateChannel([FromBody] CreateChannelForm createChannelForm)
    {
        await SessionService.VerifyRequest(HttpContext);

        // TO-DO: Check if the user is actually allowed to do it

        var channel = new Channel
        {
            Name = createChannelForm.Name.Trim(),
            Description = createChannelForm.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var instertedChannel = DatabaseContext.Channels.Add(channel);
        await DatabaseContext.SaveChangesAsync();

        return new CreateChannelResponse
        {
            ChannelId = instertedChannel.Entity.ChannelId
        };
    }

    [HttpGet]
    [Route("list")]
    [SwaggerResponse(200, typeof(IEnumerable<Channel>))]
    public async Task<IEnumerable<Channel>> ListChannels()
    {
        await SessionService.VerifyRequest(HttpContext);

        return await DatabaseContext.Channels.ToListAsync();
    }

    [HttpGet]
    [Route("{channelId}")]
    [SwaggerResponse(200, typeof(Channel))]
    public async Task<Channel> GetChannel([FromRoute] long channelId)
    {
        await SessionService.VerifyRequest(HttpContext);

        var channel = await DatabaseContext.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        return channel;
    }

    public class SendMessageForm
    {
        [MaxLength(2000)]
        public required string Content { get; set; }
    }

    public class SendMessageResponse
    {
        public required long MessageId { get; set; }
    }

    [HttpPost]
    [Route("{channelId}/messages")]
    public async Task SendMessage([FromRoute] long channelId, [FromBody] SendMessageForm messageForm)
    {
        var token = await SessionService.VerifyRequest(HttpContext);

        var channel = await DatabaseContext.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        var message = new Message
        {
            Content = messageForm.Content.Trim(),
            SentAt = DateTime.UtcNow,
            ChannelId = channelId,
            AuthorId = token.AccountId
        };

        DatabaseContext.Messages.Add(message);
        await DatabaseContext.SaveChangesAsync();

        return;
    }

    public class ListMessagesResponse
    {
        public required long MessageId { get; set; }
        public required string Content { get; set; }
        public required DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public required AccountController.UserAccountResponse Author { get; set; }
    }

    public class ListMessagesQuery
    {
        public long? Before { get; set; }
        public long? After { get; set; }
        [Range(1, 100)]
        public int? Limit { get; set; }
    }

    [HttpGet]
    [Route("{channelId}/messages")]
    [SwaggerResponse(200, typeof(IEnumerable<ListMessagesResponse>))]
    public async Task<IEnumerable<ListMessagesResponse>> ListMessages([FromRoute] long channelId, [FromQuery] ListMessagesQuery query)
    {
        await SessionService.VerifyRequest(HttpContext);

        var messages = await DatabaseContext.Messages
            .Include(x => x.Author)
            .Where(x => x.ChannelId == channelId)
            .OrderByDescending(x => x.SentAt)
            .Where(x => query.Before == null || x.MessageId < query.Before)
            .Where(x => query.After == null || x.MessageId > query.After)
            .Take(query.Limit ?? 100)
            .ToListAsync();

        return messages.ConvertAll(x => new ListMessagesResponse
        {
            MessageId = x.MessageId,
            Content = x.Content,
            SentAt = x.SentAt,
            EditedAt = x.EditedAt,
            Author = new AccountController.UserAccountResponse
            {
                Id = x.Author.Id,
                Name = x.Author.Name,
                LastName = x.Author.LastName,
                Email = x.Author.Email,
                AvatarUrl = x.Author.AvatarUrl
            }
        });
    }
}