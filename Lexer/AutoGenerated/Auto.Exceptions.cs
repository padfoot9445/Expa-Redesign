
namespace Exceptions;
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
