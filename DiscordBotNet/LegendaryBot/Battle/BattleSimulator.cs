
using System.Collections.Immutable;
using System.Diagnostics;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents;
using DiscordBotNet.LegendaryBot.Battle.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Battle.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Battle.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Battle.Moves;
using DiscordBotNet.LegendaryBot.Battle.Results;
using DiscordBotNet.LegendaryBot.Battle.StatusEffects;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotNet.LegendaryBot.Battle;

public enum BattleDecision
{
    Forfeit, Surge, BasicAttack, Skill, Other,
}
public class BattleSimulator
{
   

    public static DiscordButtonComponent basicAttackButton = new(ButtonStyle.Secondary, "BasicAttack", null,emoji: new DiscordComponentEmoji("⚔️"));
    public static DiscordButtonComponent skillButton = new(ButtonStyle.Secondary, "Skill", null, emoji: new DiscordComponentEmoji("🪄"));
    public static DiscordButtonComponent surgeButton = new(ButtonStyle.Secondary, "Surge", null, emoji: new DiscordComponentEmoji("⚡"));
    public static DiscordButtonComponent forfeitButton = new(ButtonStyle.Danger, "Forfeit", "Forfeit");

    public static DiscordButtonComponent proceed = new(ButtonStyle.Success, "Proceed", "Proceed");




    /// <summary>
    /// 
    /// This will be used to invoke an event if it happens
    /// 
    /// </summary>
    /// <param name="eventArgs">The argument instance of the battle event</param>
    /// <typeparam name="T">the type of argument of the battle event</typeparam>

    public async Task<Image<Rgba32>> GetCombatImageAsync()
    {
        var image = new Image<Rgba32>(1300, 1200);

        int xOffSet = 120;
        int widest = 0;
        int length = 0;
        int yOffset = 0;
        IImageProcessingContext? imageCtx = null;

        image.Mutate(ctx => imageCtx = ctx);
        
        foreach (var i in CharacterTeams)
        {
            foreach (var j in i)
            {
        
                var characterImage = await j.GetCombatImageAsync();

                if (characterImage.Width > widest)
                {
                    widest = characterImage.Width;
                }
                imageCtx.DrawImage(characterImage, new Point(xOffSet, yOffset), new GraphicsOptions());
                yOffset += characterImage.Height + 30;
                if (yOffset > length)
                {
                    length = yOffset;
                }
                
            }

            yOffset = 0;
            xOffSet += widest + 300;
        }

        imageCtx.BackgroundColor(Color.Gray);
        var combatReadinessLineTRectangle = new Rectangle(40, 0, 15, length);
        imageCtx.Fill(Color.White, combatReadinessLineTRectangle);
        imageCtx.Draw(Color.Black, 8, combatReadinessLineTRectangle);
        foreach (var i in characters.Where(i => !i.IsDead && ActiveCharacter != i).OrderBy(i => i.CombatReadiness))
        {
            var characterImageToDraw = await BasicFunction.GetImageFromUrlAsync(i.IconUrl);
            characterImageToDraw.Mutate(mutator =>
            {
                mutator.Resize(60, 60);
                if (i.Team == Team1)
                {
                    mutator.BackgroundColor(Color.Blue);
                }
                else
                {
                    mutator.BackgroundColor(Color.Red);
                }
                mutator.ConvertToAvatar();
            });

            var characterImagePoint =
                new Point(((combatReadinessLineTRectangle.X + (combatReadinessLineTRectangle.Width / 2.0))
                           - (characterImageToDraw.Width / 2.0)).Round(),
                    (i.CombatReadiness * length / 100).Round());
            imageCtx.DrawImage(characterImageToDraw, characterImagePoint
                ,new GraphicsOptions());
            var circlePolygon = new EllipsePolygon(characterImageToDraw.Width / 2.0f + characterImagePoint.X,
                characterImageToDraw.Height / 2.0f + characterImagePoint.Y,
                characterImageToDraw.Height / 2.0f);
            imageCtx.Draw(Color.Black, 3,
                circlePolygon);
        }

        imageCtx.EntropyCrop(0.005f);

        return image;
    }
    public void InvokeBattleEvent<T>(T eventArgs) where T : EventArgs
    {
        foreach (var i in Characters)
        {
            if (i is IBattleEvent<T> iAsEvent)
            {
                iAsEvent.OnBattleEvent(eventArgs, i);
            }
            foreach (var j in i.MoveList.OfType<IBattleEvent<T>>())
            {
                j.OnBattleEvent(eventArgs,i);
            }

            foreach (var j in i.StatusEffects.OfType<IBattleEvent<T>>())
            {
                j.OnBattleEvent(eventArgs,i);
            }

            if (i.Blessing is IBattleEvent<T> blessingAsEvent)
            {
                blessingAsEvent.OnBattleEvent(eventArgs,i);
            }
        }
    }

    public StatsModifierArgs[] GetAllStatsModifierArgsInBattle()
    {
        List<StatsModifierArgs> statsModifierArgsList = new List<StatsModifierArgs>();
        foreach (var i in Characters)
        {
            if (i is IStatsModifier statsModifierCharacter)
            {
                statsModifierArgsList.AddRange(statsModifierCharacter.GetAllStatsModifierArgs(i));
            }

            foreach (var j in i.MoveList.OfType<IStatsModifier>())
            {
                statsModifierArgsList.AddRange(j.GetAllStatsModifierArgs(i));
            }
            if (i.Blessing is not null && i.Blessing is IStatsModifier statsModifierBlessing)
            {
                statsModifierArgsList.AddRange(statsModifierBlessing.GetAllStatsModifierArgs(i));
            }

            foreach (var j in i.StatusEffects.OfType<IStatsModifier>())
            {
                statsModifierArgsList.AddRange(j.GetAllStatsModifierArgs(i));
            }
        }

        return statsModifierArgsList.ToArray();
    }
    public List<CharacterTeam> CharacterTeams { get; } 

    /// <summary>
    /// The amount of turns passed
    /// </summary>
    private int _turnNumber;
    private string? _mainText;
    private string _additionalText = "";
    /// <summary>
    /// The character who is currently taking their tunr
    /// </summary>
    public Character ActiveCharacter { get; protected set; }

    /// <summary>
    /// All the characters in the battle
    /// </summary>
    private List<Character> characters =  new();
    /// <summary>
    /// All the characters in the battle
    /// </summary>
    public IEnumerable<Character> Characters => characters.OrderByDescending(i => i.CombatReadiness);
    /// <summary>
    /// Creates a new battle between two teams
    /// </summary>
    public CharacterTeam Team1 { get; }
    public CharacterTeam Team2 { get; }
    public BattleSimulator(CharacterTeam team1, CharacterTeam team2)
    {
        if (team1.Count == 0 || team2.Count == 0)
        {
            throw new Exception("one of the teams has no fighters");
        }

        Team1 = team1;
        Team2 = team2;
        CharacterTeams = new List<CharacterTeam> { team1, team2 };

    }
    private CharacterTeam? winners;



    /// <summary>
    /// Sets the winning team by checking if all the characters in a team are dead
    /// </summary>
    public void CheckForWinnerIfTeamIsDead()
    {
        foreach (var i in CharacterTeams)
        {
            if (i.All(j => j.IsDead))
            {
                winners = CharacterTeams.First(k => k != i);
                break;
            }
        }
    }
    /// <summary>
    /// Adds all the additional texts to the _additionalText variable
    /// </summary>
    public void CheckAdditionalTexts()
    {
        foreach (string i in AdditionalTexts)
        {
            if (_additionalText != "") _additionalText += "\n";
            _additionalText += i;
        }
        AdditionalTexts.Clear();
    }



    public int Turn { get; set; } = 0;
    public List<string> AdditionalTexts { get; } = new();
    /// <summary>
    /// Initiates a new battle between two teams
    /// </summary>
    public async Task<BattleResult> StartAsync(DiscordInteraction interaction, DiscordMessage? message = null)
    {
        characters.Clear();
        CharacterTeams.ForEach(i =>
        {
            foreach (var j in i)
            {
                
                j.Team = i;
                j.CurrentBattle = this;
               
            }
            
            characters.AddRange(i);
        });

        _mainText = "Battle Begins!";
        _additionalText = "Have fun!";
     
        CharacterTeam? timedOut = null;
        CharacterTeam? forfeited = null;
        
        Character? target = null;
        Turn = 0;
        while (true)
        {
 

            Turn += 1;
            while (!characters.Any(i => i.CombatReadiness >= 100 && !i.IsDead))
            {
                
                foreach (var j in characters)
                {
                   
                    if(!j.IsDead) j.CombatReadiness +=  (0.0025 * j.Speed);
                }
     
                
            }

           
            ActiveCharacter = BasicFunction.RandomChoice(characters.Where(i => i.CombatReadiness >= 100 && !i.IsDead));
            InvokeBattleEvent(new TurnStartEventArgs(ActiveCharacter));

            ActiveCharacter.CombatReadiness = 0;
            foreach (StatusEffect i in ActiveCharacter.StatusEffects.ToArray())
            {
                //this code executes for status effects that occur just before the beginning of the turn
                if (i.ExecuteStatusEffectBeforeTurn)
                {
                     i.PassTurn(ActiveCharacter);
                     if (i.Duration <= 0) ActiveCharacter.StatusEffects.Remove(i);
                }
            }
            
            if (ActiveCharacter.IsDead)
                AdditionalTexts.Add($"{ActiveCharacter} cannot take their turn because they are dead!");
            CheckAdditionalTexts();



            if (_additionalText == "") _additionalText = "No definition";
            DiscordEmbedBuilder embedToEdit = new DiscordEmbedBuilder()
                .WithTitle("**BATTLE!!!**")
                .WithAuthor(ActiveCharacter.Name, iconUrl: ActiveCharacter.IconUrl)
                .WithColor(ActiveCharacter.Color)
                .AddField(_mainText, _additionalText)
                .WithImageUrl("attachment://battle.png");
            var combatImage = await GetCombatImageAsync();
            var stream = new MemoryStream();
            await combatImage.SaveAsPngAsync(stream);
            stream.Position = 0;
            DiscordMessageBuilder messageBuilder =new DiscordMessageBuilder()
                .AddEmbed(embedToEdit.Build())
                .AddFile("battle.png", stream);
                
   
            CheckForWinnerIfTeamIsDead();
            var components = new List<DiscordComponent> {  };
            if (!(!ActiveCharacter.Team.IsPlayerTeam || ActiveCharacter.IsOverriden) 
                && !ActiveCharacter.IsDead 
                && winners is null)
            {
        
                components.Add(basicAttackButton);
                if (ActiveCharacter.Skill.CanBeUsed(ActiveCharacter))
                {
                    components.Add(skillButton);
                }
                if (ActiveCharacter.Surge.CanBeUsed(ActiveCharacter))
                {
                    components.Add(surgeButton);
                }
            }
            components.Add(forfeitButton);
            messageBuilder
                .AddComponents(components);
            if (message is null)
                message = await interaction.Channel.SendMessageAsync(messageBuilder);
            else message = await message.ModifyAsync(messageBuilder);

            _mainText = $"{ActiveCharacter} is thinking of a course of action...";
            if (winners is not null)
            {
                await Task.Delay(5000); break;
            }

            BattleDecision decision =BattleDecision.Other;
 
            StatusEffect? mostPowerfulStatusEffect = null;
            if (ActiveCharacter.StatusEffects.Any())
                mostPowerfulStatusEffect = ActiveCharacter.StatusEffects.OrderByDescending(i => i.OverrideTurnType).First();
            if (ActiveCharacter.IsDead) await Task.Delay(5000);
 
            else if ( decision != BattleDecision.Forfeit && mostPowerfulStatusEffect is not null &&  mostPowerfulStatusEffect.OverrideTurnType > 0 )
            {

                UsageResult overridenUsage  = mostPowerfulStatusEffect.OverridenUsage(ActiveCharacter,ref target, ref decision, UsageType.NormalUsage);
                if (overridenUsage.Text is not null) _mainText = overridenUsage.Text;
           
                await Task.Delay(5000);
          
            }
            
            else if (!ActiveCharacter.Team.IsPlayerTeam)
            {
               ActiveCharacter.NonPlayerCharacterAi(ref target, ref decision);
               var buttonAwaitercancellationToken = new CancellationTokenSource();
               var delayCancellationToken = new CancellationTokenSource();

               message.WaitForButtonAsync(e =>
               {
                   var didParse = Enum.TryParse(e.Interaction.Data.CustomId, out decision);
                   if (!didParse) return false;
                   if (decision == BattleDecision.Forfeit)
                   {
                       var forfeitedTeam = CharacterTeams.FirstOrDefault(i => i.UserId == e.User.Id);
                       if (forfeitedTeam is not null)
                       {
                           forfeited = forfeitedTeam;
                           e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                           delayCancellationToken.Cancel();
                           return true;
                       }
                   }

                   return false;
               }, buttonAwaitercancellationToken.Token);
               try
               {
                   await Task.Delay(5000,delayCancellationToken.Token);
               }
               catch
               {
                   // ignored
               }
                
               buttonAwaitercancellationToken.Cancel();
               delayCancellationToken.Dispose();
               buttonAwaitercancellationToken.Dispose();
            }
            else
            {
  
                var results = await message.WaitForButtonAsync(e =>
                {
                    var didParse = Enum.TryParse(e.Interaction.Data.CustomId, out decision);
                    if (!didParse) return false;
                    if (decision == BattleDecision.Forfeit)
                    {
                        var forfeitedTeam = CharacterTeams.FirstOrDefault(i => i.UserId == e.User.Id);
                        if (forfeitedTeam is not null)
                        {
                            forfeited = forfeitedTeam;
                            e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            return true;
                        }
                    }
                    if (e.User.Id == ActiveCharacter.Team.UserId)
                    {

                        e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        return true;
                    }
                    return false;
                });
             
                if (results.TimedOut)
                {
                    timedOut = ActiveCharacter.Team;
                    winners = CharacterTeams.First(i => i != ActiveCharacter.Team);
                    break;
                }
                List<DiscordSelectComponentOption> enemiesToSelect = new();
                List<Character> possibleTargets = new List<Character>();
                
                if ( ActiveCharacter[decision] is Move theMove)
                {
                    possibleTargets.AddRange(theMove.GetPossibleTargets(ActiveCharacter));
                    foreach (var i in possibleTargets)
                    {
                        bool isEnemy = i.Team != ActiveCharacter.Team;
                        enemiesToSelect.Add(new DiscordSelectComponentOption(i.GetNameWithPosition(isEnemy), i.GetNameWithPosition(isEnemy)));
                    }
                        
                }
                if (enemiesToSelect.Any())
                {
                    target = possibleTargets.First(i => i.GetNameWithPosition(i.Team != ActiveCharacter.Team) == enemiesToSelect.First().Value);
                    DiscordSelectComponent selectTarget = new("targeter",target.GetNameWithPosition(target.Team != ActiveCharacter.Team), enemiesToSelect);
                    messageBuilder = new DiscordMessageBuilder()
                        .AddComponents(proceed)
                        .AddComponents(selectTarget)
                        .AddEmbed(embedToEdit.Build());
                    message = await message.ModifyAsync(messageBuilder);
                    CancellationTokenSource buttonAwaiterToken = new CancellationTokenSource();
                    message.WaitForSelectAsync(e =>
                    {
                        if (e.User.Id == ActiveCharacter.Team.UserId)
                        {
                            target = characters.First(i => i.GetNameWithPosition(i.Team != ActiveCharacter.Team) == e.Values.First().ToString());
                            e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        }
                        return false;
                    },buttonAwaiterToken.Token);
                
                    var results1 = await  message.WaitForButtonAsync(e =>
                    {
                        if (e.User.Id == ActiveCharacter.Team.UserId && e.Interaction.Data.CustomId == "Proceed")
                        {
                            buttonAwaiterToken.Cancel();
                            e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            return true;
                        }
                        return false;
                    });
                    buttonAwaiterToken.Dispose();
                    if (results1.TimedOut)
                    {
                        timedOut = ActiveCharacter.Team;
                        winners = CharacterTeams.First(i => i != ActiveCharacter.Team);
                        break;
                    }
                }
            }

            if (forfeited is not null)
            {
                winners = CharacterTeams.First(i => i != forfeited);
                break;
            }
            if (winners is not null)
            {
            
                await Task.Delay(5000); break;
                
            }
            var move = ActiveCharacter[decision];
   


            if (move is not null)
            {
                var moveResult =  move.Utilize(ActiveCharacter,target, UsageType.NormalUsage);
                
                if (moveResult.Text is not null)
                {
                    _mainText = moveResult.Text;
                }
            }


            _additionalText = "";


            foreach (var i in ActiveCharacter.MoveList)
            {
                if (i is Special special && special.Cooldown > 0)
                {
                    special.Cooldown -= 1;
                }
            }
            InvokeBattleEvent(new TurnEndEventArgs(ActiveCharacter));
            foreach (StatusEffect i in ActiveCharacter.StatusEffects.ToArray())
            {
                if (i.ExecuteStatusEffectAfterTurn)
                {
                    i.PassTurn(ActiveCharacter);
                    if (i.Duration <= 0) ActiveCharacter.StatusEffects.Remove(i);
                }
            }
            CheckAdditionalTexts();

        }

        
        return new BattleResult
        {
            Turns = Turn,
            Forfeited = forfeited,
            Winners = winners,
            TimedOut = timedOut
        };
    }
}