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
        public required bool IsDirectMessages { get; set; }
        public required bool IsPublic { get; set; }
        public List<int> Members { get; set; } = new();
    }

    public class CreateChannelResponse
    {
        public required long ChannelId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required bool IsDirectMessages { get; set; }
        public required bool IsPublic { get; set; }
        public required List<int> Members { get; set; }
    }

    [HttpPost, Route("create")]
    [SwaggerResponse(200, typeof(CreateChannelResponse))]
    public async Task<CreateChannelResponse> CreateChannel([FromBody] CreateChannelForm createChannelForm)
    {
        var session = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == session.AccountId).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new AccountNotFoundException(session.AccountId.ToString());
        }

        var channel = new Channel
        {
            Name = createChannelForm.Name.Trim(),
            Description = createChannelForm.Description.Trim(),
            Creator = user,
            CreatedAt = DateTime.UtcNow,
            IsPublic = true,
            IsDirectMessages = false
        };

        if (!createChannelForm.IsPublic && !createChannelForm.IsDirectMessages)
        {
            if (createChannelForm.Members.Count == 0 || !createChannelForm.Members.Contains(session.AccountId))
            {
                throw new InvalidMembersException();
            }

            channel.IsPublic = false;
            channel.Members = await DatabaseContext.Users.Where(x => createChannelForm.Members.Contains(x.Id)).ToListAsync();
        }
        else if (createChannelForm.IsDirectMessages)
        {
            if (createChannelForm.Members.Count != 2 || !createChannelForm.Members.Contains(session.AccountId))
            {
                throw new InvalidMembersException();
            }

            channel.IsPublic = false;
            channel.IsDirectMessages = true;
            channel.Members = await DatabaseContext.Users.Where(x => createChannelForm.Members.Contains(x.Id)).ToListAsync();
        }

        var instertedChannel = DatabaseContext.Channels.Add(channel);
        await DatabaseContext.SaveChangesAsync();

        var response = new CreateChannelResponse
        {
            ChannelId = instertedChannel.Entity.ChannelId,
            Name = instertedChannel.Entity.Name,
            Description = instertedChannel.Entity.Description,
            IsDirectMessages = instertedChannel.Entity.IsDirectMessages,
            IsPublic = instertedChannel.Entity.IsPublic,
            Members = instertedChannel.Entity.Members.Select(x => x.Id).ToList()
        };

        await WebSocketClient.Manager.BroadcastToChannel(
            instertedChannel.Entity,
            new WebSocketMessage<Channel>
            {
                Type = "NewChannel",
                Data = instertedChannel.Entity
            }
        );

        return response;
    }

    public class ChannelResponse
    {
        public required long ChannelId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required DateTime CreatedAt { get; set; }
        public UserAccountResponse? Creator { get; set; }
        public List<int> Members { get; set; } = new();
        public required bool IsPublic { get; set; }
        public required bool IsDirectMessages { get; set; }
        public string? IconUrl { get; set; }

        public static ChannelResponse FromDatabase(Channel channel)
        {
            return new ChannelResponse
            {
                ChannelId = channel.ChannelId,
                Name = channel.Name,
                Description = channel.Description,
                CreatedAt = channel.CreatedAt,
                Creator = channel.Creator == null ? null : new UserAccountResponse
                {
                    Id = channel.Creator.Id,
                    Name = channel.Creator.Name,
                    LastName = channel.Creator.LastName,
                    Email = channel.Creator.Email,
                    AvatarUrl = channel.Creator.AvatarUrl
                },
                Members = channel.Members.Select(x => x.Id).ToList(),
                IsPublic = channel.IsPublic,
                IsDirectMessages = channel.IsDirectMessages,
                IconUrl = channel.IconUrl
            };
        }
    }

    [HttpGet, Route("list")]
    [SwaggerResponse(200, typeof(IEnumerable<ChannelResponse>))]
    public async Task<IEnumerable<ChannelResponse>> ListChannels()
    {
        var session = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == session.AccountId).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new AccountNotFoundException(session.AccountId.ToString());
        }

        var channels = await DatabaseContext.Channels
            .Include(x => x.Members)
            .Include(x => x.Creator)
            .Where(x => x.IsPublic || x.Members.Contains(user))
            .ToListAsync();

        return channels.ConvertAll(ChannelResponse.FromDatabase);
    }

    [HttpGet, Route("{channelId}")]
    [SwaggerResponse(200, typeof(ChannelResponse))]
    public async Task<ChannelResponse> GetChannel([FromRoute] long channelId)
    {
        var session = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == session.AccountId).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new AccountNotFoundException(session.AccountId.ToString());
        }

        var channel = await DatabaseContext.Channels
            .Include(x => x.Members)
            .Include(x => x.Creator)
            .Where(x => x.ChannelId == channelId && (x.IsPublic || x.Members.Contains(user)))
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        return ChannelResponse.FromDatabase(channel);
    }

    [HttpDelete, Route("{channelId}")]
    public async Task DeleteChannel([FromRoute] long channelId)
    {
        var session = await SessionService.VerifyRequest(HttpContext);

        var channel = await DatabaseContext.Channels
            .Where(x => x.ChannelId == channelId && x.Creator != null && x.Creator.Id == session.AccountId)
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        await WebSocketClient.Manager.BroadcastToChannel(channel, new WebSocketMessage<long>
        {
            Type = "ChannelDeleted",
            Data = channel.ChannelId
        });

        var messages = await DatabaseContext.Messages
            .Include(x => x.Attachments)
            .Where(x => x.ChannelId == channelId)
            .ToListAsync();

        foreach (var message in messages)
        {
            foreach (var attachment in message.Attachments)
            {
                await FileService.DeleteFile(attachment.CdnNamespace, attachment.Name);
            }

            DatabaseContext.Remove(message);
        }

        DatabaseContext.Remove(channel);

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

        if (user == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        var channel = await DatabaseContext.Channels
            .Where(x => x.ChannelId == channelId && (x.IsPublic || x.Members.Contains(user)))
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
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

        if (user == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        var channel = await DatabaseContext.Channels
            .Where(x => x.ChannelId == channelId && (x.IsPublic || x.Members.Contains(user)))
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        var message = await DatabaseContext.Messages
            .Include(x => x.Attachments)
            .Where(x => x.ChannelId == channelId && x.MessageId == messageId)
            .FirstOrDefaultAsync();

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

        await WebSocketClient.Manager.BroadcastToChannel(channel, new WebSocketMessage<ListMessagesResponse>
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

        if (user == null)
        {
            throw new AccountNotFoundException(token.AccountId.ToString());
        }

        var channel = await DatabaseContext.Channels
            .Where(x => x.ChannelId == channelId && (x.IsPublic || x.Members.Contains(user)))
            .FirstOrDefaultAsync();

        if (channel == null)
        {
            throw new ChannelNotFoundException(channelId.ToString());
        }

        var message = await DatabaseContext.Messages
            .Include(x => x.Attachments)
            .Where(x => x.ChannelId == channelId && x.MessageId == messageId)
            .FirstOrDefaultAsync();

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
        }

        DatabaseContext.Remove(message);

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
        var session = await SessionService.VerifyRequest(HttpContext);
        var user = await DatabaseContext.Users.Where(x => x.Id == session.AccountId).FirstOrDefaultAsync();

        if (user == null)
        {
            throw new AccountNotFoundException(session.AccountId.ToString());
        }

        var messages = await DatabaseContext.Messages
            .Include(x => x.Author)
            .Include(x => x.Attachments)
            .Where(x => x.ChannelId == channelId && (x.Channel.IsPublic || x.Channel.Members.Contains(user)))
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
        var channel = await DatabaseContext.Channels
            .Include(x => x.Creator)
            .Where(x => x.ChannelId == channelId && x.Creator != null && x.Creator.Id == token.AccountId)
            .FirstOrDefaultAsync();

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
