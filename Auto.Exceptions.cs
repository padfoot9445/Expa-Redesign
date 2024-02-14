
namespace Exceptions;

abstract class ExpaException : System.Exception
{
    private protected ExpaException(string message, int line, string name): base($"{name} at line {line}: \n {message}") { }
}
class ExpaEOFException(int line) : ExpaException("Unexpected end of file or input stream", line, name)
{
    private const string name = "ExpaEOFException";
}
class ExpaSyntaxException : ExpaException 
{
	private const string Name = "ExpaSyntaxException"; 
	public ExpaSyntaxException(string message, int line) : base(message, line, Name){ }
	private protected ExpaSyntaxException(string message, int line, string childName): base(message, line, childName){ }
}
class ExpaParseError : ExpaException 
{
	private const string Name = "ExpaParseError"; 
	public ExpaParseError(string message, int line) : base(message, line, Name){ }
	private protected ExpaParseError(string message, int line, string childName): base(message, line, childName){ }
}
class ExpaLiteralError : ExpaSyntaxException 
{
	private const string Name = "ExpaLiteralError"; 
	public ExpaLiteralError(string message, int line) : base(message, line, Name){ }
	private protected ExpaLiteralError(string message, int line, string childName): base(message, line, childName){ }
}
class ExpaNumberError : ExpaLiteralError 
{
	private const string Name = "ExpaNumberError"; 
	public ExpaNumberError(string message, int line) : base(message, line, Name){ }
	private protected ExpaNumberError(string message, int line, string childName): base(message, line, childName){ }
}
