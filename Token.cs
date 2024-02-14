namespace Tokens;
using Tokens.TokenTypes;
internal class Token(TokenType TT, string lexeme, int line)
{
    public TokenType TokenType { get; init; } = TT;
    public string Lexeme { get; init; } = lexeme;
    public int Line { get; init; } = line;
}
