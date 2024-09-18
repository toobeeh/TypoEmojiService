using System.Web;
using Microsoft.Extensions.Options;
using tobeh.TypoEmojiService.Server.Config;
using tobeh.TypoEmojiService.Server.Service.ApiDtos;

namespace tobeh.TypoEmojiService.Server.Service;

public class EmojiApiScraper
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmojiApiScraper> _logger;
    
    public EmojiApiScraper(IOptions<ScraperConfig> config, IHttpClientFactory httpClientFactory, ILogger<EmojiApiScraper> logger)
    {
        var config1 = config.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(config1.ApiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
    }
    
    public async Task<IList<ApiEmojiDto>> SearchEmojis(string name, bool animated, int page = 1)
    {
        _logger.LogTrace("SearchEmojis(name={name}, animated={animated})", name, animated);
        
        var response = await _httpClient.GetAsync($"emoji/search?query={HttpUtility.UrlEncode(name)}&page={page}&type={(animated ? "animated" : "static")}");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogDebug("Api returned error: {statusCode}", response.StatusCode);
            return [];
        }
        
        var content = await response.Content.ReadFromJsonAsync<ApiEmojiResponseDto>() ?? throw new InvalidOperationException("Failed to parse api response");
        return content.Emojis;
    }
}