using System;
using System.Runtime.Serialization;

namespace Projector
{
    [Serializable]
    internal class VSNotFoundException : Exception
    {
        public VSNotFoundException()
        {
        }

        public VSNotFoundException(string message) : base(message)
        {
        }

        public VSNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VSNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}