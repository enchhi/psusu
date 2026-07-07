using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class PasswordHasherTests
    {
        [TestMethod]
        public void SameInput_SameHash()
            => Assert.AreEqual(PasswordHasher.Hash("Tajna123!"), PasswordHasher.Hash("Tajna123!"));

        [TestMethod]
        public void DifferentInput_DifferentHash()
            => Assert.AreNotEqual(PasswordHasher.Hash("Tajna123!"), PasswordHasher.Hash("Druga123!"));

        [TestMethod]
        public void Hash_IsHex64()
            => Assert.AreEqual(64, PasswordHasher.Hash("abc").Length); // SHA-256 = 32 bajta = 64 hex
    }
}
