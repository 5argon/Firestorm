using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.Networking;
using Firebase.Auth;

namespace FirestormTests
{
    public class UnityWebRequestTest : FirestormTestBase
    {
        [UnityTest]
        public IEnumerator UseTokenFromAuthUnitySdkForRest()
        {
            yield return T().YieldWait(); async Task T()
            {
                await EnsureSuperUserAccountCreated();
                await SignInSuperUser();
                var uwr = await FirestormConfig.Instance.UWRGet("/my-collection");
                Debug.Log($"{uwr.downloadHandler.text}");
                Assert.That(uwr.isHttpError, Is.Not.True, uwr.error);
            }
        }

        [UnityTest]
        public IEnumerator DiscoveryService()
        {
            yield return T().YieldWait(); async Task T()
            {
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
