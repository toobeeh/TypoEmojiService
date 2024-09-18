using System.Text.Json.Serialization;

namespace tobeh.TypoEmojiService.Server.Service.ApiDtos;

public record ApiEmojiResponseDto(
    [property: JsonPropertyName("pages")] int Pages, 
    [property: JsonPropertyName("emojis")] ApiEmojiDto[] Emojis);

public record ApiEmojiDto(
    [property: JsonPropertyName("id")] string Id, 
    [property: JsonPropertyName("name")] string Name, 
    [property: JsonPropertyName("animated")] bool Animated);