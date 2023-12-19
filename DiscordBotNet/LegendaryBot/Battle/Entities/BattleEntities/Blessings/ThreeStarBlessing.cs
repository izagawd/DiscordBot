namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;

public  class ThreeStarBlessing : Blessing
{
    /// <summary>
    /// The description of the blessing in relation to the level provided
    /// </summary>

    public override string GetDescription(int level)
    {
        return "";
    }

    public override Rarity Rarity { get; protected set; } = Rarity.ThreeStar;

}