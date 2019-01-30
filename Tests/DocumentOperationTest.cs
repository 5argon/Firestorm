using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace FirestormTests
{

    public class DocumentOperationTest : FirestormTestBase
    {


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
        public IEnumerator SetGetDocumentMultiple()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();

                //See what TestDocument1 is in the FirestormTestBase
                var t1 = TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 1, b = "x" }, SetOption.Overwrite);
                var t2 = TestDocument2.SetAsync<TestDataAB>(new TestDataAB { a = 2, b = "y" }, SetOption.Overwrite);
                var t3 = TestDocument3.SetAsync<TestDataAB>(new TestDataAB { a = 3, b = "z" }, SetOption.Overwrite);
                await Task.WhenAll(new Task[]{ t1, t2, t3 });

                var i1 = (await TestDocument1.GetSnapshotAsync()).ConvertTo<TestDataAB>();
                var i2 = (await TestDocument2.GetSnapshotAsync()).ConvertTo<TestDataAB>();
                var i3 = (await TestDocument3.GetSnapshotAsync()).ConvertTo<TestDataAB>();

                Assert.That(i1.a, Is.EqualTo(1));
                Assert.That(i1.b, Is.EqualTo("x"));

                Assert.That(i2.a, Is.EqualTo(2));
                Assert.That(i2.b, Is.EqualTo("y"));

                Assert.That(i3.a, Is.EqualTo(3));
                Assert.That(i3.b, Is.EqualTo("z"));
            }
        }

        [UnityTest]
        public IEnumerator SetGetDocumentNested()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();

                var t1 = TestDocument2.SetAsync<TestDataAB>(new TestDataAB { a = 1, b = "x" }, SetOption.Overwrite);
                var t2 = TestDocument21.SetAsync<TestDataAB>(new TestDataAB { a = 2, b = "y" }, SetOption.Overwrite);
                var t3 = TestDocument22.SetAsync<TestDataAB>(new TestDataAB { a = 3, b = "z" }, SetOption.Overwrite);
                await Task.WhenAll(new Task[] { t1, t2, t3 });
                var i1 = (await TestDocument2.GetSnapshotAsync()).ConvertTo<TestDataAB>();
                var i2 = (await TestDocument21.GetSnapshotAsync()).ConvertTo<TestDataAB>();
                var i3 = (await TestDocument22.GetSnapshotAsync()).ConvertTo<TestDataAB>();

                Assert.That(i1.a, Is.EqualTo(1));
                Assert.That(i1.b, Is.EqualTo("x"));

                Assert.That(i2.a, Is.EqualTo(2));
                Assert.That(i2.b, Is.EqualTo("y"));

                Assert.That(i3.a, Is.EqualTo(3));
                Assert.That(i3.b, Is.EqualTo("z"));
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
        public IEnumerator SetAsyncMergeAllUpdate()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 31, b = "hi", c = 55.555 }, SetOption.MergeAll);
                await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 66, b = "yo", c = 66.666 }, SetOption.MergeAll);

                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestDataABC>();
                Assert.That(td.a, Is.EqualTo(66));
                Assert.That(td.b, Is.EqualTo("yo"));
                Assert.That(td.c, Is.EqualTo(66.666));
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncMergeAllLess()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 31, b = "hi", c = 55.555 }, SetOption.MergeAll);
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 66, b = "yo" }, SetOption.MergeAll);

                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestDataABC>();
                Assert.That(td.a, Is.EqualTo(66));
                Assert.That(td.b, Is.EqualTo("yo"));
                Assert.That(td.c, Is.EqualTo(55.555), "Unlike Overwrite mode the non-intersecting field remain untouched");
            }
        }

        [UnityTest]
        public IEnumerator SetAsyncMergeAllMore()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureCleanTestCollection();
                await SignInSuperUser();
                await TestDocument1.SetAsync<TestDataAB>(new TestDataAB { a = 31, b = "hi" }, SetOption.MergeAll);
                await TestDocument1.SetAsync<TestDataABC>(new TestDataABC { a = 66, b = "yo", c = 55.555 }, SetOption.MergeAll);

                var snapshot = await TestDocument1.GetSnapshotAsync();
                Assert.That(snapshot.IsEmpty, Is.Not.True);
                var td = snapshot.ConvertTo<TestDataABC>();
                Assert.That(td.a, Is.EqualTo(66));
                Assert.That(td.b, Is.EqualTo("yo"));
                Assert.That(td.c, Is.EqualTo(55.555));
            }
        }

        [UnityTest]
        public IEnumerator AllSupportedTypesSurvivalTest()
        {
            yield return T().YieldWait(); async Task T()
            {
                //Test if all value type can go and come back from the server correctly
                await EnsureCleanTestCollection();
                await SignInSuperUser();

                var ts = new TestStruct();
                ts.typeTimestamp = DateTime.MinValue;
                ts.typeString = "CYCLONEMAGNUM";
                ts.typeNumber = 55.55;
                ts.typeNumberInt = 555;
                ts.typeBoolean = false;
                ts.typeArray = new List<object>();
                ts.typeArray.Add("5argonTheGod");
                ts.typeArray.Add(6789);
                ts.typeArray.Add(DateTime.MaxValue);
                ts.typeArray.Add(true);
                ts.typeArray.Add(11.111);

                await TestDocument1.SetAsync<TestStruct>(ts, SetOption.Overwrite);
                Debug.Log($"WRITTEN");
                var getBack = (await TestDocument1.GetSnapshotAsync()).ConvertTo<TestStruct>();

                Assert.That(getBack.typeTimestamp.Year, Is.EqualTo(DateTime.MinValue.Year));
                Assert.That(getBack.typeTimestamp.Month, Is.EqualTo(DateTime.MinValue.Month));
                Assert.That(getBack.typeTimestamp.Day, Is.EqualTo(DateTime.MinValue.Day));
                Assert.That(getBack.typeTimestamp.TimeOfDay.TotalHours, Is.EqualTo(DateTime.MinValue.TimeOfDay.TotalHours));

                Assert.That(getBack.typeString, Is.EqualTo("CYCLONEMAGNUM"));
                Assert.That(getBack.typeNumber, Is.EqualTo(55.55));
                Assert.That(getBack.typeNumberInt, Is.EqualTo(555));
                Assert.That(getBack.typeBoolean, Is.EqualTo(false));
                Assert.That((string)getBack.typeArray[0], Is.EqualTo("5argonTheGod"));
                Assert.That(getBack.typeArray[1], Is.EqualTo(6789));
                var timeInArray = (DateTime)getBack.typeArray[2];

                Assert.That(timeInArray.Year, Is.EqualTo(DateTime.MaxValue.Year));
                Assert.That(timeInArray.Month, Is.EqualTo(DateTime.MaxValue.Month));
                Assert.That(timeInArray.Day, Is.EqualTo(DateTime.MaxValue.Day));
                Assert.That(timeInArray.TimeOfDay.TotalHours, Is.EqualTo(DateTime.MaxValue.TimeOfDay.TotalHours).Within(0.00001), 
                "Allowed Google to round off some decimals places in the returned JSON");

                Assert.That((bool)getBack.typeArray[3], Is.EqualTo(true));
                Assert.That((double)getBack.typeArray[4], Is.EqualTo(11.111));
            }
        }

    }
}
