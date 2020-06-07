using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Date;
using System.Collections.Generic;

namespace UnitTestProject.TestsCore
{
    [TestClass]
    public class TimeSeriesKeysUnitTests
    {
        [TestMethod]
        public void Freq_Comparison()
        {
            bool test = Frequency.Day1.IsInferiorFrequency(Frequency.Day15);
            test = test && Frequency.Min1.IsInferiorFrequency(Frequency.Min1);
            test = test && !Frequency.Day1.IsInferiorFrequency(Frequency.Min15);
            Assert.IsTrue(test);
        }

        [TestMethod]
        public void Freq_Adjust_Hour4()
        {
            DateTime date0 = new DateTime(2019, 5, 21, 19, 50, 20);
            DateTime date1 = FrequencyMethods.Adjust(Frequency.Hour4, date0);
            DateTime date2 = FrequencyMethods.Adjust(Frequency.Hour4, date0, true);
            Assert.IsTrue(date1 == new DateTime(2019, 5, 21, 16, 0, 0) &&
                          date2 == new DateTime(2019, 5, 21, 20, 0, 0));
        }

        [TestMethod]
        public void Freq_Adjust_MoreThan1Day()
        {
            DateTime date0 = new DateTime(2019, 5, 21, 19, 50, 20);
            DateTime dateRef = new DateTime(2019, 5, 21, 0, 0, 0);
            DateTime dateD = FrequencyMethods.Adjust(Frequency.Day1, date0);
            DateTime dateW = FrequencyMethods.Adjust(Frequency.Week1, date0);
            DateTime date15D = FrequencyMethods.Adjust(Frequency.Day15, date0);
            Assert.IsTrue(dateD == dateRef &&
                          dateW == dateRef &&
                          date15D == dateRef);
        }

        [TestMethod]
        public void Freq_Add()
        {
            DateTime date0 = new DateTime(2019, 5, 21);
            DateTime date1 = FrequencyMethods.Add(Frequency.Day1, date0);
            Assert.IsTrue(date1 == new DateTime(2019, 5, 22));
        }

        [TestMethod]
        public void Freq_GetSchedule()
        {
            DateTime startDate = new DateTime(2020, 2, 8);
            DateTime endDate = new DateTime(2020, 3, 1);
            List<DateTime> schedule = Frequency.Hour4.GetSchedule(startDate, endDate);
            Assert.IsTrue(schedule[10] == new DateTime(2020, 2, 9, 16, 0, 0));
        }

        [TestMethod]
        public void Freq_GetScheduleWithDepth()
        {
            DateTime startDate = new DateTime(2020, 2, 8);
            Frequency freq0 = Frequency.Hour1;
            int n = 32;
            DateTime endDate = freq0.Add(startDate, n);
            List<DateTime> schedule1 = freq0.GetSchedule(startDate, endDate);
            List<DateTime> schedule2 = freq0.GetSchedule(endDate, n);
            Assert.IsTrue(TestTools<DateTime>.ListComparison(schedule1, schedule2));
        }
    }
}
