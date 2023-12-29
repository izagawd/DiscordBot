using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

namespace DiscordBotNet.LegendaryBot.BattleEvents;

public class BattleEventFunc<T> : IBattleEvent<T> where T : BattleEventArgs
{

    private Action<T, Character> _funcToUse;
    public BattleEventFunc(Action<T, Character> funcToUse)
    {
        _funcToUse = funcToUse;
    }
    public void OnBattleEvent(T eventArgs, Character owner)
    {
        _funcToUse?.Invoke(eventArgs, owner);
    }

}