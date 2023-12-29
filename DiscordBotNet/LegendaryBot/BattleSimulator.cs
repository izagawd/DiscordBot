using System.Collections.Concurrent;
using System.Diagnostics;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot;

public enum BattleDecision
{
    
    Forfeit, Surge, BasicAttack, Skill, Other,
}


public class BattleSimulator
{
   
    public struct CharacterWithImage
    {
        public Character Character;
        public Image<Rgba32> Image;
    }
    public static DiscordButtonComponent basicAttackButton = new(ButtonStyle.Secondary, nameof(BasicAttack), null,emoji: new DiscordComponentEmoji("⚔️"));
    public static DiscordButtonComponent skillButton = new(ButtonStyle.Secondary, nameof(Skill), null, emoji: new DiscordComponentEmoji("🪄"));
    public static DiscordButtonComponent surgeButton = new(ButtonStyle.Secondary, nameof(Surge), null, emoji: new DiscordComponentEmoji("⚡"));
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

        var stop = new Stopwatch(); stop.Start();
        var heightToUse = CharacterTeams.Select(i => i.Count).Max() * 160;
        var image = new Image<Rgba32>(500, heightToUse);
        int xOffSet = 70;
        int widest = 0;
        int length = 0;
        int yOffset = 0;
        IImageProcessingContext imageCtx = null!;
        image.Mutate(ctx => imageCtx = ctx);
        var characterImagesInTeam1 = new ConcurrentDictionary<Character,Image<Rgba32>>();
        var characterImagesInTeam2 = new ConcurrentDictionary<Character, Image<Rgba32>>();
        await Parallel.ForEachAsync(Characters, async (character, token) =>
        {
            if (character.Team == Team1)
                characterImagesInTeam1[character] = await character.GetCombatImageAsync();
            else
                characterImagesInTeam2[character] = await character.GetCombatImageAsync();
        });
        foreach (var i in CharacterTeams)
        {
            var characterImagesInTeam = characterImagesInTeam1;
            if (i == Team2)
                characterImagesInTeam = characterImagesInTeam2;
            foreach (var j in i)
            {
                var gottenImage = characterImagesInTeam[j];
                if (gottenImage.Width > widest)
                {
                    widest = gottenImage.Width;
                }
                imageCtx.DrawImage(gottenImage, new Point(xOffSet, yOffset), new GraphicsOptions());
                yOffset += gottenImage.Height + 15;
                if (yOffset > length)
                {
                    length = yOffset;
                }
                gottenImage.Dispose();
            }
            yOffset = 0;
            xOffSet += widest + 75;
        }
        imageCtx.BackgroundColor(Color.Gray);
        var combatReadinessLineTRectangle = new Rectangle(30, 0, 3, length);
        imageCtx.Draw(Color.Black, 8, combatReadinessLineTRectangle);
        imageCtx.Fill(Color.White, combatReadinessLineTRectangle);   

        foreach (var i in Characters.Where(i => !i.IsDead && ActiveCharacter != i).OrderBy(i => i.CombatReadiness))
        {
            using var characterImageToDraw = await BasicFunction.GetImageFromUrlAsync(i.IconUrl);
            var circleColor = Color.Blue;
            if (i.Team == Team2)
                circleColor = Color.Red;
            characterImageToDraw.Mutate(mutator =>
            {
                var circleBgColor = Color.DarkBlue;
                if (i.Team == Team2)
                    circleBgColor = Color.DarkRed;
                mutator.Resize(30, 30);
                mutator.BackgroundColor(circleBgColor);
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
            imageCtx.Draw(circleColor, 2,
                circlePolygon);
        }

        imageCtx.EntropyCrop();
        stop.Elapsed.TotalMilliseconds.Print();
        return image;
    }

    public void InvokeBattleEvent<T>(T eventArgs) where T : BattleEventArgs
    {

        if(this is IBattleEvent<T> battleSimulatorEvent)
            battleSimulatorEvent.OnBattleEvent(eventArgs,null);
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

    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgsInBattle()
    {
        List<StatsModifierArgs> statsModifierArgsList = [];
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

        return statsModifierArgsList;
    }

    public IEnumerable<CharacterTeam> CharacterTeams => new[] { Team1, Team2 };


    private string? _mainText;
    private string _additionalText = "";
    /// <summary>
    /// The character who is currently taking their tunr
    /// </summary>
    public Character ActiveCharacter { get; protected set; }



    /// <summary>
    /// All the characters in the battle
    /// </summary>
    public IEnumerable<Character> Characters => Team1.Union(Team2);
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



    }
    private CharacterTeam? _winners;



    /// <summary>
    /// Sets the winning team by checking if all the characters in a team are dead
    /// </summary>
    public void CheckForWinnerIfTeamIsDead()
    {
        foreach (var i in CharacterTeams)
        {
            if (!i.All(j => j.IsDead)) continue;
            
            _winners = CharacterTeams.First(k => k != i);
            break;
        }
    }
    /// <summary>
    /// Adds all the additional texts to the _additionalText variable
    /// </summary>
    public void CheckAdditionalTexts()
    {
        foreach (var i in AdditionalTexts)
        {
            if (_additionalText != "") _additionalText += "\n";
            _additionalText += i;
        }

        if (_additionalText.Length > 1024)
            _additionalText = _additionalText.Substring(0, 1021) + "...";

        AdditionalTexts.Clear();
    }



    private DiscordMessage _message;
    /// <summary>
    /// The team that has forfeited
    /// </summary>
    private CharacterTeam? _forfeited;
    private async Task CheckForForfeitAsync()
    {
        using var buttonAwaitercancellationToken = new CancellationTokenSource();
        using var delayCancellationToken = new CancellationTokenSource();

        _message.WaitForButtonAsync(e =>
        {
            var didParse = Enum.TryParse(e.Interaction.Data.CustomId, out BattleDecision decision);
            if (!didParse) return false;
            if (decision == BattleDecision.Forfeit)
            {
                var forfeitedTeam = CharacterTeams.FirstOrDefault(i => i.UserId == e.User.Id);
                if (forfeitedTeam is not null)
                {
                    _forfeited = forfeitedTeam;
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
    }
    public int Turn { get; set; } = 0;
    public List<string> AdditionalTexts { get; } = new();
    /// <summary>
    /// Initiates a new battle between two teams
    /// </summary>
    public async Task<BattleResult> StartAsync(InteractionContext context, DiscordMessage? message = null)
    {
        _message = message;
        Team1.CurrentBattle = this;
        Team2.CurrentBattle = this;
        foreach (var i in CharacterTeams)
        {
            foreach (var j in i)
            {
                j.Team = i;

            }
        }
        _mainText = "Battle Begins!";
        _additionalText = "Have fun!";
     
        CharacterTeam? timedOut = null;

        
        Character? target = null;
        Turn = 0;
        while (true)
        {
            Turn += 1;
            bool extraTurnGranted = false;
            var extraTurners =
                Characters.Where(i => i.ShouldTakeExtraTurn)
                    .ToArray();
            
            if (extraTurners.Any())
            {
                ActiveCharacter = BasicFunction.RandomChoice(extraTurners.AsEnumerable());
                ActiveCharacter.ShouldTakeExtraTurn = false;
                extraTurnGranted = true;
            }
            while (!Characters.Any(i => i.CombatReadiness >= 100 && !i.IsDead) && !extraTurnGranted)
            {
                
                foreach (var j in Characters)
                {
                   
                    if(!j.IsDead) j.CombatReadiness +=  (0.0025 * j.Speed);
                }
     
                
            }

            if (!extraTurnGranted)
            {
                ActiveCharacter = BasicFunction.RandomChoice(Characters.Where(i => i.CombatReadiness >= 100 && !i.IsDead));

            }
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
            var name = "";
            if (ActiveCharacter.Team.UserName is not null)
            {
                name = $" ({ActiveCharacter.Team.UserName})";
            }
            DiscordEmbedBuilder embedToEdit = new DiscordEmbedBuilder()
                .WithTitle("**BATTLE!!!**")
                .WithAuthor($"{ActiveCharacter.Name}{name}", iconUrl: ActiveCharacter.IconUrl)
                .WithColor(ActiveCharacter.Color)
                .AddField(_mainText, _additionalText)
                .WithImageUrl("attachment://battle.png");
            using var combatImage = await GetCombatImageAsync();
    
            await using var stream = new MemoryStream();
            await combatImage.SaveAsPngAsync(stream);
       
            stream.Position = 0;
            DiscordMessageBuilder messageBuilder =new DiscordMessageBuilder()
                .AddEmbed(embedToEdit.Build())
                .AddFile("battle.png", stream);
                
       
            CheckForWinnerIfTeamIsDead();

            var components = new List<DiscordComponent>();
            if (!(!ActiveCharacter.Team.IsPlayerTeam || ActiveCharacter.IsOverriden) 
                && !ActiveCharacter.IsDead 
                && _winners is null)
            {
        
                components.Add(basicAttackButton);
                if (ActiveCharacter.Skill is not null &&  ActiveCharacter.Skill.CanBeUsed(ActiveCharacter))
                {
                    components.Add(skillButton);
                }
                if (ActiveCharacter.Surge is not null && ActiveCharacter.Surge.CanBeUsed(ActiveCharacter))
                {
                    components.Add(surgeButton);
                }
            }
          
            components.Add(forfeitButton);
            messageBuilder
                .AddComponents(components);
            if (_message is null)
                _message = await context.Channel.SendMessageAsync(messageBuilder);
            else _message = await _message.ModifyAsync(messageBuilder);

            _mainText = $"{ActiveCharacter} is thinking of a course of action...";
            if (_winners is not null) 
            {
                await Task.Delay(5000); break;
            }

            var decision =BattleDecision.Other;
 
            StatusEffect? mostPowerfulStatusEffect = null;
            if (ActiveCharacter.StatusEffects.Any())
                mostPowerfulStatusEffect = ActiveCharacter.StatusEffects.OrderByDescending(i => i.OverrideTurnType).First();
            if (ActiveCharacter.IsDead) await Task.Delay(5000);
 
            else if ( mostPowerfulStatusEffect is not null &&  mostPowerfulStatusEffect.OverrideTurnType > 0 )
            {

                UsageResult overridenUsage  = mostPowerfulStatusEffect.OverridenUsage(ActiveCharacter,ref target!, ref decision, UsageType.NormalUsage);
                if (overridenUsage.Text is not null) _mainText = overridenUsage.Text;
                await CheckForForfeitAsync();
            }
            
            else if (!ActiveCharacter.Team.IsPlayerTeam)
            {
               ActiveCharacter.NonPlayerCharacterAi(ref target!, ref decision);
               await CheckForForfeitAsync();
            }
            else
            {
  
                var results = await _message.WaitForButtonAsync(e =>
                {
                    var didParse = Enum.TryParse(e.Interaction.Data.CustomId, out decision);
                    if (!didParse) return false;
                    if (decision == BattleDecision.Forfeit)
                    {
                        var forfeitedTeam = CharacterTeams.FirstOrDefault(i => i.UserId == e.User.Id);
                        if (forfeitedTeam is not null)
                        {
                            _forfeited = forfeitedTeam;
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
                    _winners = CharacterTeams.First(i => i != ActiveCharacter.Team);
                    break;
                }
                List<DiscordSelectComponentOption> enemiesToSelect = new();
                List<Character> possibleTargets = [];
                
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
                    _message = await _message.ModifyAsync(messageBuilder);
                    using var buttonAwaiterToken = new CancellationTokenSource();
                    _message.WaitForSelectAsync(e =>
                    {
                        if (e.User.Id == ActiveCharacter.Team.UserId)
                        {
                            target = Characters.First(i => i.GetNameWithPosition(i.Team != ActiveCharacter.Team) == e.Values.First().ToString());
                            e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                        }
                        return false;
                    },buttonAwaiterToken.Token);
                
                    var results1 = await  _message.WaitForButtonAsync(e =>
                    {
                        if (e.User.Id == ActiveCharacter.Team.UserId && e.Interaction.Data.CustomId == "Proceed")
                        {
                            buttonAwaiterToken.Cancel();
                            e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                            return true;
                        }
                        return false;
                    });
             
                    if (results1.TimedOut)
                    {
                        timedOut = ActiveCharacter.Team;
                        _winners = CharacterTeams.First(i => i != ActiveCharacter.Team);
                        break;
                    }
                }
            }

            if (_forfeited is not null)
            {
                _winners = CharacterTeams.First(i => i != _forfeited);
                break;
            }
            if (_winners is not null)
            {
            
                await Task.Delay(5000); break;
                
            }
            var move = ActiveCharacter[decision];


            var moveResult =  move?.Utilize(ActiveCharacter,target, UsageType.NormalUsage);
                
            if (moveResult?.Text != null)
            {
                _mainText = moveResult.Text;
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
            foreach (var i in ActiveCharacter.StatusEffects.ToArray())
            {
                if (i.ExecuteStatusEffectAfterTurn)
                {
                    i.PassTurn(ActiveCharacter);
                    if (i.Duration <= 0) ActiveCharacter.StatusEffects.Remove(i);
                }
            }
            CheckAdditionalTexts();

        }

        var losers = CharacterTeams.First(i => i != _winners);
        var rewards = new List<Reward>();

        var coinsToGain = (ulong)(losers
                .Average(i => i.Level) * 500 * losers.Count * 0.75f)
            .Round();
        rewards.Add(new CoinsReward(coinsToGain));
        foreach (var i in losers)
        {
            if (i.IsDead) rewards.AddRange(i.DroppedRewards);
        }
        


        return new BattleResult
        {
            BattleRewards = rewards,
            Turns = Turn,
            Forfeited = _forfeited,
            Winners = _winners,
            TimedOut = timedOut,
            Message = _message
        };
    }
}