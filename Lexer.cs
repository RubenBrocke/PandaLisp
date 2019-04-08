using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace PandaLisp
{
    class Lexer
    {
        private int _index;
        private string _code;
        private List<Token> _tokenList = new List<Token>();
        private int _line = 1;
        private bool _isAtEnd
        {
            get
            {
                return _index >= _code.Length;
            }
        }

        private char[] _chars
        {
            get
            {
                return _code.ToCharArray();
            }
        }

        public Lexer(string code)
        {
            _code = code;
        }

        public List<Token> Scan()
        {

            while(!_isAtEnd)
            {
                //Check single character Tokens
                Char c = NextChar();

                switch (c)
                {
                    case '(': AddToken(TokenType.LEFT_PAREN, "("); break;
                    case ')': AddToken(TokenType.RIGHT_PAREN, ")"); break;
                    case '[': AddToken(TokenType.LEFT_BRACKET, "["); break;
                    case ']': AddToken(TokenType.RIGHT_BRACKET, "]"); break;                   
                    case ' ': break;
                    case '\r': break;
                    case '\t': break;
                    case '\n': _line++; break;
                    case '"': String(); break;
                    case '#': TakeWhile(n => n != '\n'); _line++; break;
                    default:
                        //Check for number
                        if (char.IsDigit(c))
                        {
                            string number = c.ToString();
                            number += new string(TakeWhile(n => char.IsDigit(n)));
                            if (!_isAtEnd && PeekChar() == '.' && char.IsDigit(PeekNextChar()))
                            {
                                number += NextChar();
                                number += new string(TakeWhile(n => char.IsDigit(n)));
                            }                            
                            //Create a new token
                            _tokenList.Add(new Token(TokenType.NUMBER, number, _line));
                        }
                        //Check for Identifier
                        else if (Regex.IsMatch(c.ToString(), Syntax.IdentRegex))
                        {
                            string IdString = new string(TakeWhile(n => Regex.IsMatch(n.ToString(), Syntax.IdentRegex)));
                            if (Syntax.Keywords.ContainsKey(c + IdString))
                                //It's a keyword
                                _tokenList.Add(new Token(Syntax.Keywords[c + IdString], c + IdString, _line));
                            else
                            //Create a new token
                            _tokenList.Add(new Token(TokenType.IDENTIFIER, c + IdString, _line));
                        }
                        else
                        {
                            new CompilerException(string.Format("Unknown character {0} at line {1}", c, _line));
                        }
                        break;
                }
            }

            _tokenList.Add(new Token(TokenType.EOF, "EOF", _line));
            return _tokenList;
        }

        private void String()
        {
            string newString = "\"";
            newString += new string(TakeWhile(n => n != '"'));

            if (_isAtEnd)
            {
                new CompilerException("String not terminated at line: " + _line);
                return;
            }

            _line += newString.Where(n => n == '\n').Count();

            newString += NextChar();

            AddToken(TokenType.STRING, newString);
        }

        private void AddToken(TokenType tokenType, string value)
        {
            _tokenList.Add(new Token(tokenType, value, _line));
            for (int i = 0; i < value.Length - 1; i++)
            {
                if (tokenType != TokenType.STRING)
                    NextChar();
            }
        }

        private Char NextChar()
        {
            return _code.ElementAt(_index++);
        }

        private Char PeekChar()
        {
            return _code.ElementAt(_index);
        }

        private Char PeekNextChar()
        {
            if (_index + 1 > _code.Length - 1) return '\0';
            return _code.ElementAt(_index + 1);
        }

        private Char[] TakeWhile(Func<char, bool> func)
        {
            List<Char> returnList = new List<char>();
            while (!_isAtEnd && func(PeekChar()))
            {
                returnList.Add(NextChar());
            }
            return returnList.ToArray();
        }

        public void PrintTokens()
        {
            foreach (Token item in _tokenList)
            {
                Console.WriteLine(item.TokenType + " : " + item.Value);
            }
        }
    }
}