using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WingetHelper.Utils;

namespace WingetHelper.Tests
{
    [TestClass]
    public class ArgumentValidatorTests
    {
        [TestMethod]
        public void ValidateMany_AllowsSafeArguments()
        {
            var args = new[] { "list", "--query", "package.name" };
            // Should not throw
            ArgumentValidator.ValidateMany(args);
        }

        [TestMethod]
        public void ValidateMany_RejectsDangerousArgument()
        {
            var args = new[] { "install", "> out.txt" };
            Assert.Throws<ArgumentException>(() => ArgumentValidator.ValidateMany(args));
        }

        [TestMethod]
        public void ValidateMany_RejectsNullArgumentArrayElement()
        {
            var args = new string[] { null };
            Assert.Throws<ArgumentNullException>(() => ArgumentValidator.ValidateMany(args));
        }
    }
}
