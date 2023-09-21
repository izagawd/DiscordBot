using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class FireBall : Skill
{
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    public override int GetMaxCooldown(int level) => 2;
    public override int MaxEnhance { get; } = 4;
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {  
        DamageResult damageResult = target.Damage(      new DamageArgs()
        {
            Damage = owner.Attack * 1.9,
            Caster = owner,
            CanCrit = true,
            DamageText =$"{owner} threw a fireball at {target} and dealt $ damage!",
        });
        if (BasicFunction.RandomChance(40))
        {
            target.StatusEffects.Add(new Burn(owner),owner.Effectiveness);
        }


        return new UsageResult(usageType, TargetType.SingleTarget) { DamageResults = new List<DamageResult> { damageResult } };
    }
}
public class Ignite : Surge
{
    public override int GetMaxCooldown(int level) => 1;

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AdditionalTexts.Add($"{owner} attempts to make a human torch out of {target}!");
        for (int i = 0; i < 3; i++)
        {
            if (BasicFunction.RandomChance(70))
            {
                target.StatusEffects.Add(new Burn(owner),owner.Effectiveness);
            }
        }
        return new UsageResult(usageType, TargetType.SingleTarget,"Ignite!");
    }
}
public class Player : Character
{
    public override bool IsInStandardBanner => false;

    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;

    [NotMapped]
    public DiscordUser User { get; set; }
    [NotMapped]
    private Surge fireSurge { get; } = new Ignite();
    [NotMapped]
    private Skill fireSkill { get; } = new FireBall();
    public override Skill Skill
    {
        get
        {
            switch (Element)
            {
                case Element.Fire:
                    return fireSkill;
                default:
                    return fireSkill;
            }
        }
    }

    public override Surge Surge
    {
        get
        {
            switch (Element)
            {
                case Element.Fire:
                    return fireSurge;
                default:
                    return fireSurge;
            }
        }
    
    }
    


    public void SetElement(Element element)
    {
        Element = element;
    }

    public override string IconUrl { get; protected set; }

    public async Task LoadAsync(ClaimsPrincipal claimsUser)
    {
        await base.LoadAsync();
        Name = claimsUser.GetDiscordUserName();
        IconUrl = claimsUser.GetDiscordUserAvatarUrl();
        if (UserData is not null)
        {
            Color = UserData.Color;
        }
    }
    public async Task LoadAsync(DiscordUser? discordUser)
    {
        await base.LoadAsync();
        if (discordUser is not null)
        {
            User = discordUser;
        } else if (User is null)
        {
            User = await Bot.Client.GetUserAsync(UserDataId);
        }

        Name = User.Username;
        IconUrl = User.GetAvatarUrl(ImageFormat.Png);
        if (UserData is not null)
        {
            Color = UserData.Color;
        } 
    }
    public override async Task LoadAsync()
    {
        await LoadAsync(discordUser: null);
    }
    public override string Name { get; protected set; }

    public override int BaseMaxHealth => 1100 + (60 * Level);
    public override int BaseAttack => 120 + (13 * Level);
    public override int BaseDefense => (100 + (5.2 * Level)).Round();
    public override int BaseSpeed => 105;


}