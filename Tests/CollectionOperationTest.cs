using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;

namespace FirestormTests
{
    public class CollectionOperationTest : FirestormTestBase
    {
        [UnityTest]
        public IEnumerator EmptyCollectionGetSnapshot()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                var querySnapshot = await TestCollection.GetSnapshotAsync();
                Assert.That(querySnapshot.Documents.Count(), Is.Zero);
                
                var querySnapshot2 = await TestSubcollection.GetSnapshotAsync();
                Assert.That(querySnapshot2.Documents.Count(), Is.Zero);
            }
        }

        [UnityTest]
        public IEnumerator DocumentReturnedInOrderAndCorrectData()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();

                await TestDocument2.SetAsync<TestDataAB>(new TestDataAB { a = 2, b = "y" }, SetOption.Overwrite);
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 1, b = "x" }, SetOption.Overwrite);
                await TestDocument3.SetAsync<TestDataAB>(new TestDataAB { a = 3, b = "z" }, SetOption.Overwrite);

                //await Task.WhenAll(new Task<TestDataAB>[] { t1, t2, t3 });
                var collection = await TestCollection.GetSnapshotAsync();

                var enumerator = collection.Documents.GetEnumerator();

                //It will be ordered by [ascending document ID](https://firebase.google.com/docs/firestore/query-data/order-limit-data)
                //Even if we write 2->1->3 we will get 1->2->3
                //Custom ordering not supported

                enumerator.MoveNext();
                var doc = enumerator.Current.ConvertTo<TestDataAB>();

                Assert.That(doc.a, Is.EqualTo(1));
                Assert.That(doc.b, Is.EqualTo("x"));

                enumerator.MoveNext();
                doc = enumerator.Current.ConvertTo<TestDataAB>();

                Assert.That(doc.a, Is.EqualTo(2));
                Assert.That(doc.b, Is.EqualTo("y"));

                enumerator.MoveNext();
                doc = enumerator.Current.ConvertTo<TestDataAB>();

                Assert.That(doc.a, Is.EqualTo(3));
                Assert.That(doc.b, Is.EqualTo("z"));
            }
        }

        [UnityTest]
        public IEnumerator ReturnsAllDocumentNotIncludingSubCollectionDocument()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                var t1 = TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 1, b = "x" }, SetOption.Overwrite);
                var t2 = TestDocument2.SetAsync<TestDataAB>(new TestDataAB { a = 2, b = "y" }, SetOption.Overwrite);
                var t3 = TestDocument3.SetAsync<TestDataAB>(new TestDataAB { a = 3, b = "z" }, SetOption.Overwrite);
                var t21 = TestDocument21.SetAsync<TestDataAB>(new TestDataAB { a = 4, b = "ok" }, SetOption.Overwrite);
                var t22 = TestDocument22.SetAsync<TestDataAB>(new TestDataAB { a = 5, b = "okk" }, SetOption.Overwrite);

                await Task.WhenAll(new Task[] { t1, t2, t3, t21, t22 });

                var collection = await TestCollection.GetSnapshotAsync();
                var subCollection = await TestSubcollection.GetSnapshotAsync();
                Assert.That(collection.Documents.Count(), Is.EqualTo(3));
                Assert.That(subCollection.Documents.Count(), Is.EqualTo(2));
            }
        }
    }
}
