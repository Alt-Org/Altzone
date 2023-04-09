using System.Collections;
using NUnit.Framework;
using Prg.Scripts.Common.PubSub;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Prg.Tests
{
    [TestFixture]
    public class UnityHubTest
    {
        [UnityTest]
        public IEnumerator UnityHubTestWithEnumeratorPasses()
        {
            yield return null;
            var hub = new UnityHub();
            Assert.IsNotNull(hub);
        }
    }
}