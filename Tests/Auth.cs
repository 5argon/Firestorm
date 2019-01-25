using Google.Apis.Auth.OAuth2;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine;
using NUnit.Framework;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2.Responses;
using System;
using Grpc.Core;

namespace FirestormTests
{
    public class Auth
    {
        [UnityTest]
        public IEnumerator RPC()
        {
            yield return T().YieldWait(); async Task T()
            {
                Channel channel = new Channel("firestore.googleapis.com", ChannelCredentials.Insecure);
            }
        }

        [UnityTest]
        public IEnumerator ServiceAccoutAccessTokenRequest()
        {
            yield return T().YieldWait(); async Task T()
            {
                var service = FirestormConfig.Instance.ServiceAccountPrivateKey;
                ServiceAccountCredential.Initializer init = new ServiceAccountCredential.Initializer(
                     id: service.client_email,
                     tokenServerUrl: service.token_uri
                )
                {
                    Scopes = new[]{
                        "https://www.googleapis.com/auth/userinfo.email",
                        "https://www.googleapis.com/auth/cloud-platform",
                        "https://www.googleapis.com/auth/datastore"
                    },
                }.FromPrivateKey(service.private_key);

                ServiceAccountCredential cred = new ServiceAccountCredential(init);

                string accessToken = await cred.GetAccessTokenForRequestAsync(authUri: service.auth_uri);
                Assert.That(accessToken, Is.Not.Null);
            }
        }

        [UnityTest]
        public IEnumerator ServiceAccoutAccessTokenInvalidRequest()
        {
            yield return T().YieldWaitExpectException<TokenResponseException>(); async Task T()
            {
                var service = FirestormConfig.Instance.ServiceAccountPrivateKey;
                ServiceAccountCredential.Initializer init = new ServiceAccountCredential.Initializer(
                    id: "555"
                )
                {
                    Scopes = new[]{
                        "https://www.googleapis.com/auth/userinfo.email",
                        "https://www.googleapis.com/auth/cloud-platform",
                        "https://www.googleapis.com/auth/datastore"
                    },
                }.FromPrivateKey(service.private_key);

                ServiceAccountCredential cred = new ServiceAccountCredential(init);

                string accessToken = await cred.GetAccessTokenForRequestAsync(authUri: service.auth_uri);
                Assert.Fail("It should not come to this point, it should end the task with exception.");
            }
        }

        [UnityTest]
        public IEnumerator ServiceAccoutAccessTokenInvalidRequest2()
        {
            yield return T().YieldWaitExpectException<ArgumentNullException>(); async Task T()
            {
                var service = FirestormConfig.Instance.ServiceAccountPrivateKey;
                ServiceAccountCredential.Initializer init = new ServiceAccountCredential.Initializer(
                     id: service.client_email,
                     tokenServerUrl: service.token_uri
                )
                {
                    Scopes = new[]{
                        "https://www.googleapis.com/auth/userinfo.email",
                        "https://www.googleapis.com/auth/cloud-platform",
                        "https://www.googleapis.com/auth/datastore"
                    },
                };//.FromPrivateKey(service.private_key);

                ServiceAccountCredential cred = new ServiceAccountCredential(init);

                string accessToken = await cred.GetAccessTokenForRequestAsync(authUri: service.auth_uri);
                Assert.That(accessToken, Is.Not.Null);
            }
        }
    }
}
