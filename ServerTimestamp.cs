namespace E7.Firebase
{
    /// <summary>
    /// Tries to emulate : https://jskeet.github.io/google-cloud-dotnet/docs/Google.Cloud.Firestore.Data/datamodel.html#server-side-timestamp
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class ServerTimestamp : System.Attribute
    {
    }
}