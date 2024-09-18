using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace tobeh.TypoEmojiService.Server.Database.Model;

[PrimaryKey(nameof(Name), nameof(Id))]
public class EmojiEntity
{
    public string Name { get; set; }
    
    public int Id { get; set; }
    
    public string Url { get; set; }
    
    public bool Animated { get; set; }
}