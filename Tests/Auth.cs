using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Networking;
using Firebase.Auth;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using System.IO;

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
                var querySnapshot = await TestCollection.GetSnapshotAsync();
                //Assert.That(querySnapshot.Documents.Length, Is.Zero);
            }
        }
    }

    public class UnityWebRequestTest : FirestormTestBase
    {
        [UnityTest]
        public IEnumerator UseTokenFromAuthUnitySdkForRest()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureSuperUserAccountCreated();
                await SignInSuperUser();
                var uwr = await (Task<UnityWebRequest>)typeof(FirestormConfig).GetMethod("UWRGet", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(FirestormConfig.Instance, new object[] { "/my-collection" });
                Debug.Log($"{uwr.downloadHandler.text}");
                File.WriteAllText($"{Application.dataPath}/hi.txt", $"{uwr.downloadHandler.text}");
                Assert.That(uwr.isHttpError, Is.Not.True, uwr.error);
            }
        }

        [UnityTest]
        public IEnumerator DiscoveryService()
        {
            yield return T().YieldWait(); async Task T()
            {
                //The discovery service does not require any credentials
                UnityWebRequest uwr = UnityWebRequest.Get($"https://firestore.googleapis.com/$discovery/rest?version=v1beta1");
                var ao = uwr.SendWebRequest();
                await ao.WaitAsync();
                Assert.That(ao.webRequest.error, Is.Null);
            }
        }

        [UnityTest]
        public IEnumerator DiscoveryServiceBadRequest()
        {
            yield return T().YieldWait(); async Task T()
            {
                UnityWebRequest uwr = UnityWebRequest.Get($"https://firestore.googleapis.com/$discovery/rest?version=v1beta1");
                uwr.SetRequestHeader("Authorization",$"Bearer 555"); //<- this makes a bad request
                var ao = uwr.SendWebRequest();
                await ao.WaitAsync();
                Assert.That(ao.webRequest.isHttpError);
                Assert.That(ao.webRequest.isNetworkError, Is.Not.True);
                Assert.That(ao.webRequest.error, Is.Not.Null);
            }
        }
    }
}
