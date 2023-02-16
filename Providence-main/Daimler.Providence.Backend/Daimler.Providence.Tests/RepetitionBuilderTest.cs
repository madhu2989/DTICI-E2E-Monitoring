using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Daimler.Providence.Service.Models;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Daimler.Providence.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RepetitionBuilderTest
    {
        private RepetitionBuilder _repetitionBuilder;

        [TestInitialize]
        public void TestInitialization()
        {
            _repetitionBuilder = new RepetitionBuilder();
        }

        #region General

        [TestMethod]
        public void RepeatEveryDay_InvalidRepeatType_InvalidEnumArgumentException()
        {
            // Assign
            var startDate      = new DateTime(2019, 1, 1);
            var endDate        = new DateTime(2019, 1, 31);
            var repeatType     = (RepeatType)3;
            var repeatInterval = 1;

            // Act
            var error = false;
            try
            {
                _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);
            }
            catch (ProvidenceException)
            {
                error = true;
            }
            catch (Exception)
            {
                error = false;
            }

            // Assert
            error.ShouldBe(true);
        }

        #endregion

        #region Daily recurring

        [TestMethod]
        public void RepeatEveryDay_BeginOfMonth_ListGenerated()
        {
            // Assign
            var startDate      = new DateTime(2019, 1, 1);
            var endDate        = new DateTime(2019, 1, 31);
            var repeatType     = RepeatType.Daily;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(31);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 1));
            recurDates[14].ShouldBe(new DateTime(2019, 1, 15));
            recurDates.Last().ShouldBe(new DateTime(2019, 1, 31));
        }

        [TestMethod]
        public void RepeatEveryNthDay_RepeatIntervalGreater1_ListGenerated()
        {
            // Assign
            var startDate      = new DateTime(2019, 1, 1);
            var endDate        = new DateTime(2019, 1, 31);
            var repeatType     = RepeatType.Daily;
            var repeatInterval = 3;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(11);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 1));
            recurDates[1].ShouldBe(new DateTime(2019, 1, 4));
            recurDates[2].ShouldBe(new DateTime(2019, 1, 7));
            recurDates[4].ShouldBe(new DateTime(2019, 1, 13));
            recurDates[7].ShouldBe(new DateTime(2019, 1, 22));
            recurDates.Last().ShouldBe(new DateTime(2019, 1, 31));
        }

        #endregion

        #region Weekly recurring

        [TestMethod]
        public void RepeatEveryWeek_BeginOfTheMonth_ListGenerated()
        {
            // Assign
            var startDate      = new DateTime(2019, 1, 1);
            var endDate        = new DateTime(2019, 2, 28);
            var repeatType     = RepeatType.Weekly;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(9);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 1));
            recurDates[1].ShouldBe(new DateTime(2019, 1, 8));
            recurDates[2].ShouldBe(new DateTime(2019, 1, 15));
            recurDates[3].ShouldBe(new DateTime(2019, 1, 22));
            recurDates[4].ShouldBe(new DateTime(2019, 1, 29));
            recurDates[5].ShouldBe(new DateTime(2019, 2, 5));
            recurDates[6].ShouldBe(new DateTime(2019, 2, 12));
            recurDates[7].ShouldBe(new DateTime(2019, 2, 19));
            recurDates.Last().ShouldBe(new DateTime(2019, 2, 26));
        }

        [TestMethod]
        public void RepeatEveryNthWeek_RepeatIntervalGreater1_Generated()
        {
            // Assign
            var startDate      = new DateTime(2019, 1, 3);
            var endDate        = new DateTime(2019, 2, 28);
            var repeatType     = RepeatType.Weekly;
            var repeatInterval = 2;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(5);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 3));
            recurDates[1].ShouldBe(new DateTime(2019, 1, 17));
            recurDates[2].ShouldBe(new DateTime(2019, 1, 31));
            recurDates[3].ShouldBe(new DateTime(2019, 2, 14));
            recurDates.Last().ShouldBe(new DateTime(2019, 2, 28));
        }

        #endregion

        #region Monthly recurring

        [TestMethod]
        public void RepeatEveryMonth_BeginOfMonth_ListOfAllMonths()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 5);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(12);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 5));
            recurDates[1].ShouldBe(new DateTime(2019, 2, 5));
            recurDates[4].ShouldBe(new DateTime(2019, 5, 5));
            recurDates[7].ShouldBe(new DateTime(2019, 8, 5));
            recurDates.Last().ShouldBe(new DateTime(2019, 12, 5));
        }

        [TestMethod]
        public void RepeatEveryNthMonth_RepeatIntervalGreater1_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 10);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 3;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(4);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 10));
            recurDates[1].ShouldBe(new DateTime(2019, 4, 10));
            recurDates[2].ShouldBe(new DateTime(2019, 7, 10));
            recurDates.Last().ShouldBe(new DateTime(2019, 10, 10));
        }

        [TestMethod]
        public void RepeatEveryMonth_StartDayIs31_ListWithLastDaysOfTheMonth()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 31);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, null);

            // Assert
            recurDates.Count.ShouldBe(12);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 31));
            recurDates[1].ShouldBe(new DateTime(2019, 2, 28));
            recurDates[2].ShouldBe(new DateTime(2019, 3, 31));
            recurDates[3].ShouldBe(new DateTime(2019, 4, 30));
            recurDates[4].ShouldBe(new DateTime(2019, 5, 31));
            recurDates[5].ShouldBe(new DateTime(2019, 6, 30));
            recurDates[6].ShouldBe(new DateTime(2019, 7, 31));
            recurDates[7].ShouldBe(new DateTime(2019, 8, 31));
            recurDates[8].ShouldBe(new DateTime(2019, 9, 30));
            recurDates[9].ShouldBe(new DateTime(2019, 10, 31));
            recurDates[10].ShouldBe(new DateTime(2019, 11, 30));
            recurDates.Last().ShouldBe(new DateTime(2019, 12, 31));
        }

        [TestMethod]
        public void RepeatEveryMonth_KeepWeekdayBeginOfMonth_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 7);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, true);

            // Assert
            recurDates.Count.ShouldBe(12);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 7));
            recurDates[1].ShouldBe(new DateTime(2019, 2, 4));
            recurDates[2].ShouldBe(new DateTime(2019, 3, 4));
            recurDates[3].ShouldBe(new DateTime(2019, 4, 1));
            recurDates[4].ShouldBe(new DateTime(2019, 5, 6));
            recurDates[5].ShouldBe(new DateTime(2019, 6, 3));
            recurDates[6].ShouldBe(new DateTime(2019, 7, 1));
            recurDates[7].ShouldBe(new DateTime(2019, 8, 5));
            recurDates[8].ShouldBe(new DateTime(2019, 9, 2));
            recurDates[9].ShouldBe(new DateTime(2019, 10, 7));
            recurDates[10].ShouldBe(new DateTime(2019, 11, 4));
            recurDates.Last().ShouldBe(new DateTime(2019, 12, 2));
        }

        [TestMethod]
        public void RepeatEveryNthMonth_KeepWeekdayBeginOfMonth_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 4);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 2;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, true);

            // Assert
            recurDates.Count.ShouldBe(6);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 4));
            recurDates[1].ShouldBe(new DateTime(2019, 3, 1));
            recurDates[2].ShouldBe(new DateTime(2019, 5, 3));
            recurDates[3].ShouldBe(new DateTime(2019, 7, 5));
            recurDates[4].ShouldBe(new DateTime(2019, 9, 6));
            recurDates.Last().ShouldBe(new DateTime(2019, 11, 1));
        }

        [TestMethod]
        public void RepeatEveryMonth_KeepWeekdayMiddleOfMonth_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 15);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, true);

            // Assert
            recurDates.Count.ShouldBe(12);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 15));
            recurDates[1].ShouldBe(new DateTime(2019, 2, 19));
            recurDates[2].ShouldBe(new DateTime(2019, 3, 19));
            recurDates[3].ShouldBe(new DateTime(2019, 4, 16));
            recurDates[4].ShouldBe(new DateTime(2019, 5, 21));
            recurDates[5].ShouldBe(new DateTime(2019, 6, 18));
            recurDates[6].ShouldBe(new DateTime(2019, 7, 16));
            recurDates[7].ShouldBe(new DateTime(2019, 8, 20));
            recurDates[8].ShouldBe(new DateTime(2019, 9, 17));
            recurDates[9].ShouldBe(new DateTime(2019, 10, 15));
            recurDates[10].ShouldBe(new DateTime(2019, 11, 19));
            recurDates.Last().ShouldBe(new DateTime(2019, 12, 17));
        }

        [TestMethod]
        public void RepeatEveryNthMonth_KeepWeekdayMiddleOfMonth_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 20);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 3;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, true);

            // Assert
            recurDates.Count.ShouldBe(4);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 20));
            recurDates[1].ShouldBe(new DateTime(2019, 4, 21));
            recurDates[2].ShouldBe(new DateTime(2019, 7, 21));
            recurDates.Last().ShouldBe(new DateTime(2019, 10, 20));
        }

        [TestMethod]
        public void RepeatEveryMonth_KeepWeekdayEndOfMonth_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 31);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 1;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, true);

            // Assert
            recurDates.Count.ShouldBe(12);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 31));
            recurDates[1].ShouldBe(new DateTime(2019, 2, 28));
            recurDates[2].ShouldBe(new DateTime(2019, 3, 28));
            recurDates[3].ShouldBe(new DateTime(2019, 4, 25));
            recurDates[4].ShouldBe(new DateTime(2019, 5, 30));
            recurDates[5].ShouldBe(new DateTime(2019, 6, 27));
            recurDates[6].ShouldBe(new DateTime(2019, 7, 25));
            recurDates[7].ShouldBe(new DateTime(2019, 8, 29));
            recurDates[8].ShouldBe(new DateTime(2019, 9, 26));
            recurDates[9].ShouldBe(new DateTime(2019, 10, 31));
            recurDates[10].ShouldBe(new DateTime(2019, 11, 28));
            recurDates.Last().ShouldBe(new DateTime(2019, 12, 26));
        }

        [TestMethod]
        public void RepeatEveryNthMonth_KeepWeekdayEndOfMonth_ListGenerated()
        {
            // Assign
            var startDate = new DateTime(2019, 1, 30);
            var endDate = new DateTime(2019, 12, 31);
            var repeatType = RepeatType.Monthly;
            var repeatInterval = 4;

            // Act
            var recurDates = _repetitionBuilder.GetRepetitionDates(startDate, endDate, repeatType, repeatInterval, true);

            // Assert
            recurDates.Count.ShouldBe(3);
            recurDates.First().ShouldBe(new DateTime(2019, 1, 30));
            recurDates[1].ShouldBe(new DateTime(2019, 5, 29));
            recurDates.Last().ShouldBe(new DateTime(2019, 9, 25));
        }

        #endregion
    }
}
