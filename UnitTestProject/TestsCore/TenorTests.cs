using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.Date;

namespace UnitTestProject.TestsCore
{
    public static class TenorTestsTools
    {
        public static DateTime DateRef = new DateTime(2019, 5, 23, 5, 26, 15);
        public static DateTime DateRef2 = new DateTime(2020, 1, 2, 0, 0, 0);
    }

    [TestClass]
    public class TenorTests
    {
        [TestMethod]
        public void Tenor_GetNumberOfUnit()
        {
            int hours = TenorTestsTools.DateRef.GetNumberOfUnit(TenorUnit.Hour);
            int weeks = TenorTestsTools.DateRef.GetNumberOfUnit(TenorUnit.Week);
            Assert.IsTrue(hours == 5 && weeks == 3);
        }

        [TestMethod]
        public void Tenor_GetRoundDateMonth()
        {
            DateTime newDate = TenorTestsTools.DateRef.GetRoundDate(TenorUnit.Month);
            Assert.IsTrue(newDate == new DateTime(2019,5,1,0,0,0));
        }

        [TestMethod]
        public void Tenor_GetRoundDateWeek()
        {
            DateTime newDate = TenorTestsTools.DateRef.GetRoundDate(TenorUnit.Week);
            Assert.IsTrue(newDate == new DateTime(2019, 5, 20, 0, 0, 0));
        }

        [TestMethod]
        public void Tenor_GetRoundDateDay()
        {
            DateTime newDate = TenorTestsTools.DateRef.GetRoundDate(TenorUnit.Day);
            DateTime newDate2 = TenorTestsTools.DateRef2.GetRoundDate(TenorUnit.Day);
            Assert.IsTrue(newDate == new DateTime(2019, 5, 23, 0, 0, 0)
                && newDate2 == new DateTime(2020, 1, 2, 0, 0, 0));
        }

        [TestMethod]
        public void Tenor_AddTenor()
        {
            Tenor tnr = new Tenor("-3M");
            DateTime testDate = new DateTime(2019, 2, 23, 5, 26, 15);
            DateTime newDate = TenorTestsTools.DateRef.AddTenor(tnr);
            DateTime newDateRd = TenorTestsTools.DateRef.AddTenor(tnr, isRounded: true);
            Assert.IsTrue(newDate == testDate && newDateRd == testDate.GetRoundDate(tnr.Unit));
        }
    }
}
