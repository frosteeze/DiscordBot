using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace DiscordConsoleBot.Types
{
    public class DiceResolver
    {
        public enum DiceOperation
        {
            [Description("+")]
            Add,
            [Description("-")]
            Subtract
        }

        public int Count { get; set; } = 0;
        public int Sides { get; set; } = 0;
        public DiceOperation? Operation { get; set; } = null;
        public int Modifier { get; set; } = 0;
        public bool? RemoveLowest { get; set; } = null;

        public List<int> LastRolls { get; private set; }
        public int LastSum { get; private set; }

        public string LastResult
        {
            get
            {
                var resultStr = "";
                foreach (var roll in LastRolls) resultStr += roll.ToString() + ", ";

                resultStr = resultStr.Remove(resultStr.Length - 2);
                resultStr += " : " + LastSum;
                return resultStr;
            }
        }

        private static Random Rng = new Random();

        public DiceResolver()
        {

        }

        public static DiceResolver FromDiceNotation(string diceNotation)
        {
            var regex = new Regex(@"(\d+)[Dd](\d+)(?:([+-])(\d*)([HLhl])?)?");

            if (!regex.IsMatch(diceNotation)) return null;

            var match = regex.Match(diceNotation);

            if (match.Groups.Count < 2) return null;
            if (!int.TryParse(match.Groups[1].Value, out var count) || count < 1) return null;
            if (!int.TryParse(match.Groups[2].Value, out var sides) || sides < 1) return null;

            var operation = (DiceOperation?)null;
            var removeLowest = (bool?)null;

            int.TryParse(match.Groups[4].Value, out int modifier);
            if (!string.IsNullOrEmpty(match.Groups[3].Value))
                operation = match.Groups[3].Value == "+" ? DiceOperation.Add : DiceOperation.Subtract;
            if (!string.IsNullOrEmpty(match.Groups[5].Value))
                removeLowest = match.Groups[5].Value.ToLower() == "l";

            return new DiceResolver()
            {
                Count = count,
                Sides = sides,
                Operation = operation,
                Modifier = modifier,
                RemoveLowest = removeLowest
            };
        }

        public int Roll()
        {
            LastRolls = new List<int>();
            for (var i = 0; i < Count; i++)
            {
                LastRolls.Add(Rng.Next(1, Sides));
            }

            LastRolls.Sort();

            var modifierConsumed = false;
            if (RemoveLowest.HasValue)
            {
                if (Modifier == 0)
                    Modifier = 1;

                for (var i = 0; i < Modifier; i++)
                {
                    if (RemoveLowest.Value)
                        LastRolls.RemoveAt(0);
                    else
                        LastRolls.RemoveAt(LastRolls.Count - 1);
                }
                modifierConsumed = true;
            }
            var sum = LastRolls.Sum();

            if (Operation.HasValue && !modifierConsumed)
            {
                if(Operation.Value == DiceOperation.Add)
                    sum += Modifier;
                if(Operation.Value == DiceOperation.Subtract)
                    sum -= Modifier;
            }

            LastSum = sum;
            return sum;
        }
    }
}
