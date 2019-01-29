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
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                var doc = await TestDocument1.GetSnapshotAsync();
                Assert.That(doc.IsEmpty);
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncOverwriteNew()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 31, b = "hi" }, SetOption.Overwrite);
                //Check if the data is there on the server
                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestDataAB>();
                Assert.That(td.a, Is.EqualTo(31));
                Assert.That(td.b, Is.EqualTo("hi"));
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncMergeAllNew()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 31, b = "hi" }, SetOption.MergeAll);
                //Check if the data is there on the server
                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestDataAB>();
                Assert.That(td.a, Is.EqualTo(31));
                Assert.That(td.b, Is.EqualTo("hi"));
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncOverwriteUpdate()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 31, b = "hi" }, SetOption.Overwrite);
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 55, b = "yo" }, SetOption.Overwrite);

                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestDataAB>();
                Assert.That(td.a, Is.EqualTo(55));
                Assert.That(td.b, Is.EqualTo("yo"));
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncOverwriteLess()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 31, b = "hi", c = 55.555 }, SetOption.Overwrite);
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 55, b = "yo" }, SetOption.Overwrite);

                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);

                var ab = snapshot.ConvertTo<TestDataAB>();
                Assert.That(ab.a, Is.EqualTo(55));
                Assert.That(ab.b, Is.EqualTo("yo"));

                var abc = snapshot.ConvertTo<TestDataABC>();
                Assert.That(abc.a, Is.EqualTo(55));
                Assert.That(abc.b, Is.EqualTo("yo"));
                Assert.That(abc.c, Is.EqualTo(default(double)), "With overwrite the double field is removed from the server");
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncOverwriteMore()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 55, b = "yo" }, SetOption.Overwrite);
                await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 31, b = "hi", c = 55.555 }, SetOption.Overwrite);

                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);

                var abc = snapshot.ConvertTo<TestDataABC>();
                Assert.That(abc.a, Is.EqualTo(31));
                Assert.That(abc.b, Is.EqualTo("hi"));
                Assert.That(abc.c, Is.EqualTo(55.555));

                var ab = snapshot.ConvertTo<TestDataAB>();
                Assert.That(ab.a, Is.EqualTo(31), "It is fine to convert to data type with less fields");
                Assert.That(ab.b, Is.EqualTo("hi"), "It is fine to convert to data type with less fields");
            }
        }
    }
}
