using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

public class Slime : Character
{
    public override Rarity Rarity { get; protected set; } = Rarity.TwoStar;
    public override int BaseMaxHealth => 500 + (Level / 4 * 150);
    public override int BaseAttack => ((336 + (67.2 * Level / 7)) / 1.75).Round();
    public override int BaseSpeed => 80;
    public override int BaseDefense => (336 + (67.2 * Level / 5)).Round();
    public override DiscordColor Color => DiscordColor.Blue;


}