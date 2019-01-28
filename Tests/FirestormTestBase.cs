using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using Firebase.Auth;
using Firebase;
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace FirestormTests
{
    public class FirestormTestBase
    {
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
            public int typeNumberInt;
            public bool typeBoolean;
            public List<object> typeArray;
            public DateTime typeTimestamp;
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

        protected FirestormCollection TestCollection => Firestorm.Collection(testCollectionName);
        protected FirestormDocument TestDocument1 => TestCollection.Document(testDataName1);
        protected FirestormDocument TestDocument2 => TestCollection.Document(testDataName2);
        protected FirestormDocument TestDocument3 => TestCollection.Document(testDataName3);

        /// <summary>
        /// This subcollection is under document 2
        /// </summary>
        protected FirestormCollection TestSubcollection => TestDocument2.Collection(testSubCollectionName);
        protected FirestormDocument TestDocument21 => TestSubcollection.Document(testDataName21);
        protected FirestormDocument TestDocument22 => TestSubcollection.Document(testDataName22);

        protected IEnumerable<FirestormDocument> AllTestDocuments
        {
            get{
                yield return TestDocument1;
                yield return TestDocument2;
                yield return TestDocument3;
                yield return TestDocument21;
                yield return TestDocument22;
            }
        }

        protected async Task EnsureCleanTestCollection()
        {
            await SignInSuperUser();
            await Task.WhenAll(AllTestDocuments.Select(x => x.DeleteAsync()));
            //The collection is automatically removed when you delete all documents.
            
        }

        protected async Task EnsureSuperUserAccountCreated()
        {
            var config = FirestormConfig.Instance;
            await EnsureUserCreated(config.superUserEmail, config.superUserPassword);
        }

        protected async Task SignInSuperUser()
        {
            var config = FirestormConfig.Instance;
            FirebaseUser fu = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(config.superUserEmail, config.superUserPassword);
        }

        private async Task EnsureUserCreated(string email, string password)
        {
            try
            {
                await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
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
        public void SignOutBeforeTest()
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
    }
}
