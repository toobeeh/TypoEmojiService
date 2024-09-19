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
                await responseStream.WriteAsync(new EmojiCandidateMessage
                {
                    Name = candidate.Name,
                    Url = $"https://cdn.discordapp.com/emojis/{candidate.Name}.{(candidate.Animated ? "gif" : "png")}",
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
        var emoji = await savedEmojiService.SaveEmoji(request.Name, request.Url, request.Animated);
        return new EmojiIdentificationMessage { Name = emoji.Name, NameId = emoji.Id };
    }

    public override async Task<Empty> RemoveEmoji(EmojiIdentificationMessage request, ServerCallContext context)
    {
        await savedEmojiService.RemoveEmoji(request.Name, request.NameId);
        return new Empty();
    }
}