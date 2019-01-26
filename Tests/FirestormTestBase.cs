using System.Threading.Tasks;
using System.Collections;
using UnityEngine.TestTools;
using Firebase.Auth;
using Firebase;
using System;

namespace FirestormTests
{
    public class FirestormTestBase
    {
        [UnitySetUp]
        public IEnumerator EnsureAllTestAccounts()
        {
            yield return T().YieldWait(); async Task T()
            {
                var config = FirestormConfig.Instance;
                await EnsureUserCreated(config.superUserEmail, config.superUserPassword);
                await EnsureUserCreated(Firestorm.testUserEmail1, Firestorm.testUserPassword1);
                await EnsureUserCreated(Firestorm.testUserEmail2, Firestorm.testUserPassword2);
            }
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
    }
}
