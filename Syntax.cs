using System.Collections.Generic;
using System.Text;

namespace PandaLisp
{
    public static class Syntax
    {
        public static string IdentRegex = @"[\+\-\*\/\%a-zA-Z_][a-zA-Z0-9_]*";

        public static Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
        {
            { "fun",    TokenType.FUN },
        };
    }
}