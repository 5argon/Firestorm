using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;

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

        private async Task SetupForQuery()
        {
            //Scrambled write order
            await TestDocument3.SetAsync<TestDataABC>(new TestDataABC { a = 3, b = "z", c = 33.333 }, SetOption.Overwrite);
            await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 1, b = "x", c = 11.111 }, SetOption.Overwrite);
            await TestDocument2.SetAsync<TestDataABC>(new TestDataABC { a = 2, b = "y", c = 22.222 }, SetOption.Overwrite);
            await TestDocument22.SetAsync<TestDataABC>(new TestDataABC { a = 22, b = "yo", c = 222.222 }, SetOption.Overwrite);
            await TestDocument21.SetAsync<TestDataABC>(new TestDataABC { a = 21, b = "hey", c = 111.111 }, SetOption.Overwrite);
        }

        [UnityTest]
        public IEnumerator QueryScope()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await SetupForQuery();
                var snap = await TestCollection.GetSnapshotAsync(("a", "<=", 999));
                Assert.That(snap.Documents.Count(), Is.EqualTo(3), "Do not get documents in the subcollection");
                var snapSub = await TestSubcollection.GetSnapshotAsync(("a", "<=", 999));
                Assert.That(snapSub.Documents.Count(), Is.EqualTo(2));
            }
        }

        [UnityTest]
        public IEnumerator FieldFilterLessThanEqual()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
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
                await EnsureCleanTestCollection();
                await SignInSuperUser();
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
                await EnsureCleanTestCollection();
                await SignInSuperUser();
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
                await EnsureCleanTestCollection();
                await SignInSuperUser();
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
                await EnsureCleanTestCollection();
                await SignInSuperUser();
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
                await EnsureCleanTestCollection();
                await SignInSuperUser();

                var t1 = TestDocument1.SetAsync<PlayerWithFriends>(new PlayerWithFriends
                { playerName = "5argon", friends = new object[] { "Suna", "Sompong" }.ToList() }, SetOption.Overwrite);
                var t2 = TestDocument2.SetAsync<PlayerWithFriends>(new PlayerWithFriends
                { playerName = "Suna", friends = new object[] { "5argon", "Shun", "Hydrangea", "Eru" }.ToList() }, SetOption.Overwrite);
                var t3 = TestDocument3.SetAsync<PlayerWithFriends>(new PlayerWithFriends
                { playerName = "Shuno413", friends = new object[] { "Suna", "Reef", "Hydrangea", "5argondesu" }.ToList() }, SetOption.Overwrite);

                await Task.WhenAll(new Task[] { t1, t2, t3 });

                var snap = await TestCollection.GetSnapshotAsync(("friends", "array_contains", "5argon"));

                var enu = snap.Documents.GetEnumerator();

                Assert.That(snap.Documents.Count(), Is.EqualTo(1));

                enu.MoveNext();
                var current = enu.Current.ConvertTo<PlayerWithFriends>();
                Assert.That(current.playerName, Is.EqualTo("Suna"));
            }
        }

    }
}
