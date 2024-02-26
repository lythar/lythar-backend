using LytharBackend.Exceptons;
using LytharBackend.Files;
using LytharBackend.ImageGeneration;
using LytharBackend.Models;
using LytharBackend.Session;
using LytharBackend.WebSocket;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using static LytharBackend.Controllers.AccountController;
using static LytharBackend.Controllers.AttachmentsController;

namespace LytharBackend.Controllers;

[Route("channels/api")]
public class ChannelsController : Controller
{
    private ISessionService SessionService;
    private IFileService FileService;
    private DatabaseContext DatabaseContext;

    public ChannelsController(ISessionService sessionService, IFileService fileService, DatabaseContext databaseContext)
    {
        SessionService = sessionService;
        FileService = fileService;
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
        public required string Name { get; set; }
        public required string Description { get; set; }
    }

    [HttpPost, Route("create")]
    [SwaggerResponse(200, typeof(CreateChannelResponse))]
    public async Task<CreateChannelResponse> CreateChannel([FromBody] CreateChannelForm createChannelForm)
    {
        await SessionService.VerifyRequest(HttpContext);

        // TO-DO: Check if the user is actually allowed to do it via some role system

        var channel = new Channel
        {
            Name = createChannelForm.Name.Trim(),
            Description = createChannelForm.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var instertedChannel = DatabaseContext.Channels.Add(channel);
        await DatabaseContext.SaveChangesAsync();

        var response = new CreateChannelResponse
        {
            ChannelId = instertedChannel.Entity.ChannelId,
            Name = instertedChannel.Entity.Name,
            Description = instertedChannel.Entity.Description
        };

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<CreateChannelResponse>
        {
            Type = "ChannelCreated",
            Data = response
        });

        return response;
    }

    [HttpGet, Route("list")]
    [SwaggerResponse(200, typeof(IEnumerable<Channel>))]
    public async Task<IEnumerable<Channel>> ListChannels()
    {
        await SessionService.VerifyRequest(HttpContext);

        return await DatabaseContext.Channels.ToListAsync();
    }

    [HttpGet, Route("{channelId}")]
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

    [HttpDelete, Route("{channelId}")]
    public async Task DeleteChannel([FromRoute] long channelId)
    {
        await SessionService.VerifyRequest(HttpContext);

        // TO-DO: Check if the user is actually allowed to do it via some role system

        var channel = await DatabaseContext.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<long>
        {
            Type = "ChannelDeleted",
            Data = channel.ChannelId
        });

        var messages = await DatabaseContext.Messages.Where(x => x.ChannelId == channelId).ToListAsync();

        foreach (var message in messages)
        {
            foreach (var attachment in message.Attachments)
            {
                await FileService.DeleteFile(attachment.CdnNamespace, attachment.Name);
                DatabaseContext.Attachments.Remove(attachment);
            }

            DatabaseContext.Messages.Remove(message);
        }

        DatabaseContext.Channels.Remove(channel);
        await DatabaseContext.SaveChangesAsync();
    }

    public class SendMessageForm
    {
        [MaxLength(2000)]
        public required string Content { get; set; }
        public List<Guid> AttachmentIds { get; set; } = new();
    }

    public class SendMessageResponse
    {
        public required long MessageId { get; set; }
    }

    [HttpPost, Route("{channelId}/messages")]
    public async Task SendMessage([FromRoute] long channelId, [FromBody] SendMessageForm messageForm)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == token.AccountId).FirstOrDefaultAsync();

        var channel = await DatabaseContext.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        if (user == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        var attachments = await DatabaseContext.Attachments.Where(x => messageForm.AttachmentIds.Contains(x.Id)).ToListAsync();

        var message = new Message
        {
            Content = messageForm.Content.Trim(),
            SentAt = DateTime.UtcNow,
            ChannelId = channelId,
            AuthorId = token.AccountId,
            Attachments = attachments
        };

        var insertedMessage = DatabaseContext.Messages.Add(message);
        await DatabaseContext.SaveChangesAsync();

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<ListMessagesResponse>
        {
            Type = "NewMessage",
            Data = new ListMessagesResponse
            {
                MessageId = insertedMessage.Entity.MessageId,
                Content = insertedMessage.Entity.Content,
                ChannelId = insertedMessage.Entity.ChannelId,
                SentAt = insertedMessage.Entity.SentAt,
                Author = new UserAccountResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl
                },
                Attachments = insertedMessage.Entity.Attachments.Select(AttachmentResponse.FromDatabase).ToList()
            }
        });
    }

    [HttpPatch, Route("{channelId}/messages/{messageId}")]
    public async Task EditMessage([FromRoute] long channelId, [FromRoute] long messageId, [FromBody] SendMessageForm messageForm)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == token.AccountId).FirstOrDefaultAsync();

        var message = await DatabaseContext.Messages.Where(x => x.ChannelId == channelId && x.MessageId == messageId).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        if (message == null)
        {
            throw new MessageNotFoundException(messageId.ToString());
        }

        if (message.AuthorId != token.AccountId)
        {
            throw new ForbiddenException($"{token.AccountId} nie ma uprawnień do edycji wiadomości innego użytkownika.");
        }

        message.Content = messageForm.Content.Trim();
        message.EditedAt = DateTime.UtcNow;

        await DatabaseContext.SaveChangesAsync();

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<ListMessagesResponse>
        {
            Type = "MessageEdited",
            Data = new ListMessagesResponse
            {
                MessageId = message.MessageId,
                Content = message.Content,
                ChannelId = message.ChannelId,
                SentAt = message.SentAt,
                EditedAt = message.EditedAt,
                Author = new AccountController.UserAccountResponse
                {
                    Id = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    AvatarUrl = user.AvatarUrl
                },
                Attachments = message.Attachments.Select(AttachmentResponse.FromDatabase).ToList()
            }
        });
    }

    [HttpDelete, Route("{channelId}/messages/{messageId}")]
    public async Task DeleteMessage([FromRoute] long channelId, [FromRoute] long messageId)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == token.AccountId).FirstOrDefaultAsync();

        var message = await DatabaseContext.Messages.Where(x => x.ChannelId == channelId && x.MessageId == messageId).FirstOrDefaultAsync();

        if (message == null)
        {
            throw new MessageNotFoundException(messageId.ToString());
        }

        if (message.AuthorId != token.AccountId)
        {
            throw new ForbiddenException($"{token.AccountId} nie ma uprawnień do usunięcia wiadomości innego użytkownika.");
        }

        foreach (var attachment in message.Attachments)
        {
            await FileService.DeleteFile(attachment.CdnNamespace, attachment.Name);
            DatabaseContext.Attachments.Remove(attachment);
        }

        DatabaseContext.Messages.Remove(message);
        await DatabaseContext.SaveChangesAsync();

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<long>
        {
            Type = "MessageDeleted",
            Data = messageId
        });
    }

    public class ListMessagesResponse
    {
        public required long MessageId { get; set; }
        public required string Content { get; set; }
        public required long ChannelId { get; set; }
        public required DateTime SentAt { get; set; }
        public DateTime? EditedAt { get; set; }
        public required UserAccountResponse Author { get; set; }
        public required List<AttachmentResponse> Attachments { get; set; }
    }

    public class ListMessagesQuery
    {
        public long? Before { get; set; }
        public long? After { get; set; }
        [Range(1, 100)]
        public int? Limit { get; set; }
    }

    [HttpGet, Route("{channelId}/messages")]
    [SwaggerResponse(200, typeof(IEnumerable<ListMessagesResponse>))]
    public async Task<IEnumerable<ListMessagesResponse>> ListMessages([FromRoute] long channelId, [FromQuery] ListMessagesQuery query)
    {
        await SessionService.VerifyRequest(HttpContext);

        var messages = await DatabaseContext.Messages
            .Include(x => x.Author)
            .Include(x => x.Attachments)
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
            ChannelId = x.ChannelId,
            SentAt = x.SentAt,
            EditedAt = x.EditedAt,
            Author = new UserAccountResponse
            {
                Id = x.Author.Id,
                Name = x.Author.Name,
                LastName = x.Author.LastName,
                Email = x.Author.Email,
                AvatarUrl = x.Author.AvatarUrl
            },
            Attachments = x.Attachments.Select(AttachmentResponse.FromDatabase).ToList()
        });
    }

    [HttpPost, Route("{channelId}/icon")]
    [OpenApiBodyParameter(["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp"])]
    public async Task<Channel> UpdateIcon(long channelId)
    {
        var token = await SessionService.VerifyRequest(HttpContext);
        var channel = await DatabaseContext.Channels.Where(x => x.ChannelId == channelId).FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        long? length = HttpContext.Request.ContentLength;
        var iconData = HttpContext.Request.Body;

        if (length == null || iconData == null)
        {
            throw new FileSizeException(0, 1024 * 1024);
        }

        if (length > 32 * 1000 * 1024)
        {
            throw new FileSizeException((long)length, 1000 * 1024);
        }

        if (channel.IconId != null)
        {
            await FileService.DeleteFile("channel-icons", channel.IconId);
        }

        using var memoryStream = await IconCreator.Generate(iconData, 512, 512);

        var fileName = $"{channel.ChannelId}.{Guid.NewGuid()}.webp";

        var iconId = await FileService.UploadFile(memoryStream, "channel-icons", fileName);
        var iconUrl = await FileService.GetFileUrl("channel-icons", iconId);

        channel.IconId = iconId;
        channel.IconUrl = iconUrl;

        await DatabaseContext.SaveChangesAsync();

        await WebSocketClient.Manager.Broadcast(new WebSocketMessage<Channel>
        {
            Type = "ChannelUpdated",
            Data = channel
        });

        return channel;
    }
}
