using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace FirestormTests
{
    public class OperationTest : FirestormTestBase
    {
        private class TestData
        {
            public int a;
            public string b;
        }

        [UnityTest]
        public IEnumerator CleaningWorks()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                var querySnapshot = await TestCollection.GetSnapshotAsync();
                Assert.That(querySnapshot.Documents.Count(), Is.Zero);
            }
        }

        // [UnityTest]
        // public IEnumerator CleaningWorks20Times()
        // {
        //     yield return T().YieldWait(); async Task T()
        //     {
        //         for (int i = 0; i < 20; i++)
        //         {
        //             await EnsureCleanTestCollection();
        //             Debug.Log($"Passed {i}");
        //         }
        //     }
        // }

        [UnityTest]
        public IEnumerator GetEmptyDocument()
        {
            //Getting empty document is an HTTP 404 error on UnityWebRequest. It would bubble up as an exception.
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                var doc = await TestDocument1.GetSnapshotAsync();
                Assert.That(doc.IsEmpty);
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncNewDocument()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestData>(new TestData { a = 31, b = "hi" }, SetOption.Overwrite);
                //Check if the data is there on the server
                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestData>();
                Assert.That(td.a, Is.EqualTo(31));
                Assert.That(td.b, Is.EqualTo("hi"));
            }
        }
    }
}
