using System;

namespace Projector
{
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
    }
}