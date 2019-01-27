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
    }
}
