using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using Firebase.Auth;
using Firebase;
using Firebase.Functions;
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using E7.Firebase;

namespace FirestormTest
{
    public class FirestormTestDataStructure
    {
        protected class TestDataAB
        {
            public int a;
            public string b;
        }

        protected class TestDataAC
        {
            public int a;
            public double c;
        }

        protected class TestDataABC
        {
            public int a;
            public string b;
            public double c;
        }

        protected class TestDataNestedAB
        {
            public int a;
            public string b;
            public double c;
            public TestDataAB nested;
        }
        protected class TestDataNestedAC
        {
            public int a;
            public string b;
            public double c;
            public TestDataAC nested;
        }
        protected class TestDataNestedABC
        {
            public int a;
            public string b;
            public double c;
            public TestDataABC nested;
        }

        const string testCollectionName = "firestorm-test-collection";

        const string testDataName1 = "firestorm-test-data-1";
        const string testDataName2 = "firestorm-test-data-2";
        const string testDataName3 = "firestorm-test-data-3";

        const string testSubCollectionName = "firestorm-test-sub-collection";
        const string testDataName21 = "firestorm-test-data-21";
        const string testDataName22 = "firestorm-test-data-22";

        protected class TestStruct
        {
            public string typeString;
            public double typeNumber;
            public List<object> typeArray; //array is sandwiched at center to increase JSON complexity..
            public DateTime typeTimestamp;
            public TestStructInner typeMap;
            public int typeNumberInt;
            public bool typeBoolean;
        }

        protected class TestStructInner
        {
            public DateTime typeTimestampMap;
            public string typeStringMap;
            public double typeNumberMap;
            public bool typeBooleanMap;
        }

        /// <summary>
        /// Contains all data supported by Firestorm, that is no nested Map or Geopoint.
        /// </summary>
        protected const string sampleDocumentJson = @"
    {
      ""name"": ""projects/firestrike5555/databases/(default)/documents/my-collection/my-doc"",
      ""fields"": {
        ""typeTimestamp"": {
          ""timestampValue"": ""2019-01-27T17:00:00Z""
        },
        ""typeString"": {
          ""stringValue"": ""hey""
        },
        ""typeNumber"": {
          ""doubleValue"": 23.44
        },
        ""typeBoolean"": {
          ""booleanValue"": true
        },
        ""typeMap"": {
          ""mapValue"": {
                ""fields"" : {
                    ""typeTimestampMap"": {
                        ""timestampValue"": ""2016-02-13T19:00:00Z""
                    },
                    ""typeStringMap"": {
                        ""stringValue"": ""omgmapmap""
                    },
                    ""typeNumberMap"": {
                        ""doubleValue"": 98.76
                    },
                    ""typeBooleanMap"": {
                        ""booleanValue"": true
                    }
                }
            }
        },
        ""typeNumberInt"": {
          ""integerValue"": ""1234""
        },
        ""typeArray"": {
          ""arrayValue"": {
            ""values"": [
              {
                ""stringValue"": ""in the array!""
              },
              {
                ""timestampValue"": ""2018-02-16T14:09:04.978706Z"" 
              },
              {
                ""integerValue"": ""5678""
              },
              {
                ""booleanValue"": false
              }
            ]
          }
        }
      },
      ""createTime"": ""2019-01-26T10:18:04.978706Z"",
      ""updateTime"": ""2019-01-28T12:45:25.496931Z""
    }
        ";

        protected FirestormCollectionReference TestCollection => Firestorm.Collection(testCollectionName);
        protected FirestormDocumentReference TestDocument1 => TestCollection.Document(testDataName1);
        protected FirestormDocumentReference TestDocument2 => TestCollection.Document(testDataName2);
        protected FirestormDocumentReference TestDocument3 => TestCollection.Document(testDataName3);

        /// <summary>
        /// This subcollection is under document 2
        /// </summary>
        protected FirestormCollectionReference TestSubcollection => TestDocument2.Collection(testSubCollectionName);
        protected FirestormDocumentReference TestDocument21 => TestSubcollection.Document(testDataName21);
        protected FirestormDocumentReference TestDocument22 => TestSubcollection.Document(testDataName22);

        protected IEnumerable<FirestormDocumentReference> AllTestDocuments
        {
            get
            {
                yield return TestDocument1;
                yield return TestDocument2;
                yield return TestDocument3;
                yield return TestDocument21;
                yield return TestDocument22;
            }
        }
    }

    public class FirestormTestBase : FirestormTestDataStructure
    {

        /// <summary>
        /// Clean up using cloud function. Also delete and create the super user.
        /// </summary>
        private async Task SetUpFirestoreForTest(bool isTearDown)
        {
            var ff = FirebaseFunctions.GetInstance(Firestorm.AuthInstance.App);
            var testCleanUp = ff.GetHttpsCallable("firestormTestCleanUp");
            var callResult = await testCleanUp.CallAsync(new Dictionary<string, object>
            {
                ["isTearDown"] = isTearDown,
                ["testSecret"] = FirestormConfig.Instance.testSecret,
                ["superUserId"] = FirestormConfig.Instance.superUserEmail,
                ["superUserPassword"] = FirestormConfig.Instance.superUserPassword,
            });
        }

        private async Task SignInSuperUser()
        {
            var config = FirestormConfig.Instance;
            FirebaseUser fu = await Firestorm.AuthInstance.SignInWithEmailAndPasswordAsync(config.superUserEmail, config.superUserPassword);
            //Debug.Log($"Signed in to super user {Firestorm.AuthInstance?.CurrentUser.UserId}");
        }

        [UnitySetUp]
        public IEnumerator SetUpFirestore()
        {
            if (Application.isPlaying == false)
            {
                Firestorm.CreateEditModeInstance();
            }
            Firestorm.AuthInstance.SignOut();
            yield return T().YieldWait(); async Task T()
            {
                await SetUpFirestoreForTest(isTearDown: false);
                await SignInSuperUser();
            }
        }

        [UnityTearDown]
        public IEnumerator TearDownFirestore()
        {
            yield return T().YieldWait(); async Task T()
            {
                await SetUpFirestoreForTest(isTearDown: true);
            }
            //if you dispose before above, it hard crash unity lol
            if (Application.isPlaying == false)
            {
                Firestorm.DisposeEditModeInstance();
            }
        }
    }
}
