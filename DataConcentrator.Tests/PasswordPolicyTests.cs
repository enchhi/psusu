using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataConcentrator.Tests
{
    [TestClass]
    public class PasswordPolicyTests
    {
        [TestMethod]
        public void Valid_Password_Passes()
            => Assert.IsTrue(PasswordPolicy.IsValid("Abcdefghijklm1!")); // 15, upper, lower, special

        [TestMethod]
        public void TooShort_Fails()
            => Assert.IsFalse(PasswordPolicy.IsValid("Abc1!"));

        [TestMethod]
        public void NoUpper_Fails()
            => Assert.IsFalse(PasswordPolicy.IsValid("abcdefghijklm1!"));

        [TestMethod]
        public void NoLower_Fails()
            => Assert.IsFalse(PasswordPolicy.IsValid("ABCDEFGHIJKLM1!"));

        [TestMethod]
        public void NoSpecial_Fails()
            => Assert.IsFalse(PasswordPolicy.IsValid("Abcdefghijklm12"));
    }
}
