[System.Serializable]
public class FirestormException : System.Exception
{
    public FirestormException() { }
    public FirestormException(string message) : base(message) { }
    public FirestormException(string message, System.Exception inner) : base(message, inner) { }
    protected FirestormException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
