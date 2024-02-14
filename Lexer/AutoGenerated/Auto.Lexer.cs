
namespace Lexer;
using Tokens;
using Tokens.TokenTypes;
using Exceptions;

internal partial class Lexer : ILexer
{
    #region
    public List<Token> Lex()
    {
        List<Token> rv = new();
        //current points to the one we are currently processing(and therefore have not yet processed)
        while(Current < Code.Length)
        {
            Start = Current;
            switch(Code[Current])
            {
            
                case '+':
                    rv.Add(new(TokenType.PLUS, "+", Line ));
                    Current++;
                    break;
                
                case '-':
                    rv.Add(new(TokenType.MINUS, "-", Line ));
                    Current++;
                    break;
                 
                default:
                    int nullCount = 0;
                    foreach(var i in __ProcessDefaultFunctions)
                    {    
                        Token? token = i();
                        if(token is not null)
                        {
                            rv.Add(token);
                            break;
                        }
                        nullCount++;
                    }
                    if(nullCount == 3)
                    {
                        NonLethalExceptions.Add(new ExpaParseError($"Illegal character {Code[Current]}", Line));
                    }
                    break;

            }
        }
    }
    #endregion
}
