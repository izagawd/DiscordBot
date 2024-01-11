using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
public class FourthWallBreaker: BasicAttack
{
    public override string GetDescription(Character character) =>  "Damages the enemy by breaking the fourth wall";
    

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        return new UsageResult(this)
        {
            DamageResults = [
                target.Damage(new DamageArgs(this)
                {

                    Caster = owner,
                    DamageText =
                        $"Breaks the fourth wall, causing {target} to cringe, and making them receive $ damage!",
                    Damage = owner.Attack * 1.7

                })
            ],
            User = owner,
            TargetType = TargetType.SingleTarget,
            Text = "It's the power of being a real human",
            UsageType = usageType

        };
    }
}

public class FireBall : Skill
{
    public override string GetDescription(Character character) => "Throws a fire ball at the enemy with a 40% chance to inflict burn";
    

    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team && !i.IsDead);
    }

    public override int MaxCooldown=> 2;
  
    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {  
        DamageResult damageResult = target.Damage(      new DamageArgs(this)
        {
            Damage = owner.Attack * 2.4,
            Caster = owner,
            CanCrit = true,
            DamageText =$"{owner} threw a fireball at {target} and dealt $ damage!",
        });
        if (BasicFunction.RandomChance(40))
        {
            target.AddStatusEffect(new Burn(owner),owner.Effectiveness);
        }


        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            User = owner,
            DamageResults =[damageResult]
        };
    }
}
public class Ignite : Surge
{
    public override int MaxCooldown => 1;
    public override string GetDescription(Character character) =>$"Ignites the enemy with 3 burns. {IgniteChance}% chance each";
    

    public int IgniteChance  => 100;
    public override IEnumerable<Character> GetPossibleTargets(Character owner)
    {
        return owner.CurrentBattle.Characters.Where(i => i.Team != owner.Team&& !i.IsDead);
    }

    protected override UsageResult HiddenUtilize(Character owner, Character target, UsageType usageType)
    {
        owner.CurrentBattle.AddAdditionalBattleText(new($"{owner} attempts to make a human torch out of {target}!"));
        for (int i = 0; i < 3; i++)
        {
            if (BasicFunction.RandomChance(IgniteChance))
            {
                target.AddStatusEffect(new Burn(owner),owner.Effectiveness);
            }
        }

        return new UsageResult(this)
        {
            UsageType = usageType,
            TargetType = TargetType.SingleTarget,
            Text = "Ignite!",
            User = owner
        };
    }
}
public class Player : Character
{
    public override int GetSpeedValue(int points)
    {
        return (base.GetSpeedValue(points) * 1.02).Round();
    }

    public override bool IsInStandardBanner => false;
    public override Rarity Rarity { get; protected set; } = Rarity.FiveStar;

    
    [NotMapped]
    public DiscordUser User { get; set; }
    [NotMapped]
    private Surge fireSurge { get; } = new Ignite();
    [NotMapped]
    private Skill fireSkill { get; } = new FireBall();
    public override Skill? Skill
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

    public override BasicAttack BasicAttack { get; } = new FourthWallBreaker();


    public override Surge? Surge
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
    public override Task<Image<Rgba32>> GetDetailsImageAsync(bool loadBuild)
    {
    
        return GetDetailsImageAsync(discordUser: null,loadBuild);
    }
    public async Task<Image<Rgba32>> GetDetailsImageAsync(DiscordUser? discordUser,bool loadBuild)
    {
        await LoadAsync(discordUser, loadBuild);
        return await base.GetDetailsImageAsync(loadBuild);
    }
    public async Task<Image<Rgba32>> GetDetailsImageAsync(ClaimsPrincipal claimsUser,bool loadBuild)
    {
        await LoadAsync(claimsUser, loadBuild);
        return await base.GetDetailsImageAsync(loadBuild);
    }
    public async Task LoadAsync(ClaimsPrincipal claimsUser, bool build = true)
    {
        await base.LoadAsync(build);
        Name = claimsUser.GetDiscordUserName();
        IconUrl = claimsUser.GetDiscordUserAvatarUrl();
        if (UserData is not null)
        {
            Color = UserData.Color;
        }
    }
    public async Task LoadAsync(DiscordUser? discordUser, bool build = true)
    {
        await base.LoadAsync(build);
        if (discordUser is not null)
        {
            User = discordUser;
        } else if (User is null)
        {
            User = await Bot.Client.GetUserAsync((ulong)UserDataId);
        }

        Name = User.Username;
        IconUrl = User.AvatarUrl;
        if (UserData is not null)
        {
            Color = UserData.Color;
        } 
    }
    public override Task LoadAsync(bool build)
    {
        return LoadAsync(discordUser: null,build);
    }
    public override string Name { get; protected set; }



}