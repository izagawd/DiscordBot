﻿using System.Data.Entity.ModelConfiguration.Conventions;
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



    public Quote(string quote) : this()
    {
        QuoteValue = quote;
    }
    public Quote(){}
}