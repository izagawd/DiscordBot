using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.StatusEffects;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public partial class Character
{
    class StatusEffectBattleText : AdditionalBattleText
    {
        private StatusEffect _statusEffect;
        private List<Character> _affectedCharacters;

        public override string Text
        {
            get
            {
                var noun = "has";

                if (_affectedCharacters.Count > 1)
                    noun = "have";
                var concatenated = BasicFunction.CommaConcatenator(_affectedCharacters
                    .Select(i => i.NameWithAlphabetIdentifier));
                var whatsDone = "added to";
                if (_isAnOptimization)
                    whatsDone = "optimized on";
                return $"{_statusEffect.Name} {noun} been {whatsDone} {concatenated}";
            }
        }

        private bool _isAnOptimization = false;
        public StatusEffectBattleText(Character character, StatusEffect statusEffect, bool isAnOptimization = false)
        {
            _affectedCharacters = [character];
            _statusEffect = statusEffect;
            _isAnOptimization = isAnOptimization;
        }
        protected StatusEffectBattleText(){}
        public override AdditionalBattleText? Merge(AdditionalBattleText additionalBattleTextInstance)
        {
            if (additionalBattleTextInstance is not StatusEffectBattleText addStatusEffectBattleText) return null;
            if (addStatusEffectBattleText._statusEffect.GetType() != _statusEffect.GetType()) return null;
            if (_isAnOptimization != addStatusEffectBattleText._isAnOptimization) return null;
            if (_affectedCharacters.Any(i => addStatusEffectBattleText._affectedCharacters.Contains(i))) return null;
            return new StatusEffectBattleText()
            {
                _isAnOptimization = _isAnOptimization,
                _statusEffect =  _statusEffect,
                _affectedCharacters = [.._affectedCharacters, ..addStatusEffectBattleText._affectedCharacters]
            };

        }
    }
    class CombatReadinessChangeBattleText : AdditionalBattleText
    {


        private int _combatReadinessChangeAmount;
        private List<Character> _affectedCharacters;
        public CombatReadinessChangeBattleText(Character character,int increaseAmount)
        {
            _affectedCharacters = [character];
            _combatReadinessChangeAmount = increaseAmount;
        }

        public override string Text
        {
            get
            {
                var noun = "has";

                if (_affectedCharacters.Count > 1)
                    noun = "have";
                string thingDone = "decreased";

                if (_combatReadinessChangeAmount >= 0)
                {
                    thingDone = "increased";
                }

                return BasicFunction.CommaConcatenator(_affectedCharacters
                           .Select(i => i.NameWithAlphabetIdentifier))
                       + $" {noun} their combat readiness {thingDone} by {Math.Abs(_combatReadinessChangeAmount)}%!";
            }
        }

        protected CombatReadinessChangeBattleText(){}
        public override AdditionalBattleText? Merge(AdditionalBattleText additionalBattleTextInstance)
        {
            if (additionalBattleTextInstance is not CombatReadinessChangeBattleText combatReadinessChangeBattleText)
                return null;
            if (combatReadinessChangeBattleText._combatReadinessChangeAmount != _combatReadinessChangeAmount)
                return null;
            if (_affectedCharacters.Any(i => combatReadinessChangeBattleText._affectedCharacters.Contains(i))) return null;
            return new CombatReadinessChangeBattleText()
            {
                _combatReadinessChangeAmount = _combatReadinessChangeAmount,
                _affectedCharacters = [.._affectedCharacters, ..combatReadinessChangeBattleText._affectedCharacters]
            };
        }
    }
    class  ExtraTurnBattleText : AdditionalBattleText
    {
        private List<Character> _extraTurners;
        public override string Text {             get
        {
            var noun = "has";

            if (_extraTurners.Count > 1)
                noun = "have";
            return BasicFunction.CommaConcatenator(_extraTurners
                    .Select(i => i.NameWithAlphabetIdentifier)) + $" {noun} been granted an extra turn!";
        }
        
        }

        public ExtraTurnBattleText(Character extraTurnCharacter)
        {
            _extraTurners = [extraTurnCharacter];
        }

        public ExtraTurnBattleText()
        {
            
        }
        public override AdditionalBattleText? Merge(AdditionalBattleText additionalBattleTextInstance)
        {
            if (additionalBattleTextInstance is not ExtraTurnBattleText extraTurnBattleText) return null;
            return new ExtraTurnBattleText()
            {
                _extraTurners = [.._extraTurners, ..extraTurnBattleText._extraTurners]
            };
        }
    }
    class  ReviveBattleText : AdditionalBattleText
    {
        private List<Character> _revivedCharacters;

        public override string Text
        {
            get
            {
                var noun = "has";

                if (_revivedCharacters.Count > 1)
                    noun = "have";
                return BasicFunction.CommaConcatenator(_revivedCharacters
                        .Select(i => i.NameWithAlphabetIdentifier)) + $" {noun} been revived!";
            }
        }

        public ReviveBattleText(Character revivedCharacter)
        {
            _revivedCharacters = [revivedCharacter];
            
        }
        protected ReviveBattleText(){}
        public override AdditionalBattleText? Merge(AdditionalBattleText additionalBattleTextInstance)
        {
            if (additionalBattleTextInstance is not ReviveBattleText reviveBattleText) return null;
            return new ReviveBattleText()
            {
                _revivedCharacters = [.._revivedCharacters, ..reviveBattleText._revivedCharacters]
            };
        }
    }
    class  DeathBattleText : AdditionalBattleText
    {
        public override string Text
        {
            get
            {
                var noun = "has";

                if (_deadCharacters.Count > 1)
                    noun = "have";
                return BasicFunction.CommaConcatenator(_deadCharacters
                    .Select(i => i.NameWithAlphabetIdentifier)) + $" {noun} died!";
            }
        }


        private List<Character> _deadCharacters;
        public DeathBattleText(Character deadCharacter)
        {
            _deadCharacters = [deadCharacter];
        }
        protected DeathBattleText(){}
        public override AdditionalBattleText? Merge(AdditionalBattleText additionalBattleTextInstance)
        {
            if (additionalBattleTextInstance is not DeathBattleText deathHolder) return null;
            return new DeathBattleText()
            {
                _deadCharacters = [.._deadCharacters, ..deathHolder._deadCharacters]
            };

        }
    }
}