using System.Diagnostics.CodeAnalysis;
using Daimler.Providence.Service.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Daimler.Providence.Tests.Utilities
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ProvidenceHelperTest
    {
        #region Tests

        [TestMethod]
        public void CompareStringTest_SameString()
        {
            var string1 = "This is a test string";
            var string2 = "This is a test string";

            var isSame = ProvidenceHelper.CompareString(string1, string2);
            isSame.ShouldBe(true);

            // Both strings are the same but one is lower and one is upper case
            string1 = "This is a test string";
            string2 = "this is a test string";

            isSame = ProvidenceHelper.CompareString(string1, string2);
            isSame.ShouldBe(true);

            // Both strings are empty
            string1 = "";
            string2 = "";

            isSame = ProvidenceHelper.CompareString(string1, string2);
            isSame.ShouldBe(true);

            // Both strings are null
            string1 = null;
            string2 = null;

            isSame = ProvidenceHelper.CompareString(string1, string2);
            isSame.ShouldBe(true);

            // One string is null and one is empty
            string1 = null;
            string2 = "";

            isSame = ProvidenceHelper.CompareString(string1, string2);
            isSame.ShouldBe(true);
        }

        [TestMethod]
        public void CompareStringTest_DifferentString()
        {
            var string1 = "This is a test string";
            var string2 = "this is a test Strung";

            var isSame = ProvidenceHelper.CompareString(string1, string2);
            isSame.ShouldBe(false);
        }

        #endregion
    }
}
