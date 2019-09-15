using NUnit.Framework;
using System;
using Core.TimeSeriesKeys;

namespace Test
{
    [TestFixture]
    public class FrequencyTests
    {
        [Test]
        public void FreqAddTest()
        {
            DateTime date0 = new DateTime(2019, 5, 21);
            DateTime date1 = FrequencyMethods.Add(Frequency.Day1, date0);
            Assert.IsTrue(date1 == new DateTime(2019, 5, 22));
        }
    }
}
