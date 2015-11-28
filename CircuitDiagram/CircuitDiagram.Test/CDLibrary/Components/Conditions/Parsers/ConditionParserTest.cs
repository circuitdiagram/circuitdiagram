using System;
using System.Collections.Generic;
using CircuitDiagram.Components.Conditions;
using CircuitDiagram.Components.Conditions.Parsers;
using CircuitDiagram.Components;
using NUnit.Framework;

namespace CircuitDiagram.Test.CDLibrary.Components.Conditions.Parsers
{
    [TestFixture]
    public class ConditionParserTest
    {
        [Test]
        public void TestConditionParserValidConditions()
        {
            var horizontal = new ConditionTreeLeaf(ConditionType.State, "horizontal", ConditionComparison.Equal, new PropertyUnion(true));
            var propertyEq1 = new ConditionTreeLeaf(ConditionType.Property, "Property", ConditionComparison.Equal, new PropertyUnion(1));

            var conditionTests = new Dictionary<string, IConditionTreeItem>()
            {
                { "horizontal", horizontal},
                { "$Property==1", propertyEq1},
                { "horizontal|$Property==1", new ConditionTree(ConditionTree.ConditionOperator.OR, horizontal, propertyEq1)},
                { "horizontal,$Property==1", new ConditionTree(ConditionTree.ConditionOperator.AND, horizontal, propertyEq1)}
            };

            var parseContext = new ParseContext();
            parseContext.PropertyTypes.Add(new KeyValuePair<string,PropertyUnionType>("Property", PropertyUnionType.Double));

            ConditionParser parser = new ConditionParser(new ConditionFormat() { StatesUnderscored = false });
            foreach(var test in conditionTests)
            {
                var parsed = parser.Parse(test.Key, parseContext);
                Assert.AreEqual(test.Value, parsed);
            }
        }

        [Test]
        public void TestConditionParserInvalidConditions()
        {
            var conditionTests = new string[]
            {
                "$123",                     // Property names cannot start with a number
                "$_123",                    // Property names cannot start with an underscore
                "$1.23",                    // Property names cannot contain special characters
                "horizontal||$Prop[gt]1000",// OR is '|', not '||'
                "$Prop==1||$Prop==2"        // OR is '|', not '||'    
            };

            var parseContext = new ParseContext();
            ConditionParser parser = new ConditionParser(new ConditionFormat() { StatesUnderscored = false });
            foreach (var test in conditionTests)
            {
                try
                {
                    parser.Parse(test, parseContext);
                    Assert.Fail("Parsing {0} succeeded", test);
                }
                catch
                { 
                }
            }
        }

        [Test]
        public void TestSplitLeaf()
        {
            var leafTests = new Dictionary<string, Tuple<bool, bool, string, string, string>>()
            {
                { "horizontal", Tuple.Create(false, true, "horizontal", String.Empty, String.Empty)},
                { "!horizontal", Tuple.Create(true, true, "horizontal", String.Empty, String.Empty)},
                { "$Prop", Tuple.Create(false, false, "Prop", String.Empty, String.Empty)},
                { "!$Prop", Tuple.Create(true, false, "Prop", String.Empty, String.Empty)},
                { "$Prop==1", Tuple.Create(false, false, "Prop", "==", "1")},
                { "$Prop[lt]1.0", Tuple.Create(false, false, "Prop", "[lt]", "1.0")},
            };

            foreach (var test in leafTests)
            {
                bool isNegated;
                bool isState;
                string property;
                string op;
                string compareTo;
                ConditionParser.SplitLeaf(new ConditionToken(test.Key, 0), out isNegated, out isState, out property, out op, out compareTo);

                Assert.AreEqual(test.Value.Item1, isNegated);
                Assert.AreEqual(test.Value.Item2, isState);
                Assert.AreEqual(test.Value.Item3, property);
                Assert.AreEqual(test.Value.Item4, op);
                Assert.AreEqual(test.Value.Item5, compareTo);
            }
        }
    }
}
