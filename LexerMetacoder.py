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
using {namespace_names_dict["LexerConstants"]}

internal static class {class_names_dict["Lexer"]} : {f'I{class_names_dict["Lexer"]}'}
{{
    public static List<{class_names_dict["Token"]}> Lex(string rawCode)
    {{
        int line = 0;
        int current = 0;
        int start;
        List<{class_names_dict["Token"]}> rv = new();
        //current points to the one we are currently processing(and therefore have not yet processed)
        while(current < rawCode.Length)
        {{
            start = current;
            switch(rawCode[current])
            {{
            {
                "".join(
                f"""
                case '{key.upper()}':
                    rv.Add(new({value_dict_to_TokenType_accessor(value)}, \"{key}\", line ));
                    current++;
                    break;
                """ for key, value in keywords_dict.items() if len(key) == 1
                )
                    #TODO: Add default(support for numbers and strings)
            } 
                default:
                    
            }}
        }}
    }}
}}
"""
)
print(LexerFile)