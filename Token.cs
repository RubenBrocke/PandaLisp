namespace PandaLisp
{
    public class Token
    {
        public TokenType TokenType { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }

        public Token(TokenType tokentype, string value, int line)
        {
            TokenType = tokentype;
            Value = value;
            Line = line;
        }

        public override string ToString()
        {
            return TokenType + " " + Value;
        }
    }

    public enum TokenType
    {
        LEFT_PAREN, RIGHT_PAREN, LEFT_BRACKET, RIGHT_BRACKET,

        IDENTIFIER, STRING, NUMBER, TRUE, FALSE,

        UNDERSCORE, FUN, NONE,

        EOF
    }
}