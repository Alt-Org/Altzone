using NUnit.Framework;

namespace Assets.Tests.EditMode.ConfigTests
{
    /// <summary>
    /// Note that these tests should not change, add or delete existing data which makes these a bit poor tests.
    /// </summary>
    /// <remarks>
    /// If <c>IsFirstTimePlaying</c> is true and <c>IsAccountVerified</c> is false we can do destructive testing!
    /// </remarks>
    [TestFixture]
    public class PlayerDataCacheTest
    {
        [Test]
        public void NonDestructiveTest1()
        {
            Debug.Log($"test");
        }

        [Test]
        public void DestructiveTest2()
        {
            Debug.Log($"test");
        }
    }
}