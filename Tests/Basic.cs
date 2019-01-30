using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using E7.Firestorm;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace FirestormTests
{

    public class Basic
    {
        [Test]
        public void LoadingFirestormConfigFromResources()
        {
            var fi = FirestormConfig.Instance;
        }

        // [Test]
        // public void ServiceAccountJsonReading()
        // {
        //     var sa = FirestormConfig.Instance.ServiceAccountPrivateKey;
        //     Assert.That(sa.type, Is.Not.Empty);
        //     Assert.That(sa.project_id, Is.Not.Empty);
        //     Assert.That(sa.private_key_id, Is.Not.Empty);
        //     Assert.That(sa.private_key, Is.Not.Empty);
        //     Assert.That(sa.client_email, Is.Not.Empty);
        //     Assert.That(sa.client_id, Is.Not.Empty);
        //     Assert.That(sa.auth_uri, Is.Not.Empty);
        //     Assert.That(sa.token_uri, Is.Not.Empty);
        //     Assert.That(sa.auth_provider_x509_cert_url, Is.Not.Empty);
        //     Assert.That(sa.client_x509_cert_url, Is.Not.Empty);
        // }
        
        [UnityTest]
        public IEnumerator TestAwait()
        {
            float frameTime = 1/60f;
            var rememberTime = Time.realtimeSinceStartup;
            yield return T().YieldWait(); async Task T()
            {
                await Task.Delay((int)(frameTime * 5 * 1000));
            }
            Assert.That(Time.realtimeSinceStartup - rememberTime, Is.GreaterThan(frameTime * 5), "Waited 5 frames, it wait for real");

            rememberTime = Time.realtimeSinceStartup;
            yield return T2().YieldWait(); async Task T2()
            {
                await Task.Delay(1);
            }
            Assert.That(Time.realtimeSinceStartup - rememberTime, Is.GreaterThan(frameTime), "Waited just 1ms, it should results in a 1 frame wait on the yield");
            Assert.That(Time.realtimeSinceStartup - rememberTime, Is.LessThan(frameTime * 5), "Waited just 1ms, it should results in a 1 frame wait on the yield");
        }
    }
}
