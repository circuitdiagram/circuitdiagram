using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CircuitDiagram.Controls;
using NUnit.Framework;

namespace CircuitDiagram.Test.Controls
{
    [TestFixture]
    public class UnitsTextBoxTest
    {
        [TestCase("1", ExpectedResult = 1.0)]
        [TestCase("1.2", ExpectedResult = 1.2)]
        [TestCase("1k", ExpectedResult = 1000.0)]
        [TestCase("1m", ExpectedResult = 1e-3)]
        [TestCase("2M", ExpectedResult = 2000000.0)]
        public double TestExpandToDouble(string text)
        {
            return UnitsTextBoxConversions.ExpandToDouble(text);
        }

        [TestCase(1.0, ExpectedResult = "1")]
        [TestCase(2000.0, ExpectedResult = "2k")]
        [TestCase(120000000.0, ExpectedResult = "120M")]
        [TestCase(1e-3, ExpectedResult = "1m")]
        [TestCase(800.0, ExpectedResult = "800")]
        public string TestContractToString(double value)
        {
            return UnitsTextBoxConversions.ContractToString(value);
        }
    }
}
