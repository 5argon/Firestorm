using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Networking;

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
    }


