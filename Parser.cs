using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaLisp
{
    public class Parser
    {
        private List<Token> _tokens;
        private int _index;
        public Root Root;

        public Parser(List<Token> inputTokens)
        {
            _tokens = inputTokens;
            _index = 0;
        }

        public void Parse()
        {
            Root = CreateRoot();
        }

        private Root CreateRoot()
        {
            List<Lisp> Lisps = new List<Lisp>();
            while(PeekToken().TokenType != TokenType.EOF)
            {
                Lisps.Add(CreateLisp());
            }
            return new Root(Lisps);
        }    

        private Lisp CreateLisp()
        {
            Function function = null;
            List<Primary> primaries = new List<Primary>();
            Match(TokenType.LEFT_PAREN);
            NextToken(); // skip left parentheses
            if (PeekToken().TokenType == TokenType.FUN)
            {
                NextToken(); // skip "fun"
                function = CreateFunction();
                primaries.Add(new Identifier(function.Identifier));
            }
            else 
            {
                while(PeekToken().TokenType != TokenType.RIGHT_PAREN)
                {
                    primaries.Add(CreatePrimary());
                }
                NextToken(); // Skip right parentheses
            }
            if (function != null)
            {
                return new Lisp(function);
            }
            else
            {
                return new Lisp(null, primaries);
            }
        } 

        private Function CreateFunction()
        {
            List<Pattern> patterns = new List<Pattern>();

            if (PeekToken().TokenType != TokenType.IDENTIFIER)
                throw new CompilerException("Need identifier to create function");

            string Ident = NextToken().Value;

            while (PeekToken().TokenType != TokenType.RIGHT_PAREN)
            {
                if (PeekToken().TokenType != TokenType.LEFT_BRACKET)
                    throw new CompilerException("Need at least 1 pattern to create function");
                
                NextToken(); // Skip left bracket
                Matcher matcher = CreateMatcher();
                NextToken(); // Skip right bracket
                if (PeekToken().TokenType != TokenType.LEFT_PAREN)
                    throw new CompilerException("Need Lisp after matcher");

                Lisp result = CreateLisp();

                patterns.Add(new Pattern(matcher, result));
            }
            
            NextToken(); // Skip right parentheses  

            return new Function(Ident, patterns);
        }

        private Matcher CreateMatcher()
        {
            List<Primary> arguments = new List<Primary>();
            if (PeekToken().TokenType == TokenType.UNDERSCORE)
            {
                return new Matcher();
            }
            else
            {
                while(PeekToken().TokenType != TokenType.RIGHT_BRACKET)
                {
                    arguments.Add(CreatePrimary());
                }
                return new Matcher(arguments);
            }
        }

        private Primary CreatePrimary()
        {
            TokenType type = PeekToken().TokenType;
            if (type == TokenType.TRUE) { return new Boolean(true); }
            if (type == TokenType.FALSE) { return new Boolean(false); }

            if (type == TokenType.NUMBER) { return new Number(Convert.ToInt32(NextToken().Value)); }
            if (type == TokenType.STRING) { return new String(NextToken().Value); }
            if (type == TokenType.LEFT_PAREN) { return CreateLisp(); }
            if (type == TokenType.IDENTIFIER) { return new Identifier(NextToken().Value); }

            throw new CompilerException("COULD NOT PARSE: " + NextToken().TokenType.ToString());
        }

        private Token NextToken()
        {
            return _tokens[_index++];
        }

        private Token PeekToken(int lookahead = 0)
        {
            if (_index < _tokens.Count - lookahead)
                return _tokens[_index + lookahead];
            else
                return new Token(TokenType.NONE, "", -1);
        }

        private Token[] TakeWhile(Func<Token, bool> func)
        {
            List<Token> returnList = new List<Token>();
            while (_index < _tokens.Count && func(PeekToken()))
            {
                returnList.Add(NextToken());
            }
            return returnList.ToArray();
        }

        private bool Match(params TokenType[] types)
        {
            return !types.Any(n => n == PeekToken().TokenType);
        }
    }

}