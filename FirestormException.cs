using UnityEngine.Networking;

namespace E7.Firebase
{

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

    [System.Serializable]
    public class FirestormWebRequestException : System.Exception
    {
        private UnityWebRequest uwr;
        public long ErrorCode => uwr.responseCode;

        public FirestormWebRequestException() { }
        public FirestormWebRequestException(UnityWebRequest uwr, string message) : base(message)
        {
            this.uwr = uwr;
        }

        public FirestormWebRequestException(UnityWebRequest uwr, string message, System.Exception inner) : base(message, inner)
        {
            this.uwr = uwr;
        }

        protected FirestormWebRequestException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class FirestormPleaseCreateCompositeIndexException : System.Exception
    {
        public FirestormPleaseCreateCompositeIndexException() { }
        public FirestormPleaseCreateCompositeIndexException(string message) : base(message) { }
        public FirestormPleaseCreateCompositeIndexException(string message, System.Exception inner) : base(message, inner) { }
        protected FirestormPleaseCreateCompositeIndexException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class FirestormDocumentNotFoundException : System.Exception
    {
        public FirestormDocumentNotFoundException() { }
        public FirestormDocumentNotFoundException(string message) : base(message) { }
        public FirestormDocumentNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        protected FirestormDocumentNotFoundException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}