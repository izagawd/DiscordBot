﻿using DiscordBotNet.Database;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.Pages.team;


[Authorize]
public class Index : PageModel
{
    
    
    public UserData UserData { get; private set; }

    public string[] TeamNames { get; protected set; } = [];
    private PostgreSqlContext DatabaseContext { get;set; }

    public Index(PostgreSqlContext context)
    {
        DatabaseContext = context;
    }

    public async Task OnPostAsync(string teamLabel)
    {
        await OnGetAsync();
        var gottenTeam = await DatabaseContext.Set<PlayerTeam>()
            .Where(i => i.Label == teamLabel && i.UserDataId == UserData.Id)
            .FirstOrDefaultAsync();

        if (gottenTeam is not null)
        {
            UserData.EquippedPlayerTeam = gottenTeam;
            await DatabaseContext.SaveChangesAsync();
        }
    }
    public async Task OnGetAsync()
    {
        var anonymous = await DatabaseContext.UserData
            .Include(i => i.EquippedPlayerTeam)
            .Include(j => j.Inventory.Where(k => k is Character))
            
            .FindOrCreateSelectAsync(User.GetDiscordUserId(),
                i => new{UserData = i,TeamNames= i.PlayerTeams.Select(j => j.Label)});
        UserData = anonymous.UserData;
        TeamNames = anonymous.TeamNames.ToArray();


        foreach (var i in UserData.Inventory.OfType<Player>())
        {
            await i.LoadAsync(User);
        }
    }


}