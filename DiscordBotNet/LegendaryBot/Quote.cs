using System.Data.Entity.ModelConfiguration.Conventions;
using System.Numerics;
using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;

namespace DiscordBotNet.LegendaryBot;

public class QuoteReaction
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public ulong UserDataId { get; set; } 
    public Guid QuoteId { get; set; }
    /// <summary>
    /// The quote that was reacted to
    /// </summary>
    public Quote Quote { get; set; }

    public bool IsLike { get; set; } = true;
    
    public UserData UserData { get; set; }
}
public class Quote
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public bool IsPermitted { get; set; } = false;
    public ulong UserDataId { get; set; }
    public string QuoteValue { get; set; } = "Nothing";
    public UserData UserData { get; set; }
    public List<QuoteReaction> QuoteReactions { get; set; } = new();

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public override string ToString() => QuoteValue;

    public Image<Rgba32> GetImage(Image<Rgba32> quoteUserImage, int likes, int dislikes)
    {
        var image = new Image<Rgba32>(400, 300);
        quoteUserImage = quoteUserImage.Clone();
        quoteUserImage.Mutate(i => i.Resize(60,60));
        image.Mutate(mutateCtx =>
        {
            mutateCtx.BackgroundColor(Color.Blue);
            mutateCtx.DrawImage(quoteUserImage,
                new Point(image.Width/2 - 30,30),1);
            var font = SystemFonts.CreateFont("Arial", 10);
            
            RichTextOptions options = new RichTextOptions(font){WrappingLength = 350};
            options.Origin = new Vector2(20, 100);
            font = SystemFonts.CreateFont("Arial", 20);
            options.Font = font;
            mutateCtx.DrawText(options, $"likes: {likes} dislikes: {dislikes}", Color.Black);
            options.Origin = new Vector2(20, 140);
           mutateCtx.DrawText(options, QuoteValue + $"\n\nDate and time created: {DateCreated:MM/dd/yyyy HH:mm:ss}", Color.Black);
            
       
        });
        return image;
    }

    public Quote(string quote) : this()
    {
        QuoteValue = quote;
    }
    public Quote(){}
}