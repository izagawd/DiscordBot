using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.Results;

namespace DiscordBotNet.LegendaryBot.Battle.Moves;

public abstract class Special : Move
{
    /// <summary>
    /// The maximum cooldown of the move based on the move level
    /// </summary>

    public virtual int GetMaxCooldown(int level) => 3;

    public int GetMaxCooldown(Character character)
    {
        switch(MoveType){
            case MoveType.Skill:
                return GetMaxCooldown(character.SkillLevel);
            case MoveType.Surge:
                return GetMaxCooldown(character.SurgeLevel);
            default:
                return 0;
        }
    }
    /// <summary>
    /// The cooldown of the move
    /// </summary>
    public int Cooldown { get; set; } = 0;
    public bool IsOnCooldown => Cooldown > 0;
    public bool IsDefault => _default_types.Contains(GetType());

    public sealed override bool CanBeUsed(Character owner)
    {
        return base.CanBeUsed(owner) && !IsDefault && !IsOnCooldown;
    }

    public override string ToString()
    {
        if (IsOnCooldown)
            return base.ToString() + $" ({Cooldown})";
        return base.ToString();
    }

    public sealed override UsageResult Utilize(Character owner, Character target, UsageType usageType)
    {
        if (usageType == UsageType.NormalUsage)
        {
            switch (MoveType)
            {
                case MoveType.Skill:
                    Cooldown = GetMaxCooldown(owner.SkillLevel);
                    break;
                case MoveType.Surge:
                    Cooldown = GetMaxCooldown(owner.SkillLevel);
                    break;
            }
        }
        return base.Utilize(owner, target, usageType);
    }
}