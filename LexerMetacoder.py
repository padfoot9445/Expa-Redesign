import json
#forego constants file for just storing in json and python
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\NamespaceNames.json", "r") as namespace_names:
    namespace_names_dict:dict[str,str] = json.load(namespace_names)
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\ClassNames.json", "r") as class_names:
    class_names_dict: dict[str,str] = json.load(class_names)
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\Keywords.json", "r") as keywords:
    keywords_dict: dict[str,dict[str, str | list[int]]] = json.load(keywords)
    
def value_dict_to_TokenType_accessor(value:dict[str, str | list[int]]):
    return f"{class_names_dict["TokenTypes"]}.{value["name"].upper()}"
LexerFile: str = (
f"""
namespace {namespace_names_dict["Lexer"]};
using {namespace_names_dict["Tokens"]};
using {namespace_names_dict["TokenTypes"]};
using {namespace_names_dict["Exceptions"]};
using System.Text;

internal class {class_names_dict["Lexer"]} : {f'I{class_names_dict["Lexer"]}'}
{{
    private int Current{{ get; set; }} = 0;
    private int Start{{ get; set; }} = 0;
    private int Line{{ get; set; }} = 0;
    private string Code{{ get; init;}}
    private readonly List<IExpaException> NonLethalExceptions = new();
    private delegate Token? ProcessDefaultFunction();
    private static ProcessDefaultFunction[] ProcessDefaultFunctions {{ get; set; }} = Array.Empty<ProcessDefaultFunction>();
    private Dictionary<string, {class_names_dict["TokenTypes"]}> __KeywordsToTokenType = new(){{}};
    private {class_names_dict["TokenTypes"]} KeywordToTokenType(string Keyword) => __KeywordsToTokenType.TryGetValue(Keyword, out {class_names_dict["TokenTypes"]} TT) TT : {value_dict_to_TokenType_accessor(keywords_dict["InterpreterIdentifier"])};
    public {class_names_dict["Lexer"]}(string code)
    {{
        Code = code;
        ProcessDefaultFunctions = [ProcessNum, ProcessString, ProcessIdentifier];
    }}
    #region
    public List<{class_names_dict["Token"]}> Lex()
    {{
        List<{class_names_dict["Token"]}> rv = new();
        //current points to the one we are currently processing(and therefore have not yet processed)
        while(Current < Code.Length)
        {{
            Start = Current;
            switch(Code[Current])
            {{
            {
                "".join(
                f"""
                case '{key.upper()}':
                    rv.Add(new({value_dict_to_TokenType_accessor(value)}, \"{key}\", Line ));
                    Current++;
                    break;
                """ for key, value in keywords_dict.items() if len(key) == 1
                )
                    #TODO: Add default(support for numbers and strings)
            } 
                default:
                    int nullCount = 0;
                    foreach(var i in ProcessDefaultFunctions)
                    {{    
                        Token? token = i();
                        if(token is not null)
                        {{
                            rv.Add(token);
                            break;
                        }}
                        nullCount++;
                    }}
                    if(nullCount == 3)
                    {{
                        NonLethalExceptions.Add(new ExpaParseError(Line, $"Illegal character {{Code[Current]}}"));
                    }}
                    break;

            }}
        }}
    }}
    #endregion
    #region ThrowExceptionIfAtEnd
    private void ThrowExceptionIfAtEnd()
    {{
        if(Current == Code.Length)
        {{
            Throw(new ExpaEOFException(Line));
        }}
    }}
    #endregion
    #region ProcessNum
    private Token? ProcessNum()
    {{
        if(!char.IsNumber(Code[Current]))
        {{
            return null;
        }}
        Current++;
        bool hasDot = false;
        bool lastDot = false;
        while(char.IsNumber(Code[Current]) || Code[Current] == '.' || Code[Current] == ',')
        {{
            if(lastDot)
            {{
                lastDot = false;
            }}
            if(Code[Current] == '.')
            {{
                if(hasDot)
                {{
                    NonLethalExceptions.Add(new ExpaNumberError(Line, "Invalid number: number cannot have two decimal points"));
                }}
                hasDot = true;
                Current++;
            }}
            ThrowExceptionIfAtEnd();
        }}
        if(lastDot)
        {{
            Throw(new ExpaNumberError(Line, "Invalid number: number must not end on a decimal point"));
        }}
        return new({class_names_dict["TokenTypes"]}.{keywords_dict["InterpreterNumber"]["name"].upper()}, Code[Start..Current], Line);
    }}
    #endregion

    #region ProcessString
    private Token? ProcessString()
    {{
        if(Code[Current] != '"')
        {{
            return null;
        }}
        Current++;
        while(Code[Current] != '"')
        {{
            Current++;
        }}
        return new({value_dict_to_TokenType_accessor(keywords_dict["InterpreterString"])}, Code[Start..Current], Line);
    }}
    #endregion

    #region ProcessIdentifier
    private Token? ProcessIdentifier()
    {{
        if(!(Code[Current] == '_' || char.IsLetter(Code[Current])))
        {{
            return null;
        }}
        while(Code[Current] == '_' || char.IsLetterOrDigit(Code[Current]))
        {{
            Current++;
            ThrowExceptionIfAtEnd();
        }}
        
        //check if Identifier
        return new(KeywordToTokenType(Code[Start..Current]), Code[Start..Current], Line);
    }}
    #endregion
    #region Throw
    private void Throw(ExpaSyntaxException exception)
    {{
        StringBuilder ov = new();  
        foreach(ExpaSyntaxException i in NonLethalExceptions)
        {{
            ov.Append(i.Message);
            ov.Append(\"\\n * \\n\");
        }}
    }}
    #endregion
}}
"""
)

TokenFile: str = (
f"""
namespace {namespace_names_dict["Tokens"]};
using {namespace_names_dict["TokenTypes"]};
public class {class_names_dict['Token']}
{{
    public {class_names_dict['TokenTypes']} TokenType {{ get; init; }}
    public string Lexeme {{ get; init; }}
    public int Line {{ get; init; }}
    public {class_names_dict['Token']}({class_names_dict['TokenTypes']} TT, string lexeme, int line)
    {{
        TokenType = TT;
        Lexeme = lexeme;
        Line = line;
    }}
}}
""")

TTFile: str = (
f"""
namespace {namespace_names_dict['TokenTypes']};
enum {class_names_dict['TokenTypes']}
{{
    {
        ",\n\t".join(
            i["name"].upper() 
            for i in keywords_dict.values() 
            if not i["name"].startswith("Interpreter")
        )
    }
}}
"""
)

ILexerFile: str = (
f"""
namespace {namespace_names_dict['Lexer']};
using {namespace_names_dict['Tokens']};
interface ILexer
{{
    public List<Token> Lex();
}}
"""
)
for fp, var in [("Lexer.cs", LexerFile), ("Token.cs", TokenFile), ("TokenTypes.cs", TTFile), ("ILexer.cs", ILexerFile)]:
    with open(fp, "w") as f:
        f.write(var)

print(1)