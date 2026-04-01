using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Core.Application.Exceptions;

public class InvalidResponseException : Exception
{
    public InvalidResponseException() : base() { }
    public InvalidResponseException(string? message) : base(message) { }
    public InvalidResponseException(string? message, Exception? innerException) : base(message, innerException) { }
}
