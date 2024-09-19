using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using tobeh.EmojiService;
using tobeh.TypoEmojiService.Server.Service;
using tobeh.TypoEmojiService.Server.Service.ApiDtos;

namespace tobeh.TypoEmojiService.Server.Grpc;

public class EmojisGrpcService(ILogger<EmojisGrpcService> logger, EmojiApiScraper emojiApiScraper, SavedEmojiService savedEmojiService) : Emojis.EmojisBase
{
    public override async Task LoadNewEmojiCandidates(SearchEmojisMessage request, IServerStreamWriter<EmojiCandidateMessage> responseStream,
        ServerCallContext context)
    {
        logger.LogTrace("LoadNewEmojiCandidates({request})", request);
        
        if(request.Static == request.Animated)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Either static or animated mode must be enabled, but not both"));
        }

        var count = 0;
        var nextPage = 1;
        while (count < request.MaxCount)
        {
            var candidates = await emojiApiScraper
                .SearchEmojis(request.Name, request.Animated, nextPage);
            if(candidates.Count == 0) break;
            
            foreach (var candidate in candidates)
            {
                if (count >= request.MaxCount) break;
                
                var url = $"https://cdn.discordapp.com/emojis/{candidate.Id}.{(candidate.Animated ? "gif" : "png")}";
                if(await savedEmojiService.HasEmoji(url)) continue;
                
                await responseStream.WriteAsync(new EmojiCandidateMessage
                {
                    Name = candidate.Name,
                    Url = url,
                    Animated = candidate.Animated
                });
                count++;
            }
            
            nextPage++;
        }
    }

    public override async Task ListEmojis(SearchEmojisMessage request, IServerStreamWriter<EmojiMessage> responseStream, ServerCallContext context)
    {
        logger.LogTrace("ListEmojis({request})", request);

        var emojis = await savedEmojiService.SearchEmojis(request.Name, request.Animated, request.Static, request.MaxCount);
        foreach (var emoji in emojis)
        {
            await responseStream.WriteAsync(new EmojiMessage
            {
                Id = new EmojiIdentificationMessage { Name = emoji.Name, NameId = emoji.Id },
                Url = emoji.Url,
                Animated = emoji.Animated
            });
        }
    }

    public override async Task<EmojiIdentificationMessage> AddEmoji(EmojiCandidateMessage request, ServerCallContext context)
    {
        logger.LogTrace("AddEmoji({request})", request);
        
        if(await savedEmojiService.HasEmoji(request.Url))
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Emoji already with same image exists"));
        }
        
        var emoji = await savedEmojiService.SaveEmoji(request.Name, request.Url, request.Animated);
        return new EmojiIdentificationMessage { Name = emoji.Name, NameId = emoji.Id };
    }

    public override async Task<Empty> RemoveEmoji(EmojiIdentificationMessage request, ServerCallContext context)
    {
        logger.LogTrace("RemoveEmoji({request})", request);
        
        await savedEmojiService.RemoveEmoji(request.Name, request.NameId);
        return new Empty();
    }

    public override async Task<EmojiMessage> GetEmoji(EmojiIdentificationMessage request, ServerCallContext context)
    {
        logger.LogTrace("GetEmoji({request})", request);
        
        var emoji = await savedEmojiService.GetEmoji(request.Name, request.NameId);
        return new EmojiMessage
        {
            Id = new EmojiIdentificationMessage { Name = emoji.Name, NameId = emoji.Id },
            Url = emoji.Url,
            Animated = emoji.Animated
        };
    }
}