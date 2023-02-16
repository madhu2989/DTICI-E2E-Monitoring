using System;
using System.Diagnostics.CodeAnalysis;
using Daimler.Providence.Database;
using Daimler.Providence.Service.Models.AlertIgnoreRule;
using Daimler.Providence.Service.Models.Deployment;
using Daimler.Providence.Service.Models.InternalJob;

namespace Daimler.Providence.Tests
{
    [ExcludeFromCodeCoverage]
    public static class TestParameters
    {
        public static string User = "User_Test";
        public static string Comment = "Comment_Test";
        public static string Name = "Name_Test";
        public static string Description = "Description_Test";
        public static string ShortDescription = "ShortDescription_Test";
        public static string IncreasedAlertDescription = "[State Increased] Description_Test";
        public static string CloseReason = "CloseReason_Test";
        public static string EmailAddress = "EmailAddresses@Test.com";
        public static string EmailAddress2 = "EmailAddresses2@Test.com";
        public static string ElementId = "ElementId_Test";
        public static string OrphanElementId = "ElementId_Orphan_Test";
        public static string ComponentType = "ComponentType_Test";

        public static string ServiceElementId = "Service_ElementId_Test_1";
        public static string ServiceElementId2 = "Service_ElementId_Test_2";
        public static string ServiceElementId3 = "Service_ElementId_Test_3";
        public static string ServiceElementId4 = "Service_ElementId_Test_4";
        public static string ActionElementId = "Action_ElementId_Test_1";
        public static string ActionElementId2 = "Action_ElementId_Test_2";
        public static string ActionElementId3 = "Action_ElementId_Test_3";
        public static string ActionElementId4 = "Action_ElementId_Test_4";
        public static string ActionElementId5 = "Action_ElementId_Test_5";
        public static string ComponentElementId = "Component_ElementId_Test_1";
        public static string ComponentElementId2 = "Component_ElementId_Test_2";
        public static string ComponentElementId3 = "Component_ElementId_Test_3";
        public static string ComponentElementId4 = "Component_ElementId_Test_4";
        public static string CheckElementId = "Check_ElementId_Test_1";
        public static string CheckElementId2 = "Check_ElementId_Test_2";
        public static string CheckElementId3 = "Check_ElementId_Test_3";
        public static string CheckElementId4 = "Check_ElementId_Test_4";
        public static string CheckElementId5 = "Check_ElementId_Test_5";
        public static string AlertName = "AlertName_Test";

        public static string EnvironmentSubscriptionId = "EnvironmentSubscriptionId_Test_1";
        public static string EnvironmentSubscriptionId2 = "EnvironmentSubscriptionId_Test_2";
        public static string EnvironmentSubscriptionId3 = "EnvironmentSubscriptionId_Test_3";
        public static string EnvironmentName = "EnvironmentName_Test";
        public static string EnvironmentName2 = "EnvironmentName_Test_2";
        public static string EnvironmentName3 = "EnvironmentName_Test_3";
        public static string ConfigurationKey = "ConfigurationKey_Test";
        public static string ConfigurationValue = "ConfigurationValue_Test";
        public static string CheckId = "CheckId_Test";
        public static string CheckId2 = "CheckId_Test_2";
        public static string HeartbeatCheckId = "HeartbeatAlert";
        public static int TriggerTime = 30;

        public static int NotificationDelay = 10;
        public static int Frequency = 60;
        public static int ValidId = 1;
        public static int ValidId2 = 2;
        public static int ValidId3 = 3;
        public static int ValidId4 = 4;
        public static int ValidId5 = 5;
        public static int ValidId6 = 6;
        public static int ValidId7 = 7;
        public static int InvalidId = 0;

        public static string UserName = "Username";
        public static string FileName = "Filename";
        public static JobType JobType = JobType.Sla;
        public static JobState JobState = JobState.Running;

        public static Guid ValidRecordId = Guid.NewGuid();
        public static string InvalidRecordId = "";

        public static DateTime PastTime = DateTime.UtcNow.AddDays(-1);
        public static DateTime CurrentTime = DateTime.UtcNow;
        public static DateTime FutureTime = DateTime.UtcNow.AddDays(1);

        public static State DbOkState = new State { Id = 1, Name = "OK" };
        public static State DbWarningState = new State { Id = 2, Name = "WARNING" };
        public static State DbErrorState = new State { Id = 3, Name = "ERROR" };

        public static ComponentType DbEnvironmentType = new ComponentType { Id = 5, Name = "Environment" };
        public static ComponentType DbServiceType = new ComponentType { Id = 6, Name = "Service" };
        public static ComponentType DbActionType = new ComponentType { Id = 4, Name = "Action" };
        public static ComponentType DbComponentType = new ComponentType { Id = 7, Name = "Component" };
        public static ComponentType DbCheckType = new ComponentType { Id = 3, Name = "Check" };

        public static RepeatInformation RepeatInformation = new RepeatInformation
        {
            RepeatType = RepeatType.Daily,
            RepeatInterval = 2,
            RepeatUntil = DateTime.UtcNow.AddDays(7),
            RepeatOnSameWeekDayCount = null
        };

        public static AlertIgnoreCondition AlertIgnoreCondition = new AlertIgnoreCondition { CheckId = "" };

        public static DateTime EnvironmentCreationDate = new DateTime(2018, 1, 1);
        public static DateTime ServiceCreationDate = new DateTime(2019, 7, 1);
        public static DateTime ActionCreationDate = new DateTime(2019, 8, 11);
        public static DateTime ComponentCreationDate = new DateTime(2019, 12, 24);
       
        #region Public Methods

        public static bool CompareTimestamps(DateTime t1, DateTime t2)
        {
            return t1.Year.Equals(t2.Year) &&
                   t1.Month.Equals(t2.Month) &&
                   t1.Day.Equals(t2.Day) &&
                   t1.Hour.Equals(t2.Hour) &&
                   t1.Minute.Equals(t2.Minute) &&
                   t1.Second.Equals(t2.Second);
        }

        #endregion
    }
}
