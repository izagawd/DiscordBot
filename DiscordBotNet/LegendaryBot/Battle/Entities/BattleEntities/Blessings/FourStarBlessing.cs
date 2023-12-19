namespace DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Blessings;

public class FourStarBlessing : Blessing
{
    /// <summary>
    /// The description of the blessing in relation to the level provided
    /// </summary>


    public override Rarity Rarity { get; protected set; } = Rarity.FourStar;

    public override string GetDescription(int level)
    {
        return "";
    }
}