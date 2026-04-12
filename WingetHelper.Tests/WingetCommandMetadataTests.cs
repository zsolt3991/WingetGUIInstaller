using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WingetHelper.Commands;

namespace WingetHelper.Tests
{
    [TestClass]
    public class WingetCommandMetadataTests
    {
        [TestMethod]
        public void Constructor_AllowsValidArguments()
        {
            var meta = new WingetCommandMetadata<object>("list", "--query", "mypkg");
            Assert.IsNotNull(meta);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_RejectsDangerousArgument()
        {
            // Dangerous characters should be rejected via internal validation
            var meta = new WingetCommandMetadata<object>("list", "> out.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddExtraArguments_RejectsDangerousArgument()
        {
            var meta = new WingetCommandMetadata<object>("list");
            meta.AddExtraArguments("--query", "pkg & calc");
        }
    }
}
