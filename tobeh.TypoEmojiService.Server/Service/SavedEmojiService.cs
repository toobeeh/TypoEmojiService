using Microsoft.EntityFrameworkCore;
using tobeh.TypoEmojiService.Server.Database;
using tobeh.TypoEmojiService.Server.Database.Model;

namespace tobeh.TypoEmojiService.Server.Service;

public class SavedEmojiService(ILogger<SavedEmojiService> logger, AppDatabaseContext db)
{
    public async Task<EmojiEntity> SaveEmoji(string name, string url, bool animated)
    {
        logger.LogTrace("SaveEmoji(name={name}, url={url}, animated={animated})", name, url, animated);

        var lastNameId = await db.Emojis.Where(emoji => emoji.Name == name).MaxAsync(emoji => (int?)emoji.Id);
        
        var emoji = new EmojiEntity
        {
            Name = name,
            Id = lastNameId is {} id ? id + 1 : 0,
            Url = url,
            Animated = animated
        };
        
        db.Emojis.Add(emoji);
        await db.SaveChangesAsync();
        return emoji;
    }

    public async Task RemoveEmoji(string name, int nameId)
    {
        logger.LogTrace("RemoveEmoji(name={name}, nameId={nameId})", name, nameId);
        
        var emoji = await db.Emojis.FirstOrDefaultAsync(emoji => emoji.Name == name && emoji.Id == nameId);
        if (emoji == null)
        {
            throw new InvalidOperationException("Emoji not found");
        }
        
        db.Emojis.Remove(emoji);
        await db.SaveChangesAsync();
    }

    public async Task<List<EmojiEntity>> SearchEmojis(string name, bool animated, bool statics, int maxCount)
    {
        logger.LogTrace("SearchEmojis(name={name}, animated={animated}, statics={statics}, maxCount={maxCount})", name, animated, statics, maxCount);

        // If both animated and statics are false, search for both
        if (statics == false && animated == false) statics = animated = true;
        
        var emojis = await db.Emojis
            .Where(emoji => emoji.Name.Contains(name) && (emoji.Animated == animated || emoji.Animated != statics))
            .Take(maxCount)
            .ToListAsync();

        return emojis;
    }
    
    public async Task<bool> HasEmoji(string url)
    {
        logger.LogTrace("HasEmoji(url={url})", url);
        
        return await db.Emojis.AnyAsync(emoji => emoji.Url == url);
    }
    
    public async Task<EmojiEntity> GetEmoji(string name, int nameId)
    {
        logger.LogTrace("GetEmoji(name={name}, nameId={nameId})", name, nameId);
        
        var emoji = await db.Emojis.FirstOrDefaultAsync(emoji => emoji.Name == name && emoji.Id == nameId);
        if (emoji == null)
        {
            throw new InvalidOperationException("Emoji not found");
        }

        return emoji;
    }
    
}