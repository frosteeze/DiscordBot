using System;
using System.Linq;
using DiscordConsoleBot.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiscordConsoleBot.Test
{
    [TestClass]
    public class DiceResolverTests
    {
        [TestMethod]
        public void BasicRoll()
        {
            var resolver = new DiceResolver
            {
                Count = 4,
                Sides = 6
            };

            resolver.Roll();
            Assert.IsTrue(resolver.LastRolls.Count == 4);
            Assert.IsTrue(resolver.LastRolls.All(x => x >= 1 && x <= 6));
            Console.WriteLine(resolver.LastResult);
        }

        [DataTestMethod]
        [DataRow(DiceResolver.DiceOperation.Subtract, 3)]
        [DataRow(DiceResolver.DiceOperation.Add, 10)]
        public void ModifierTests(DiceResolver.DiceOperation? operation, int modifier)
        {
            var resolver = new DiceResolver
            {
                Count = 4,
                Sides = 6,
                Operation = operation,
                Modifier = modifier
            };

            resolver.Roll();
            var sum = resolver.LastRolls.Sum();

            if(operation == DiceResolver.DiceOperation.Add)
                sum += modifier;
            if (operation == DiceResolver.DiceOperation.Subtract)
                sum -= modifier;

            Assert.AreEqual(sum, resolver.LastSum);
            Console.WriteLine(resolver.LastResult);
        }

        [DataTestMethod]
        [DataRow("4d6", 4, 6, null, 0, null)]
        [DataRow("4d10-h", 4, 10, DiceResolver.DiceOperation.Subtract, 0, false)]
        [DataRow("3d8-2L", 3, 8, DiceResolver.DiceOperation.Subtract, 2, true)]
        [DataRow("10d100+3", 10, 100, DiceResolver.DiceOperation.Add, 3, null)]
        public void DiceNotationTests(string diceNotation, int count, int sides, DiceResolver.DiceOperation? operation, int modifier, bool? removeLowest)
        {
            var resolver = DiceResolver.FromDiceNotation(diceNotation);
            Assert.IsNotNull(resolver);

            Assert.AreEqual(resolver.Count, count);
            Assert.AreEqual(resolver.Sides, sides);
            Assert.AreEqual(resolver.Operation, operation);
            Assert.AreEqual(resolver.Modifier, modifier);
            Assert.AreEqual(resolver.RemoveLowest, removeLowest);

            resolver.Roll();
            Console.WriteLine(resolver.LastResult);
        }
    }
}
