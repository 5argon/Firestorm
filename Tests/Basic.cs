using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;

namespace FirestormTests
{
    public static class TaskExtension
    {
        public static async Task WaitAsync(this UnityWebRequestAsyncOperation uwr)
        {
            while(uwr.isDone == false)
            {
                await Task.Yield();
            }
        }

        public static IEnumerator YieldWait(this Task task)
        {
            while (task.IsCompleted == false)
            {
                yield return null;
            }
            if (task.Status == TaskStatus.Faulted)
            {
                throw task.Exception;
            }
        }

        public static IEnumerator YieldWaitExpectException<T>(this Task task) where T : Exception
        {
            while (task.IsCompleted == false)
            {
                yield return null;
            }
            if (task.Status == TaskStatus.Faulted)
            {
                Type failType = task.Exception.InnerException.GetType();
                if (failType == typeof(T))
                {
                }
                else
                {
                    Assert.Fail($"The task fail with exception of type {failType} instead of {typeof(T).Name}");
                }
            }
            else
            {
                Assert.Fail($"The task didn't fail with exception of type {typeof(T).Name}");
            }
        }
    }

    public class Basic
    {
        [Test]
        public void LoadingFirestormConfigFromResources()
        {
            var fi = FirestormConfig.Instance;
        }

        [Test]
        public void ServiceAccountJsonReading()
        {
            var sa = FirestormConfig.Instance.ServiceAccountPrivateKey;
            Assert.That(sa.type, Is.Not.Empty);
            Assert.That(sa.project_id, Is.Not.Empty);
            Assert.That(sa.private_key_id, Is.Not.Empty);
            Assert.That(sa.private_key, Is.Not.Empty);
            Assert.That(sa.client_email, Is.Not.Empty);
            Assert.That(sa.client_id, Is.Not.Empty);
            Assert.That(sa.auth_uri, Is.Not.Empty);
            Assert.That(sa.token_uri, Is.Not.Empty);
            Assert.That(sa.auth_provider_x509_cert_url, Is.Not.Empty);
            Assert.That(sa.client_x509_cert_url, Is.Not.Empty);
        }
        
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
