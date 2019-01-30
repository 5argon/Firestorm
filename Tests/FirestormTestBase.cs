using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using Firebase.Auth;
using Firebase;
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using E7.Firestorm;

namespace FirestormTests
{
    public class FirestormTestBase
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
            public int typeNumberInt;
            public bool typeBoolean;
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
                ""integerValue"": ""5678""
              },
              {
                ""booleanValue"": false
              }
            ]
          }
        },
        ""typeNull"": {
          ""nullValue"": null
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
            get{
                yield return TestDocument1;
                yield return TestDocument2;
                yield return TestDocument3;
                yield return TestDocument21;
                yield return TestDocument22;
            }
        }

        /// <summary>
        /// Will sign in and sign out to do the clean up
        /// </summary>
        protected async Task EnsureCleanTestCollection()
        {
            await SignInSuperUser();

            //Debug.Log($"(User still here? {Firestorm.AuthInstance.CurrentUser})");
            await Task.WhenAll(AllTestDocuments.Select(x => x.DeleteAsync()));
            //Debug.Log($"Cleaning done! (User still here? {Firestorm.AuthInstance.CurrentUser})");

            //The collection is automatically removed when you delete all documents.
            var querySs = await TestCollection.GetSnapshotAsync();
            //Debug.Log($"Test!");
            Firestorm.AuthInstance.SignOut();
            //Debug.Log($"Signout done!");
        }

        protected async Task EnsureSuperUserAccountCreated()
        {
            var config = FirestormConfig.Instance;
            await EnsureUserCreated(config.superUserEmail, config.superUserPassword);
        }

        protected async Task SignInSuperUser()
        {
            var config = FirestormConfig.Instance;
            // Firestorm.AuthInstance.IdTokenChanged -= IdTokenChanged;
            // Firestorm.AuthInstance.StateChanged -= LoginChanged;
            // Firestorm.AuthInstance.IdTokenChanged += IdTokenChanged;
            // Firestorm.AuthInstance.StateChanged += LoginChanged;
            FirebaseUser fu = await Firestorm.AuthInstance.SignInWithEmailAndPasswordAsync(config.superUserEmail, config.superUserPassword);
            //Debug.Log($"Signed in to super user {Firestorm.AuthInstance?.CurrentUser.UserId}");
        }

        // public void IdTokenChanged(object sender, EventArgs e)
        // {
        //     Debug.Log($"Token changed!!");
        // }

        // public void LoginChanged(object sender, EventArgs e)
        // {
        //     Debug.Log($"State changed to {Firestorm.AuthInstance.CurrentUser?.UserId} !!");
        // }

        private async Task EnsureUserCreated(string email, string password)
        {
            try
            {
                await Firestorm.AuthInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            }
            catch (AggregateException e)
            {
                if (e.InnerException.GetType() != typeof(FirebaseException) ||
                 e.InnerException.Message.Contains("The email address is already in use by another account.") == false)
                {
                    throw;
                }
            }
        }

        [SetUp]
        public void CreateTestInstance()
        {
            if (Application.isPlaying == false)
            {
                Firestorm.CreateEditModeInstance();
            }
        }

        [SetUp]
        public void SignOutBeforeTest()
        {
            Firestorm.AuthInstance.SignOut();
        }

        [TearDown]
        public void DisposeTestInstance()
        {
            if (Application.isPlaying == false)
            {
                Firestorm.DisposeEditModeInstance();
            }
        }
    }
}
