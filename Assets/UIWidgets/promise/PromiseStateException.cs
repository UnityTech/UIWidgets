using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSG.Exceptions
{
    /// <summary>
    /// Exception thrown when an operation is performed on a promise that is in an invalid
    /// state for it to handle.
    /// </summary>
    public class PromiseStateException : PromiseException
    {
        public PromiseStateException() { }

        public PromiseStateException(string message) : base(message) { }

        public PromiseStateException(string message, Exception inner) 
            : base(message, inner)
        { }
    }
}