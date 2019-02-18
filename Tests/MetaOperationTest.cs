using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using E7.Firebase;
using UnityEngine.Networking;
using System.Reflection;

namespace FirestormTesto
{
    public class MetaOperationTest : FirestormTestBase
    {
        [UnityTest]
        public IEnumerator CleanTest()
        {
            yield return T().YieldWait(); async Task T()
            {
                var querySs = await TestCollection.GetSnapshotAsync();
                Assert.That(querySs.Documents.Count(), Is.Zero);
                var querySs2 = await TestSubcollection.GetSnapshotAsync();
                Assert.That(querySs2.Documents.Count(), Is.Zero);
            }
        }

        [UnityTest]
        public IEnumerator UseTokenFromAuthUnitySdkForRest()
        {
            yield return T().YieldWait(); async Task T()
            {
                var uwr = await (Task<UnityWebRequest>)typeof(FirestormConfig).GetMethod("UWRGet", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(FirestormConfig.Instance, new object[] { "/nonexistent-collection" });
                Assert.That(uwr.isHttpError, Is.Not.True, uwr.error);
            }
        }
    }
}
