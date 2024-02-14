
namespace Tokens;
using Tokens.TokenTypes;
public class Token
{
    public TokenType TokenType { get; init; }
    public string Lexeme { get; init; }
    public int Line { get; init; }
    public Token(TokenType TT, string lexeme, int line)
    {
        TokenType = TT;
        Lexeme = lexeme;
        Line = line;
    }
}
