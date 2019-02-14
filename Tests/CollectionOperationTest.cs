using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using E7.Firebase;

namespace FirestormTest
{
    public class CollectionOperationTest : FirestormTestBase
    {
        [UnityTest]
        public IEnumerator EmptyCollectionGetSnapshot()
        {
            yield return T().YieldWait(); async Task T()
            {
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
                await TestDocument2.SetAsync<TestDataAB>(new TestDataAB { a = 2, b = "y" });
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 1, b = "x" });
                await TestDocument3.SetAsync<TestDataAB>(new TestDataAB { a = 3, b = "z" });

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

        private async Task SetupForQuery()
        {
            //Scrambled write order
            await TestDocument3.SetAsync<TestDataABC>(new TestDataABC { a = 3, b = "z", c = 33.333 });
            await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 1, b = "x", c = 11.111 });
            await TestDocument2.SetAsync<TestDataABC>(new TestDataABC { a = 2, b = "y", c = 22.222 });
            await TestDocument22.SetAsync<TestDataABC>(new TestDataABC { a = 22, b = "yo", c = 222.222 });
            await TestDocument21.SetAsync<TestDataABC>(new TestDataABC { a = 21, b = "hey", c = 111.111 });
        }

        [UnityTest]
        public IEnumerator QueryScope()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("a", "<=", 999));
                Assert.That(snap.Documents.Count(), Is.EqualTo(3), "Do not get documents in the subcollection");
                var snapSub = await TestSubcollection.GetSnapshotAsync(("a", "<=", 999));
                Assert.That(snapSub.Documents.Count(), Is.EqualTo(2));
            }
        }

        // Generating a new document in collection will have a hard to clean up ID, so the test is excluded to prevent hassle (but it works)
        // [UnityTest]
        // public IEnumerator AddAsync()
        // {
        //     yield return T().YieldWait(); async Task T()
        //     {
        //         await EnsureCleanTestCollection();
        //         
        //         await TestCollection.AddAsync(new TestDataABC { a = 3, b = "z", c = 33.333 });
        //         await TestSubcollection.AddAsync(new TestDataABC { a = 2323, b = "z", c = 33.333 });
        //     }
        // }

        [UnityTest]
        public IEnumerator FieldFilterLessThanEqual()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("a", "<=", 2));
                var enu = snap.Documents.GetEnumerator();
                TestDataABC current = null;

                Assert.That(snap.Documents.Count(), Is.EqualTo(2));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(1));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(2));
            }
        }

        [UnityTest]
        public IEnumerator FieldFilterLessThan()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("a", "<", 2));
                var enu = snap.Documents.GetEnumerator();
                TestDataABC current = null;

                Assert.That(snap.Documents.Count(), Is.EqualTo(1));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator FieldFilterGreaterThanEqual()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("a", ">=", 2));
                var enu = snap.Documents.GetEnumerator();
                TestDataABC current = null;

                Assert.That(snap.Documents.Count(), Is.EqualTo(2));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(2));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(3));
            }
        }

        [UnityTest]
        public IEnumerator FieldFilterGreaterThan()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("a", ">", 2));
                var enu = snap.Documents.GetEnumerator();
                TestDataABC current = null;

                Assert.That(snap.Documents.Count(), Is.EqualTo(1));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(3));
            }
        }

        [UnityTest]
        public IEnumerator FieldFilterEqual()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("b", "==", "x"));
                var enu = snap.Documents.GetEnumerator();
                TestDataABC current = null;

                Assert.That(snap.Documents.Count(), Is.EqualTo(1));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(1));
            }
        }

        [UnityTest]
        public IEnumerator FieldFilterEqualNoMatch()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("b", "==", "5argon"));
                Assert.That(snap.Documents.Count(), Is.EqualTo(0));
            }
        }

        private class PlayerWithFriends
        {
            public string playerName;
            public List<object> friends;
        }

        [UnityTest]
        public IEnumerator FieldFilterArrayContains()
        {
            yield return T().YieldWait(); async Task T()
            {
                var t1 = TestDocument1.SetAsync<PlayerWithFriends>(new PlayerWithFriends
                { playerName = "5argon", friends = new object[] { "Suna", "Sompong" }.ToList() });
                var t2 = TestDocument2.SetAsync<PlayerWithFriends>(new PlayerWithFriends
                { playerName = "Suna", friends = new object[] { "5argon", "Shun", "Hydrangea", "Eru" }.ToList() });
                var t3 = TestDocument3.SetAsync<PlayerWithFriends>(new PlayerWithFriends
                { playerName = "Shuno413", friends = new object[] { "Suna", "Reef", "Hydrangea", "5argondesu" }.ToList() });

                await Task.WhenAll(new Task[] { t1, t2, t3 });

                var snap = await TestCollection.GetSnapshotAsync(("friends", "array_contains", "5argon"));

                var enu = snap.Documents.GetEnumerator();

                Assert.That(snap.Documents.Count(), Is.EqualTo(1));

                enu.MoveNext();
                var current = enu.Current.ConvertTo<PlayerWithFriends>();
                Assert.That(current.playerName, Is.EqualTo("Suna"));
            }
        }

        [UnityTest]
        public IEnumerator CompositeFilterMultipleEqualsWorksWithoutCompositeIndex()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();

                var snap = await TestCollection.GetSnapshotAsync(("b", "==", "x"), ("a", "==", 1));
                var enu = snap.Documents.GetEnumerator();
                TestDataABC current = null;

                Assert.That(snap.Documents.Count(), Is.EqualTo(1));

                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(1));

                snap = await TestCollection.GetSnapshotAsync(("b", "==", "x"), ("a", "==", 2), ("c", "==", 43.55));
                Assert.That(snap.Documents.Count(), Is.Zero);
            }
        }

        [UnityTest]
        public IEnumerator CompositeFilter()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();

                var snap = await TestCollection.GetSnapshotAsync(("a", ">=", 1));
                Assert.That(snap.Documents.Count(), Is.EqualTo(3));

                snap = await TestCollection.GetSnapshotAsync(("b", "==", "x"), ("a", ">=", 1));
                var enu = snap.Documents.GetEnumerator();
                Assert.That(snap.Documents.Count(), Is.EqualTo(1), "Since composite filter works, the >= 1 that would have returned 3 documents will be reduced to just 1");
                enu.MoveNext();
                var current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(1));
                Assert.That(current.b, Is.EqualTo("x"));

                snap = await TestCollection.GetSnapshotAsync(("a", ">=", 1), ("b", "==", "x"));
                enu = snap.Documents.GetEnumerator();
                Assert.That(snap.Documents.Count(), Is.EqualTo(1), "Ordering of the composite filter does not matter");
                enu.MoveNext();
                current = enu.Current.ConvertTo<TestDataABC>();
                Assert.That(current.a, Is.EqualTo(1));
                Assert.That(current.b, Is.EqualTo("x"));
            }
        }

        [UnityTest]
        public IEnumerator CompositeFilterNoMatch()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetupForQuery();

                var snap = await TestCollection.GetSnapshotAsync(("b", "==", "x"), ("a", ">", 4));
                var enu = snap.Documents.GetEnumerator();
                Assert.That(snap.Documents.Count(), Is.EqualTo(0));
            }
        }

        [UnityTest]
        public IEnumerator CompositeFilterAskForCreation()
        {
            yield return T().YieldWaitExpectException<FirestormPleaseCreateCompositeIndexException>(); async Task T()
            {
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("c", "<", 55.555), ("a", "==", 2));
                //It should print the error with creation link here
            }
        }
    }
}
