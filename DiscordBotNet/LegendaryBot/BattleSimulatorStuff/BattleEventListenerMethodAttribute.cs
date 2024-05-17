namespace DiscordBotNet.LegendaryBot.BattleSimulatorStuff;

[AttributeUsage(AttributeTargets.Method)]
public class BattleEventListenerMethodAttribute : Attribute
{
    /// <summary>
    /// The higher the priority, the more likely this method is called first before any other event listener
    /// is called
    /// </summary>
    public int Priority { get; }

    public BattleEventListenerMethodAttribute(int priority = 1)
    {
        Priority = priority;
    }
}