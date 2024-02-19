
namespace Lexer;
using Tokens;
using Tokens.TokenTypes;
using Exceptions;

internal partial class Lexer : ILexer
{
    public static bool __KeywordsToTokenType(string kw, out TokenType? TT)
    {
        switch(kw)
        {
            case "e": TT = TokenType.INT; return true;
            default: TT = null; return false;
        }
    }
    #region
    public List<Token> Lex()
    {
        //current points to the one we are currently processing(and therefore have not yet processed)
        while(Current < Code.Length)
        {
            Start = Current;
            switch(Code[Current])
            {
            
                case '+':
                    LexRV.Add(new(TokenType.PLUS, "+", Line ));
                    Current++;
                    break;
                
                case '-':
                    LexRV.Add(new(TokenType.MINUS, "-", Line ));
                    Current++;
                    break;
                 
                default: ProcessDefaultFunctions(); break;

            }
        }
        return LexRV;
    }
    #endregion
}
