using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEngine.Networking;
using System.Reflection;
using E7.Firebase;
using System.Linq;

namespace FirestormTest
{
    public class UnityWebRequestTest : FirestormTestDataStructure
    {


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
