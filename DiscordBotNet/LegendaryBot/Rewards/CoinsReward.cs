using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Rewards;
/// <summary>
/// Rewards the user with coins
/// </summary>
public class CoinsReward : Reward
{
    public ulong Coins { get; }
    
  
    public CoinsReward(ulong coins)
    {
        Coins = coins;
    }

    public override string GiveRewardTo(UserData userData, string name)
    {
        userData.Coins += Coins;
        return $"{name} Gained {Coins} coins!";
    }
}