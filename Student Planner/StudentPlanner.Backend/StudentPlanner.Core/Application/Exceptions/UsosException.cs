namespace StudentPlanner.Core.Application.Exceptions;

public class UsosException : Exception
{
    public UsosException() : base() { }
    public UsosException(string message) : base(message) { }
    public UsosException(string? message, Exception? innerException) : base(message, innerException) { }
}