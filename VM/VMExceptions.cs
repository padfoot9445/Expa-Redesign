namespace ExpaIR.Exceptions;

[System.Serializable]
public abstract class ExpaIRException : System.Exception
{

    private protected static string AddCustomMessage(string message) => PrependedMessage + message + MessageSuffix;
    private protected static string PrependedMessage { get; } = String.Empty;
    private protected static string MessageSuffix { get; } = String.Empty;
    public ExpaIRException() { }
    public ExpaIRException(string message) : base(message) { }
    public ExpaIRException(string message, System.Exception inner) : base(message, inner) { }
    protected ExpaIRException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

[System.Serializable]
class ExpaIR_OutOfMemoryException : ExpaIRException
{
    private protected new static string PrependedMessage = "Out of memory:\n";
    public ExpaIR_OutOfMemoryException() { }
    public ExpaIR_OutOfMemoryException(string message) : base(AddCustomMessage(message)) { }
    public ExpaIR_OutOfMemoryException(string message, System.Exception inner) : base(AddCustomMessage(message), inner) { }
    protected ExpaIR_OutOfMemoryException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
class ExpaIR_MallocError: ExpaIR_OutOfMemoryException
{
    private protected new static string PrependedMessage = "Malloc failed\n: ";
}
class MemoryAddressAccessException: ExpaIRException
{
    public MemoryAddressAccessException(string message) : base(AddCustomMessage(message)) { }
}
class ExpaIR_ProgramAccessException: ExpaIRException
{
    private protected new static string PrependedMessage = "Program acces failed:\n";
    public ExpaIR_ProgramAccessException(string message) : base(AddCustomMessage(message)) { }

}