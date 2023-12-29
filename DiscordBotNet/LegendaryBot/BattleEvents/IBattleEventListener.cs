using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.BattleEvents;


public delegate void BattleEventDelegate(BattleEventArgs eventArgs, Character owner);

/// <summary>
/// This interface should be applied if an event wants to be used in an item, a battle, or whatever
/// </summary>
/// <typeparam name="T">the arguments of the events. this is v3ry important because if the wrong one is used, the wrong event would be listened for</typeparam>
public interface IBattleEventListener
   
{

    public void OnBattleEvent(BattleEventArgs eventArgs, Character owner);
}

