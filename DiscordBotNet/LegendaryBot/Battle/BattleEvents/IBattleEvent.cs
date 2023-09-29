using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.Battle.BattleEvents;
/// <summary>
/// This interface should be applied if an event wants to be used in an item, a battle, or whatever
/// </summary>
/// <typeparam name="T">the arguments of the events. this is v3ry important because if the wrong one is used, the wrong event would be listened for</typeparam>
public interface IBattleEvent<T> where T : System.EventArgs
{
    public void OnBattleEvent(T eventArgs, Character owner);
}