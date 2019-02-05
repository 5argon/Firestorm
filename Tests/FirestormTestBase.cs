using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using Firebase.Auth;
using Firebase;
using Firebase.Functions;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using E7.Firebase;

namespace FirestormTest
{
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
