using System.Diagnostics.CodeAnalysis;
using Daimler.Providence.Service.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Daimler.Providence.Tests.Utilities
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class JsonUtilsTest
    {
        #region Tests

        [TestMethod]
        public void GetDiffTest_SameJson()
        {
            const string json1 = "{\"test\": 2}";
            const string json2 = "{\"test\": 2}";

            var dif = JsonUtils.GetDiff(json1, json2);
            dif.ShouldBe(string.Empty);   
        }

        [TestMethod]
        public void GetDiffTest_DifferentJson()
        {
            const string json1 = "{\"test\": 1}";
            const string json2 = "{\"test\": 2}";

            var dif = JsonUtils.GetDiff(json1, json2);
            dif.ShouldBe("{\r\n  \"test\": [\r\n    1,\r\n    2\r\n  ]\r\n}");
        }

        #endregion
    }
}
