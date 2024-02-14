import json
#forego constants file for just storing in json and python
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\NamespaceNames.json", "r") as namespace_names:
    namespace_names_dict:dict[str,str] = json.load(namespace_names)
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\ClassNames.json", "r") as class_names:
    class_names_dict: dict[str,str] = json.load(class_names)
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\Keywords.json", "r") as keywords:
    keywords_dict: dict[str,dict[str, str | list[int]]] = json.load(keywords)
with open(r"D:\coding\c#\Expa V4\Expa-Redesign\Exceptions.json", "r") as exceptions:
    exceptions_dict: dict[str, list[str]] = json.load(exceptions)
    
def value_dict_to_TokenType_accessor(value:dict[str, str | list[int]]):
    return f"{class_names_dict["TokenTypes"]}.{value["name"].upper()}"
LexerFile: str = (
f"""
namespace {namespace_names_dict["Lexer"]};
using {namespace_names_dict["Tokens"]};
using {namespace_names_dict["TokenTypes"]};
using {namespace_names_dict["Exceptions"]};

internal partial class {class_names_dict["Lexer"]} : {f'I{class_names_dict["Lexer"]}'}
{{
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
                    foreach(var i in __ProcessDefaultFunctions)
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
                        NonLethalExceptions.Add(new ExpaParseError($"Illegal character {{Code[Current]}}", Line));
                    }}
                    break;

            }}
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

ExceptionsFile: str = (
f"""
namespace Exceptions;

abstract class ExpaException : System.Exception
{{
    private protected ExpaException(string message, int line, string name): base($"{{name}} at line {{line}}: \\n {{message}}") {{ }}
}}
class ExpaEOFException(int line) : ExpaException("Unexpected end of file or input stream", line, name)
{{
    private const string name = "ExpaEOFException";
}}
{
    "\n".join(
        f"class {k} : {', '.join(e for e in v )} \n{{\n\tprivate const string Name = \"{k}\"; \n\tpublic {k}(string message, int line) : base(message, line, Name){{ }}\n\tprivate protected {k}(string message, int line, string childName): base(message, line, childName){{ }}\n}}" for k, v in exceptions_dict.items()
    )
}
"""
)
for fp, var in [("Lexer.Lex.cs", LexerFile), ("Token.cs", TokenFile), ("TokenTypes.cs", TTFile), ("ILexer.cs", ILexerFile), ("Exceptions.cs", ExceptionsFile)]:
    with open(fp, "w") as f:
        f.write(var)

print(1)