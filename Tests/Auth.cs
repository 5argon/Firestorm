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
                var config = FirestormConfig.Instance;
                FirebaseUser fu = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(config.superUserEmail, config.superUserPassword);
                var token = await fu.TokenAsync(forceRefresh: false);
                Debug.Log($"Token {token}");

                UnityWebRequest uwr = UnityWebRequest.Get($"{FirestormConfig.Instance.RestDocumentBasePath}/my-collection");
                uwr.SetRequestHeader("Authorization",$"Bearer {token}");
                Debug.Log($"Sending {uwr.uri} {uwr.url}");
                var ao = uwr.SendWebRequest();
                await ao.WaitAsync();
                Debug.Log($"Done! {ao.webRequest.isDone} {ao.webRequest.isHttpError} {ao.webRequest.isNetworkError} {ao.webRequest.error} {ao.webRequest.downloadHandler.text}");
                Assert.That(ao.webRequest.isHttpError, Is.Not.True, ao.webRequest.error);
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
