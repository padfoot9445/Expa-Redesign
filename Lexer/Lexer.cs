namespace Lexer;
using Tokens;
using Exceptions;
using Tokens.TokenTypes;
using System.Text;
internal partial class Lexer: ILexer
{
    private int Current{ get; set; } = 0;
    private int Start{ get; set; } = 0;
    private int Line{ get; set; } = 0;
    private string Code{ get; init;}
    private readonly List<ExpaException> NonLethalExceptions = new();
    private delegate Token? ProcessDefaultFunction();
    private static ProcessDefaultFunction[] __ProcessDefaultFunctions { get; set; } = Array.Empty<ProcessDefaultFunction>();
    private TokenType KeywordToTokenType(string Keyword) => __KeywordsToTokenType(Keyword, out TokenType? TT)? (TokenType)TT! : TokenType.IDENTIFIER;//we know TT must not be null if KeywordsToTokenType returns true
    private List<Token> LexRV = [];
    public Lexer(string code)
    {
        Code = code;
        __ProcessDefaultFunctions = [ProcessNum, ProcessString, ProcessIdentifier];
    }
    private void ProcessDefaultFunctions()
    {
        foreach (ProcessDefaultFunction function in __ProcessDefaultFunctions)
        {
            Token? token = function();
            if (token is not null)
            {
                LexRV.Add(token);
                return;//We only need to process one default type per function call; if there are multiple back-to-back the function will be called more than once
            }
        }
        NonLethalExceptions.Add(new ExpaParseError($"Illegal character {Code[Current]}", Line));
    }
    
    #region ThrowExceptionIfAtEnd
    private void ThrowExceptionIfAtEnd()
    {
        if(Current == Code.Length)
        {
            Throw(new ExpaEOFException(Line));
        }
    }
    #endregion
    #region ProcessNum
    private Token? ProcessNum()
    {
        if(!char.IsNumber(Code[Current]))
        {
            return null;
        }
        Current++;
        bool hasDot = false;
        bool lastDot = false;
        while(char.IsNumber(Code[Current]) || Code[Current] == '.' || Code[Current] == ',')
        {
            if(lastDot)
            {
                lastDot = false;
            }
            if(Code[Current] == '.')
            {
                if(hasDot)
                {
                    NonLethalExceptions.Add(new ExpaNumberError("Invalid number: number cannot have two decimal points", Line));
                }
                hasDot = true;
                Current++;
            }
            ThrowExceptionIfAtEnd();
        }
        if(lastDot)
        {
            Throw(new ExpaNumberError("Invalid number: number must not end on a decimal point", Line));
        }
        return new(TokenType.NUMBER, Code[Start..Current], Line);
    }
    #endregion

    #region ProcessString
    private Token? ProcessString()
    {
        if(Code[Current] != '"')
        {
            return null;
        }
        Current++;
        while(Code[Current] != '"')
        {
            Current++;
        }
        return new(TokenType.STRING, Code[Start..Current], Line);
    }
    #endregion

    #region ProcessIdentifier
    private Token? ProcessIdentifier()
    {
        if(!(Code[Current] == '_' || char.IsLetter(Code[Current])))
        {
            return null;
        }
        while(Code[Current] == '_' || char.IsLetterOrDigit(Code[Current]))
        {
            Current++;
            ThrowExceptionIfAtEnd();
        }
        
        //check if Identifier
        return new(KeywordToTokenType(Code[Start..Current]), Code[Start..Current], Line);
    }
    #endregion
    #region Throw
    private void Throw(ExpaException exception)
    {
        throw new ExpaFatalException(exception, NonLethalExceptions);
    }
    #endregion
}