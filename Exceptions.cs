namespace Exceptions;
abstract class ExpaException : System.Exception
{
    private static string FormatMessage(string message, int line, string name) => $"{name} at line {line}: \\n {message}";
    private protected ExpaException(string message, int line, string name): base(FormatMessage(message, line, name)) { }
}
class ExpaEOFException(int line) : ExpaException("Unexpected end of file or input stream", line, name)
{
    private const string name = "ExpaEOFException";
}

class ExpaFatalException: AggregateException//No need to inherit from ExpaException as you won't ever be passing this around
{
    public ExpaFatalException(ExpaException MainFatalException, IEnumerable<ExpaException> NonFatalExceptions) =>
        throw new AggregateException(MainFatalException.Message, NonFatalExceptions.Prepend(MainFatalException));
    public ExpaFatalException(ExpaException MainFatalException, ExpaException NonFatalException) : this(MainFatalException, [NonFatalException]) { }
}