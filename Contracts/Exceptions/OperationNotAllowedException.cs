namespace Contracts.Exceptions;

public class OperationNotAllowedException : Exception
{
    public OperationNotAllowedException() {}
    public OperationNotAllowedException(string message) : base(message){}
    public OperationNotAllowedException(string message, Exception? innerException) : base(message, innerException) {}
}