using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;

namespace FirestormTesto
{
    public static class TaskExtensionTest
    {
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
}
